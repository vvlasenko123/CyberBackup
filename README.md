# CyberBackup

## Сборка окружения

Проект поддерживает два режима сборки:

* `Development` — для разработки
* `Release` — для production

### Настройка

Перед сборкой необходимо указать конфигурацию в файле:

```
CyberBackup/.env
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
docker compose -f deploy/docker-compose.dev.yml up --build
```

После запуска:

* Frontend: http://localhost:5173
* Auth API: http://localhost:8000
* Lab Orchestrator API: http://localhost:6666
* Postgres: localhost:5432

---

### 🏭 Production

Сборка и запуск контейнеров в фоновом режиме:

```bash
docker compose -f deploy/docker-compose.prod.yml up --build -d
```

После запуска:

* Frontend (nginx): http://localhost
* Auth API: http://localhost:8000
* Lab Orchestrator API: http://localhost:6666

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

### 📌 Примечание

* В development используется `node` контейнер с hot reload
* В production frontend собирается и отдается через `nginx`
* База данных инициализируется автоматически
