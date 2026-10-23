"""Run: python -m unittest discover -s deploy -p test_manage.py"""
import argparse
import base64
import contextlib
import io
from pathlib import Path
import tempfile
import unittest

import manage


def valid_config():
    return {
        "name": "cyberbackup-test",
        "services": {
            "backend": {"build": {"args": {"BUILD_CONFIGURATION": "Release"}}, "environment": {
                "ASPNETCORE_ENVIRONMENT": "Production",
                "PostgresOptions__ConnectionString": "Host=postgres;Database=test;Username=test;Password=test;",
                "JwtOptions__SigningKey": "a" * 32,
                "PasswordHashOptions__Pepper": base64.b64encode(b"b" * 32).decode(),
                "PasswordHashOptions__WorkFactor": "12",
                "JwtOptions__AccessTokenLifetimeMinutes": "15",
                "RefreshTokenOptions__LifetimeDays": "7",
                "RefreshTokenOptions__TokenBytes": "64",
                "SuperAdminOptions__Email": "audit@example.com",
                "SuperAdminOptions__Password": "private-audit-password",
                "SuperAdminOptions__FullName": "Audit Administrator",
                "MinioOptions__Endpoint": "minio:9000",
                "MinioOptions__AccessKey": "audit-minio",
                "MinioOptions__SecretKey": "private-minio-password",
                "MinioOptions__BucketName": "cyberbackup",
                "CyberCorsOptions__Origins__0": "https://app.example.com",
            }},
            "frontend": {"build": {"args": {"VITE_API_BASE_URL": "https://api.example.com"}}},
            "postgres": {"environment": {"POSTGRES_USER": "audit", "POSTGRES_PASSWORD": "private-db-password", "POSTGRES_DB": "audit"}},
            "minio": {"environment": {"MINIO_ROOT_USER": "audit-minio", "MINIO_ROOT_PASSWORD": "private-minio-password"}},
        },
        "volumes": {"postgres_data": {"name": "test_postgres"}, "minio_data": {"name": "test_minio"}},
    }


class PreflightTests(unittest.TestCase):
    def setUp(self):
        self.config = valid_config()

    def test_valid_configuration(self):
        self.assertEqual(manage.validate_config(self.config, "prod"), [])

    def test_missing_and_placeholder_values_rejected_without_disclosing_values(self):
        self.config["services"]["backend"]["environment"]["JwtOptions__SigningKey"] = "change-me-do-not-print-this-secret"
        errors = manage.validate_config(self.config, "prod")
        self.assertTrue(any("JwtOptions__SigningKey" in error for error in errors))
        self.assertNotIn("do-not-print-this-secret", str(errors))

    def test_invalid_pepper(self):
        self.config["services"]["backend"]["environment"]["PasswordHashOptions__Pepper"] = "not-base64"
        self.assertTrue(any("Base64" in error for error in manage.validate_config(self.config, "prod")))

    def test_short_pepper(self):
        self.config["services"]["backend"]["environment"]["PasswordHashOptions__Pepper"] = "YQ=="
        self.assertTrue(any("32 decoded bytes" in error for error in manage.validate_config(self.config, "prod")))

    def test_mixed_content_rejected(self):
        self.config["services"]["frontend"]["build"]["args"]["VITE_API_BASE_URL"] = "http://api.example.com"
        self.assertTrue(any("HTTPS API" in error for error in manage.validate_config(self.config, "prod")))

    def test_wrong_cors_origin(self):
        self.config["services"]["backend"]["environment"]["CyberCorsOptions__Origins__0"] = "https://app.example.com/"
        self.assertTrue(any("origin without a path" in error for error in manage.validate_config(self.config, "prod")))

    def test_shared_volume_rejected(self):
        self.config["volumes"]["minio_data"]["name"] = "test_postgres"
        self.assertTrue(any("different volumes" in error for error in manage.validate_config(self.config, "prod")))

    def test_postgres_connection_string_delimiter_rejected(self):
        self.config["services"]["postgres"]["environment"]["POSTGRES_PASSWORD"] = "secret;Database=other"
        errors = manage.validate_config(self.config, "prod")
        self.assertTrue(any("POSTGRES_PASSWORD" in error for error in errors))
        self.assertNotIn("secret;Database=other", str(errors))

    def test_init_generates_independent_secrets_and_never_overwrites(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / ".env.production.local"
            args = argparse.Namespace(env_file=str(path), environment="prod")
            with contextlib.redirect_stdout(io.StringIO()) as output:
                manage.initialize(args)
            initial = path.read_text(encoding="utf-8")
            values = dict(line.split("=", 1) for line in initial.splitlines() if line and not line.startswith("#"))
            self.assertGreaterEqual(len(base64.b64decode(values["PASSWORD_HASH_PEPPER"])), 32)
            self.assertNotEqual(values["JWT_SIGNING_KEY"], values["PASSWORD_HASH_PEPPER"])
            self.assertNotIn(values["POSTGRES_PASSWORD"], output.getvalue())
            with self.assertRaises(manage.PreflightError):
                manage.initialize(args)
            self.assertEqual(initial, path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
