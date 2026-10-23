# CyberBackup

## Сборка окружения

Проект поддерживает два режима сборки:

* `Development` — для разработки
* `Release` — для production

### Настройка

Перед сборкой необходимо указать конфигурацию в файле:

```
CyberBackup/deploy/.env
```

Пример:

```
BUILD_CONFIGURATION=Development
```

или

```
BUILD_CONFIGURATION=Release
```

## 🚀 Запуск проекта

### 📦 Development

Запуск всех сервисов с hot reload для frontend:

```bash
docker compose -f deploy/docker-compose.dev.yml up --build -d
```

После запуска:

* Frontend: http://localhost:5173
* CyberBackup API: http://localhost:5000
* Postgres: localhost:5432

---

### 🏭 Production

Сборка и запуск контейнеров в фоновом режиме:

```bash
docker compose -f deploy/docker-compose.prod.yml up --build -d
```

После запуска:

* Frontend (nginx): http://localhost
* CyberBackup API: http://localhost:5000

---

### 🛑 Остановка

```bash
docker compose -f deploy/docker-compose.dev.yml down
```

или для production:

```bash
docker compose -f deploy/docker-compose.prod.yml down
```

---

### 🔄 Пересборка

Если нужно пересобрать контейнеры:

```bash
docker compose -f deploy/docker-compose.dev.yml up --build --force-recreate
```

---

### 🖥 Dev-сервер и CI/CD

`.github/workflows/ci.yml` на каждый push и PR собирает backend, запускает тесты и сборку frontend. `.github/workflows/deploy-dev.yml` при push в `main` (или вручную через Run workflow) прогоняет CI, по SSH переключает git-checkout на сервере на нужный коммит, записывает `deploy/.env.dev.local` из окружения GitHub `dev` и поднимает `docker-compose.dev.yml` под именем проекта `cyberbackup-dev`. Домена нет: frontend и API доступны по IP, PostgreSQL и MinIO опубликованы только на loopback сервера.

Настройки задаются в Settings → Environments → `dev`:

* Variables: `DEV_HOST` (внешний IP), `DEV_SSH_USER`, `DEV_SSH_PORT`, `SUPERADMIN_EMAIL`; необязательные `DEV_APP_DIR` (по умолчанию `CyberBackup` от домашнего каталога), `DEV_BACKEND_PORT` (`15000`), `DEV_FRONTEND_PORT` (`5173`).
* Secrets: `DEV_SSH_PRIVATE_KEY`, `DEV_SSH_KNOWN_HOSTS` (`ssh-keyscan -p <порт> <IP>`), `POSTGRES_PASSWORD`, `MINIO_ROOT_USER`, `MINIO_ROOT_PASSWORD`, `SUPERADMIN_PASSWORD`, `JWT_SIGNING_KEY`, `PASSWORD_HASH_PEPPER` (Base64 от 32 байт).

Смена `POSTGRES_PASSWORD` или pepper после первого запуска не применяется к уже созданным данным. Порты frontend и API должны быть проброшены на сервер с тем же номером снаружи.

Состояние на сервере:

```bash
cd ~/CyberBackup
docker compose -p cyberbackup-dev --env-file deploy/.env.dev.local -f deploy/docker-compose.dev.yml ps
docker compose -p cyberbackup-dev --env-file deploy/.env.dev.local -f deploy/docker-compose.dev.yml logs -f backend
```

---

### 📌 Примечание

* В development используется `node` контейнер с hot reload
* В production frontend собирается и отдается через `nginx`
* База данных инициализируется автоматически
