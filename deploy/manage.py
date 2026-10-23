"""Docker Compose lifecycle with preflight checks. Python 3.10+, standard library only.

Never prints resolved environment values. `init` never overwrites an existing env file.
`down` deliberately preserves volumes. Frontend application code is not managed here.
"""
from __future__ import annotations

import argparse
import base64
import binascii
import json
import os
from pathlib import Path
import re
import secrets
import socket
import subprocess
import sys
from urllib.parse import urlparse

ROOT = Path(__file__).resolve().parent.parent
DEPLOY = ROOT / "deploy"


class PreflightError(Exception):
    """An actionable configuration error that contains no secret values."""


def captured(command: list[str]) -> str:
    try:
        result = subprocess.run(command, cwd=ROOT, capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=30)
    except FileNotFoundError as error:
        raise PreflightError(f"Executable not found: {command[0]}") from error
    except subprocess.TimeoutExpired as error:
        raise PreflightError("Docker did not respond within 30 seconds. Check Docker Engine before retrying.") from error
    if result.returncode:
        # Compose errors may contain input from the env file. Report variable names only.
        missing = re.search(r"required variable ([A-Z][A-Z0-9_]*)|\b([A-Z][A-Z0-9_]*) is required", result.stderr)
        suffix = f" Required variable: {next(group for group in missing.groups() if group)}." if missing else ""
        raise PreflightError(f"Docker command failed (exit {result.returncode}). Check Docker Engine, Compose and env syntax.{suffix}")
    return result.stdout


def require_real(value: str, name: str, errors: list[str]) -> None:
    if not value.strip() or value.lower().startswith("change-me"):
        errors.append(f"{name}: set a non-placeholder value")


def check_origin(value: str, name: str, errors: list[str], *, origin_only: bool) -> None:
    parsed = urlparse(value)
    try:
        parsed.port
    except ValueError:
        errors.append(f"{name}: invalid URL port")
        return
    if parsed.scheme not in ("http", "https") or not parsed.hostname or parsed.username or parsed.password:
        errors.append(f"{name}: use an absolute http(s) URL without credentials")
    if parsed.query or parsed.fragment or (origin_only and parsed.path not in ("",)):
        errors.append(f"{name}: specify an origin without a path, query or fragment")


def validate_config(config: dict, environment: str) -> list[str]:
    errors: list[str] = []
    services = config.get("services", {})
    for name in ("postgres", "minio", "backend", "frontend"):
        if name not in services:
            errors.append(f"Missing service: {name}")
    if errors:
        return errors
    backend = services["backend"].get("environment", {})
    for name in ("PostgresOptions__ConnectionString", "JwtOptions__SigningKey", "PasswordHashOptions__Pepper",
                 "SuperAdminOptions__Email", "SuperAdminOptions__Password", "SuperAdminOptions__FullName",
                 "MinioOptions__Endpoint", "MinioOptions__AccessKey", "MinioOptions__SecretKey", "MinioOptions__BucketName"):
        require_real(str(backend.get(name) or ""), name, errors)
    if len(str(backend.get("JwtOptions__SigningKey", "")).encode("utf-8")) < 32:
        errors.append("JwtOptions__SigningKey: at least 32 UTF-8 bytes required")
    try:
        pepper = base64.b64decode(str(backend.get("PasswordHashOptions__Pepper", "")), validate=True)
        if len(pepper) < 32:
            errors.append("PasswordHashOptions__Pepper: at least 32 decoded bytes required")
    except (binascii.Error, ValueError):
        errors.append("PasswordHashOptions__Pepper: valid Base64 required")
    for name, minimum, maximum in (("PasswordHashOptions__WorkFactor", 10, 31),
                                    ("JwtOptions__AccessTokenLifetimeMinutes", 1, None),
                                    ("RefreshTokenOptions__LifetimeDays", 1, None),
                                    ("RefreshTokenOptions__TokenBytes", 32, None)):
        try:
            value = int(backend.get(name, ""))
            if value < minimum or (maximum is not None and value > maximum):
                raise ValueError
        except (ValueError, TypeError):
            errors.append(f"{name}: invalid integer or out-of-range value")
    if not re.fullmatch(r"[^\s@]+@[^\s@]+\.[^\s@]+", str(backend.get("SuperAdminOptions__Email", ""))):
        errors.append("SuperAdminOptions__Email: valid email required")
    expected_environment = "Production" if environment == "prod" else "Development"
    if backend.get("ASPNETCORE_ENVIRONMENT") != expected_environment:
        errors.append("ASPNETCORE_ENVIRONMENT does not match the selected environment")
    # This Compose runs MinIO over HTTP inside the private Docker network.
    if str(backend.get("MinioOptions__UseSsl", "false")).lower() != "false":
        errors.append("MinioOptions__UseSsl: bundled MinIO uses HTTP; configure server-side TLS before enabling SSL")
    origin = str(backend.get("CyberCorsOptions__Origins__0", ""))
    check_origin(origin, "CyberCorsOptions__Origins__0", errors, origin_only=True)
    frontend = services["frontend"]
    api_url = frontend.get("build", {}).get("args", {}).get("VITE_API_BASE_URL") if environment == "prod" else frontend.get("environment", {}).get("VITE_API_BASE_URL")
    check_origin(str(api_url or ""), "VITE_API_BASE_URL", errors, origin_only=True)
    if urlparse(origin).scheme == "https" and urlparse(str(api_url or "")).scheme != "https":
        errors.append("VITE_API_BASE_URL: HTTPS frontend requires HTTPS API")
    configuration = services["backend"].get("build", {}).get("args", {}).get("BUILD_CONFIGURATION")
    if configuration not in ("Debug", "Release"):
        errors.append("BUILD_CONFIGURATION: use Debug or Release")
    # Catch accidental shared/external data mounts when both variables name the same volume.
    volume_names = [volume.get("name") for volume in config.get("volumes", {}).values() if volume.get("name")]
    if len(volume_names) != len(set(volume_names)):
        errors.append("PostgreSQL and MinIO must use different volumes")
    if any(not re.fullmatch(r"[a-zA-Z0-9][a-zA-Z0-9_.-]*", name) for name in volume_names):
        errors.append("Data volume names must contain only letters, digits, '_', '.' or '-'")
    if environment == "prod":
        for service, names in (("postgres", ("POSTGRES_USER", "POSTGRES_PASSWORD", "POSTGRES_DB")),
                               ("minio", ("MINIO_ROOT_USER", "MINIO_ROOT_PASSWORD"))):
            for name in names:
                require_real(str(services[service].get("environment", {}).get(name) or ""), name, errors)
        postgres_env = services["postgres"].get("environment", {})
        # These values are embedded in an Npgsql connection string by Compose.
        for name in ("POSTGRES_USER", "POSTGRES_PASSWORD", "POSTGRES_DB"):
            if any(character in str(postgres_env.get(name, "")) for character in (';', '"', "'", "\n", "\r")):
                errors.append(f"{name}: avoid connection-string delimiters; generated URL-safe values are supported")
    return errors


def env_value(path: Path, name: str) -> str:
    """Only used to select an explicit existing-volumes override; Compose resolves all settings."""
    if name in os.environ:
        return os.environ[name]
    for line in path.read_text(encoding="utf-8-sig").splitlines():
        match = re.match(rf"^\s*(?:export\s+)?{re.escape(name)}\s*=(.*)$", line)
        if match:
            return match.group(1).strip().strip("\"'")
    return ""


def compose_command(args: argparse.Namespace) -> tuple[list[str], Path]:
    env_file = Path(args.env_file).resolve() if args.env_file else DEPLOY / (
        ".env.production.local" if args.environment == "prod" else
        ".env.dev.local" if (DEPLOY / ".env.dev.local").is_file() else ".env.dev.example")
    if not env_file.is_file():
        raise PreflightError(f"Env file not found: {env_file}. Run init or copy the example first.")
    command = ["docker", "compose", "--env-file", str(env_file)]
    if args.project_name:
        if not re.fullmatch(r"[a-z0-9][a-z0-9_-]*", args.project_name):
            raise PreflightError("Project name must contain lowercase letters, digits, '_' or '-'")
        command += ["--project-name", args.project_name]
    command += ["-f", str(DEPLOY / f"docker-compose.{args.environment}.yml")]
    postgres_volume = env_value(env_file, "POSTGRES_VOLUME_NAME")
    minio_volume = env_value(env_file, "MINIO_VOLUME_NAME")
    if bool(postgres_volume) != bool(minio_volume):
        raise PreflightError("Set both POSTGRES_VOLUME_NAME and MINIO_VOLUME_NAME for an existing installation")
    if postgres_volume and minio_volume:
        command += ["-f", str(DEPLOY / "docker-compose.existing-volumes.yml")]
    return command, env_file


def check_volumes(config: dict) -> None:
    for volume in config.get("volumes", {}).values():
        if volume.get("external"):
            name = volume["name"]
            try:
                captured(["docker", "volume", "inspect", "--format", "{{.Name}}", name])
            except PreflightError as error:
                raise PreflightError("An explicitly selected existing data volume was not found. Check volume mapping; no volume was created.") from error


def check_ports(config: dict) -> None:
    # Inspect only host-port mappings, never full container configuration containing secrets.
    own_ports: set[tuple[str, int]] = set()
    ids = captured(["docker", "ps", "--filter", f"label=com.docker.compose.project={config['name']}", "--format", "{{.ID}}"])
    for container_id in ids.split():
        mappings = json.loads(captured(["docker", "inspect", "--format", "{{json .NetworkSettings.Ports}}", container_id])) or {}
        for bindings in mappings.values():
            for binding in bindings or []:
                own_ports.add((binding["HostIp"], int(binding["HostPort"])))
    # Socket probes run on this machine; do not claim to inspect ports of a remote Docker host.
    endpoint = os.environ.get("DOCKER_HOST")
    if not endpoint:
        endpoint = captured(["docker", "context", "inspect", "--format", "{{.Endpoints.docker.Host}}", captured(["docker", "context", "show"]).strip()]).strip()
    if not endpoint.startswith(("unix://", "npipe://")):
        print("Remote Docker host: local socket checks skipped; check remote port availability separately.")
        return
    checked: set[tuple[str, int]] = set()
    for service, values in config["services"].items():
        for binding in values.get("ports", []):
            if binding.get("protocol", "tcp") != "tcp":
                continue
            port = int(binding["published"])
            address = binding.get("host_ip") or "0.0.0.0"
            pair = (address, port)
            if any(old_port == port and (old_address == address or old_address in ("0.0.0.0", "::")
                                         or address in ("0.0.0.0", "::")) for old_address, old_port in checked):
                raise PreflightError(f"Duplicate published port: {service}, TCP {port}")
            checked.add(pair)
            if pair in own_ports:
                continue
            family = socket.AF_INET6 if ":" in address else socket.AF_INET
            with socket.socket(family, socket.SOCK_STREAM) as probe:
                try:
                    probe.bind((address, port))
                except OSError as error:
                    raise PreflightError(f"Host port unavailable: {service}, {address}:{port}. Stop the conflicting service or set another port.") from error


def initialize(args: argparse.Namespace) -> None:
    target = Path(args.env_file).resolve() if args.env_file else DEPLOY / (
        ".env.production.local" if args.environment == "prod" else ".env.dev.local")
    template = (DEPLOY / f".env.{args.environment}.example").read_text(encoding="utf-8-sig")
    if args.environment == "prod":
        replacements = {
            "POSTGRES_PASSWORD": secrets.token_urlsafe(32),
            "MINIO_ROOT_USER": "cyberbackup-admin",
            "MINIO_ROOT_PASSWORD": secrets.token_urlsafe(32),
            "JWT_SIGNING_KEY": base64.b64encode(secrets.token_bytes(32)).decode("ascii"),
            "PASSWORD_HASH_PEPPER": base64.b64encode(secrets.token_bytes(32)).decode("ascii"),
            "SUPERADMIN_PASSWORD": secrets.token_urlsafe(32),
        }
        for name, value in replacements.items():
            template = re.sub(rf"(?m)^{name}=.*$", lambda _: f"{name}={value}", template)
    try:
        descriptor = os.open(target, os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600)
    except FileExistsError as error:
        raise PreflightError("Env file already exists; initialization refused to overwrite it or rotate existing secrets.") from error
    with os.fdopen(descriptor, "w", encoding="utf-8", newline="\n") as output:
        output.write(template)
    print(f"Created {target}. Secret values omitted. Review browser addresses, ports and administrator email before up.")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=("init", "check", "up", "build", "down", "status", "logs"))
    parser.add_argument("--environment", choices=("dev", "prod"), default="dev")
    parser.add_argument("--env-file")
    parser.add_argument("--project-name")
    parser.add_argument("--no-build", action="store_true", help="Reuse existing images for up")
    parser.add_argument("--wait-timeout", type=int, default=180)
    parser.add_argument("--follow", action="store_true", help="Follow logs")
    args = parser.parse_args()
    if args.wait_timeout <= 0:
        parser.error("--wait-timeout must be positive")
    try:
        if args.action == "init":
            initialize(args)
            return 0
        command, _ = compose_command(args)
        if args.action in ("check", "build", "up"):
            config = json.loads(captured(command + ["config", "--format", "json"]))
            errors = validate_config(config, args.environment)
            if errors:
                raise PreflightError("Configuration invalid:\n- " + "\n- ".join(errors))
            captured(["docker", "info", "--format", "{{.ServerVersion}}"])
            check_volumes(config)
            if args.action in ("check", "up"):
                check_ports(config)
            print(f"Preflight OK: environment={args.environment}, project={config['name']}. Secret values omitted.", flush=True)
        if args.action == "check":
            return 0
        if args.action == "up":
            command += ["up", "--no-build" if args.no_build else "--build", "-d", "--wait", "--wait-timeout", str(args.wait_timeout)]
        elif args.action == "build":
            command += ["build"]
        elif args.action == "down":
            command += ["down"]
        elif args.action == "status":
            command += ["ps", "-a"]
        elif args.action == "logs":
            command += ["logs", "--tail", "100"] + (["--follow"] if args.follow else [])
        return subprocess.run(command, cwd=ROOT).returncode
    except (PreflightError, ValueError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
