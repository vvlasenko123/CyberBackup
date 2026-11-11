# CyberBackup

## Сборка окружения

Проект поддерживает два режима сборки:

* `Debug` — для разработки
* `Release` — для production

### Настройка

Для запуска нужен Docker Compose v2.20.2+ с Docker Engine 25+. Compose выбирает режим по файлу окружения. `BUILD_CONFIGURATION` можно переопределить через `--env-file`.

## 🚀 Запуск проекта

### 📦 Development

Запуск всех сервисов с hot reload для frontend:

```bash
docker compose -f deploy/docker-compose.dev.yml up --build -d --wait
```

После запуска:

* Frontend: http://localhost:5173
* CyberBackup API: http://localhost:5000
* Postgres: localhost:5432

`npm ci` выполняется при сборке frontend. После изменения `package-lock.json` повторите `up --build`. Порты и адреса можно задать через `--env-file deploy/.env.dev.local` по примеру `deploy/.env.dev.example`.

---

### 🏭 Production

Скопируйте `deploy/.env.prod.example` в `deploy/.env.production.local`, задайте адреса и замените заглушки в секретах. Для обновления сохраните действующие секреты и имя Compose-проекта, чтобы использовать существующие тома PostgreSQL и MinIO. Если стенд уже запущен под `cyberbackup-prod`, добавляйте `-p cyberbackup-prod` во все команды. Перед обновлением проверьте восстановление из резервной копии.

Если установка использует внешние тома, укажите их фактические имена в `POSTGRES_VOLUME_NAME` и `MINIO_VOLUME_NAME` и добавляйте `-f deploy/docker-compose.existing-volumes.yml` после основного файла во всех командах.

```bash
docker compose --env-file deploy/.env.production.local -f deploy/docker-compose.prod.yml up --build -d --wait
```

После запуска:

* Frontend (nginx): http://localhost
* CyberBackup API: http://localhost:5000

Для публичной установки настройте HTTPS и проксирование WebSocket для `/notification-hub`. Задайте адрес frontend в `CYBER_CORS_ORIGIN`, адрес API в `VITE_API_BASE_URL`. Для одновременного запуска dev и prod укажите разные имена через `-p` и непересекающиеся порты через env-файлы.

---

### 🛑 Остановка

```bash
docker compose -f deploy/docker-compose.dev.yml down
```

или для production:

```bash
docker compose --env-file deploy/.env.production.local -f deploy/docker-compose.prod.yml down
```

---

### 🔄 Пересборка

Если нужно пересобрать контейнеры:

```bash
docker compose -f deploy/docker-compose.dev.yml up --build --force-recreate
```

---

### 📌 Примечание

* В development используется `node` контейнер с hot reload
* В production frontend собирается и отдается через `nginx`
* База данных инициализируется автоматически

### Dev-сервер и CI/CD

`.github/workflows/ci.yml` проверяет сборку, тесты frontend, журналирование и конфигурацию Compose. `.github/workflows/deploy-dev.yml` после обновления `main` или ручного запуска прогоняет CI, готовит env-файл на сервере и запускает Compose по SSH с именем `cyberbackup-dev`. Порты API и frontend по умолчанию `15000` и `5173`. PostgreSQL и MinIO доступны только с самого сервера.

| Тип | Настройки окружения GitHub `dev` |
| --- | --- |
| Variables | `DEV_HOST`, `DEV_SSH_USER`, `SUPERADMIN_EMAIL` |
| Variables (необязательные) | `DEV_SSH_PORT`, `DEV_APP_DIR`, `DEV_BACKEND_PORT`, `DEV_FRONTEND_PORT`, `SUPERADMIN_FULL_NAME`, `POSTGRES_USER`, `POSTGRES_DB`, `MINIO_BUCKET_NAME` |
| Secrets | `DEV_SSH_PRIVATE_KEY`, `DEV_SSH_KNOWN_HOSTS`, `POSTGRES_PASSWORD`, `MINIO_ROOT_USER`, `MINIO_ROOT_PASSWORD`, `JWT_SIGNING_KEY`, `PASSWORD_HASH_PEPPER`, `SUPERADMIN_PASSWORD` |

Приватный SSH-ключ должен соответствовать ключу в `authorized_keys` на сервере. В `DEV_SSH_KNOWN_HOSTS` укажите проверенный ключ сервера. Используйте независимые пароли, JWT-ключ не короче 32 байт и pepper из 32 случайных байт в Base64. Значения не должны содержать одинарных кавычек и переводов строк.
