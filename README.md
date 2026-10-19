# CyberBackup

React/Vite, ASP.NET Core API, PostgreSQL и MinIO. Инструкция обновлена 19.10.2026 после проверки Docker-сборок и запуска dev/prod на отдельных локальных данных. Исходники frontend в этих исправлениях не менялись.

Локальные отчёты и сценарии аудита находятся в `docs/audit/`. Каталог `docs` не входит в Git и Docker-контекст.

## Требования

Команды выполняются из корня репозитория. Нужны Docker Engine с Linux-контейнерами, Docker Compose v2 с поддержкой `up --wait`, Python 3.10+ и доступ к реестрам образов, NuGet и npm при первой сборке. На Windows используется `py`; на Linux/macOS замените его на `python3`.

```powershell
docker version
docker compose version
py --version
```

`docker version` должен показать Client и Server. При HTTP 500 / `no route to host` сначала восстановите Docker. Скрипт прекращает предварительную проверку после 30 секунд ожидания Docker.

## Development

```powershell
py deploy/manage.py init --environment dev
py deploy/manage.py check --environment dev
py deploy/manage.py up --environment dev
py deploy/manage.py status --environment dev
```

`init` создаёт `deploy/.env.dev.local` и отказывается перезаписывать существующий файл. Этот шаг необязателен для dev: без локального файла используется `.env.dev.example`. В нём можно изменить порты и адреса браузера. Скрипт проверяет настройки и порты, собирает образы и ждёт состояния healthy у всех сервисов.

| Компонент | Адрес по умолчанию |
| --- | --- |
| Frontend | http://localhost:5173 |
| API readiness | http://localhost:15000/health/ready |
| Swagger | http://localhost:15000/swagger/index.html |
| PostgreSQL | localhost:15432 |
| MinIO API / консоль | http://localhost:19000 / http://localhost:19001 |

Порты dev опубликованы на `127.0.0.1`. Начальный вход: `superadmin@example.com` / `admin123`. Dev использует известные тестовые пароли и pepper; это локальный стенд. Backend автоматически применяет миграции и создаёт администратора. В сборку dev передаётся `BUILD_CONFIGURATION=Debug`, prod — `Release`.

Frontend использует hot reload и `npm ci` по lockfile при старте. После изменения backend повторите `up` для пересборки. Диагностика и остановка:

```powershell
py deploy/manage.py logs --environment dev --follow
py deploy/manage.py down --environment dev
```

Для доступа с другого компьютера задайте `DEV_BIND_IP`, `DEV_API_BASE_URL` и `DEV_FRONTEND_ORIGIN`. Последние два значения — адреса, доступные браузеру, со схемой и портом, без завершающего `/`. `localhost` в браузере указывает на компьютер пользователя.

## Production: новая установка

Для существующей установки сначала выполните раздел о сохранении данных ниже.

```powershell
py deploy/manage.py init --environment prod
```

Создаётся `deploy/.env.production.local` с независимыми случайными паролями PostgreSQL, MinIO и администратора, JWT-ключом и Base64 pepper. Секреты не выводятся в консоль; существующий файл не перезаписывается. Храните файл вне Git и ограничьте доступ средствами ОС.

Перед запуском проверьте в файле:

- `SUPERADMIN_EMAIL` — адрес администратора; пароль находится в `SUPERADMIN_PASSWORD`.
- `CYBER_CORS_ORIGIN` — origin frontend, например `https://cyber.example.com`.
- `VITE_API_BASE_URL` — адрес API для браузера, например `https://api.example.com`. Для HTTPS frontend требуется HTTPS API.
- `BACKEND_PORT`, `FRONTEND_PORT`, `MINIO_API_PORT`, `MINIO_CONSOLE_PORT` — свободные порты и интерфейсы публикации.

Шаблон содержит адреса локальной проверки: frontend `http://localhost`, API `http://localhost:5000`. API и MinIO опубликованы только на loopback; PostgreSQL наружу не публикуется. Для публичного сайта нужен внешний reverse proxy с TLS и поддержкой WebSocket для `/notification-hub`. Текущий frontend nginx обслуживает статику по HTTP и не проксирует API. Настройка публичного proxy и оптимизация frontend остаются отдельной работой.

```powershell
py deploy/manage.py check --environment prod
py deploy/manage.py up --environment prod
py deploy/manage.py status --environment prod
py deploy/manage.py logs --environment prod
```

Предварительная проверка отклоняет незаполненные шаблонные значения, короткий JWT-ключ, неверный Base64 pepper, недопустимые адреса/CORS, mixed content, конфликты портов и отсутствующие явно выбранные тома. `check` не запускает контейнеры. На удалённом Docker host проверка локальных портов пропускается с сообщением; проверяйте их на сервере.

`up` включает сборку и `--wait`; ошибка готовности возвращает ненулевой код. Для повторного запуска готовых образов используйте `--no-build`. Изменение `VITE_API_BASE_URL` требует пересборки frontend: Vite встраивает адрес в JavaScript. Swagger в Production отключён.

## Обновление существующей установки и данные

Dev и prod имеют разные проекты: `cyberbackup-dev` и `cyberbackup-prod`, с отдельными сетями и именованными томами. Глобальные `container_name` удалены. Для дополнительных стендов используйте `--project-name`, отдельные порты и `--env-file`. Во всех командах одного стенда сохраняйте имя проекта и env-файл.

Старый Compose мог использовать проект `deploy` и тома `deploy_postgres_data` / `deploy_minio_data`. Смена проекта без сопоставления томов создаст пустое хранилище. Перед обновлением:

1. Определите фактические тома через `docker volume ls` и mounts существующих контейнеров. Сохраните env, версии образов и конфигурацию выпуска.
2. Сделайте внешние резервные копии PostgreSQL и MinIO и проверьте восстановление отдельно. Сохранение Docker-томов не заменяет backup.
3. Сохраните действующие пароли, JWT-ключ и pepper. Не генерируйте новый env поверх существующего.
4. Остановите старый стек без удаления томов. Два экземпляра PostgreSQL не должны одновременно писать в один том.
5. В production env задайте **оба** точных существующих имени:

```dotenv
POSTGRES_VOLUME_NAME=deploy_postgres_data
MINIO_VOLUME_NAME=deploy_minio_data
```

`manage.py` автоматически подключит `docker-compose.existing-volumes.yml`. Тома объявлены external: отсутствующий том вызывает ошибку вместо создания пустого. Имена выше — пример; используйте фактические. Выполните `check`, затем `up`; проверьте вход и скачивание прежнего отчёта.

Обычный `down` сохраняет данные. **Не используйте `down -v` при обновлении.** Замена pepper нарушает проверку прежних паролей. Изменение `POSTGRES_PASSWORD` в env не меняет пароль роли в созданной БД; seed не меняет пароль существующего администратора. Автоматический rollback миграций и внешние backup jobs этим изменением не добавлены: для действующего сервера нужен проверенный порядок восстановления.

```powershell
py deploy/manage.py down --environment prod
py deploy/manage.py up --environment prod --no-build
```

## Готовность и журналы

| Endpoint | Проверка | При отказе PostgreSQL / MinIO |
| --- | --- | --- |
| `/health/live` | Работающий HTTP-процесс | 200, пока процесс отвечает |
| `/health/ready` | Подключение к БД и `SELECT 1`; аутентифицированный запрос MinIO | 503 с именем нездоровой зависимости |
| `/health` | Совместимый адрес readiness | 503 |

Readiness не возвращает строки подключения, секреты или исключения. MinIO проверяется через `ListBuckets`, требующий права `ListAllMyBuckets`; текущие Compose используют root credentials. При переходе на отдельного S3-пользователя учтите это право. Эти проверки не контролируют свободный диск и сохранность файлов.

Backend Docker healthcheck обращается к readiness; frontend ждёт healthy backend перед стартом. После возврата зависимостей backend восстанавливается без перезапуска. Статус `unhealthy` сам по себе не перезапускает контейнер: `restart: unless-stopped` действует при завершении процесса. Для эксплуатации подключите внешнюю проверку `/health/ready` и алерты.

Backend маскирует `access_token`, `refresh_token`, `Authorization` и Bearer-значения в структурированных свойствах и консольном выводе. Для Compose-сервисов включена ротация Docker `local`: до трёх файлов по 10 МБ до учёта сжатия. Это не очищает старые журналы. Полный `docker inspect`, разрешённый `compose config` и env-файлы содержат секреты; не публикуйте их. Внешний proxy должен иметь собственные правила журналирования.

## Ручные Compose-команды

Скрипт использует стандартный Compose. При прямом запуске выполните предварительную проверку отдельно и передавайте нужный env-файл:

```powershell
docker compose --env-file deploy/.env.dev.example -f deploy/docker-compose.dev.yml up --build -d --wait
docker compose --env-file deploy/.env.production.local -f deploy/docker-compose.prod.yml up --build -d --wait
```

При существующих томах вручную добавляйте `-f deploy/docker-compose.existing-volumes.yml` после основного файла во **все** команды. Прямой Compose не выполняет семантическую проверку `manage.py`.

## Локальная сборка и проверки

Backend требует .NET SDK 10. Проверено с SDK `10.0.300`, Node `24.13.1`, Docker Engine `27.3.1` и Compose `2.29.7`.

```powershell
dotnet build backend/Apps/CyberBackup/CyberBackup.sln -c Release
dotnet run --project backend/Tests/Infrastructure.Logging.Checks -c Release
py -m unittest discover -s deploy -p test_manage.py
```

Для API без Docker скопируйте `backend/Apps/CyberBackup/Api/appsettings.Development.template.json` в соседний `appsettings.Development.json`, сохранив существующий файл, если он есть. Шаблон настроен на PostgreSQL `15432`, MinIO `19000`, frontend `5173`; launch profile `http` использует API `15000`. Запустите зависимости отдельно; контейнер backend на том же порту должен быть остановлен.

```powershell
docker compose --env-file deploy/.env.dev.example -f deploy/docker-compose.dev.yml up -d postgres minio
dotnet run --project backend/Apps/CyberBackup/Api --launch-profile http
```

Локальные сценарии аудита охватывают 22 проверки API, 7 браузерных проверок и испытания отказов для каждого окружения. Они создают синтетические данные и предназначены для отдельных локальных стендов.

Уязвимые NuGet/npm-зависимости, фиксация версий SDK/образов, HTTPS, внешние backup/alert jobs и ошибки frontend требуют отдельных работ. Docker-сборка существующего frontend проходит, но не включает TypeScript/lint: исходный аудит выявил 11 ошибок TypeScript и 18 ошибок ESLint.
