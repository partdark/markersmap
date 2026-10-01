# Markers — интерактивная карта меток

Полноценное веб-приложение для отображения и управления метками на карте (Cesium + OpenStreetMap).
Проект состоит из бэкенда на ASP.NET Core, фронтенда на React и инфраструктурных сервисов
(PostgreSQL, pgAdmin, dufs), оркестрируемых через Docker Compose.

## Технологии

| Слой        | Технологии                                                                 |
|-------------|----------------------------------------------------------------------------|
| Фронтенд    | React 18, TypeScript, Vite 5, Cesium, Zustand, Axios, React Router        |
| Бэкенд      | ASP.NET Core (.NET), EF Core, JWT-аутентификация, Clean Architecture      |
| База данных | PostgreSQL                                                               |
| Файлы       | dufs (лёгкий файловый сервер для загрузки изображений)                    |
| Инфраструктура | Docker Compose, Nginx (reverse proxy + статика)                        |

## Сервисы и порты

| Сервис      | Контейнер         | Адрес            | Назначение                          |
|-------------|-------------------|------------------|-------------------------------------|
| Фронтенд    | `markers-frontend`| http://localhost:3001 | SPA + прокси `/api` на бэкенд  |
| Бэкенд API  | `markers-api`     | http://localhost:5223 | REST API (Swagger: `/swagger`) |
| PostgreSQL  | `markers-postgres`| localhost:5435   | База данных                        |
| pgAdmin     | `markers-pgadmin` | http://localhost:5050 | Веб-админка БД                |
| dufs        | `markers-dufs`    | http://localhost:5000 | Хостинг загруженных файлов     |

## Быстрый старт

Требуется установленный [Docker](https://www.docker.com/) с Docker Compose.

```bash
# 1. Скопировать пример переменных окружения
cp .env.example .env

# 2. Заполнить .env своими значениями (см. пример ниже)

# 3. Собрать и запустить все сервисы
docker compose up -d --build
```

После запуска:

- приложение доступно на http://localhost:3001
- API — http://localhost:5223
- pgAdmin — http://localhost:5050
- файловый сервер dufs — http://localhost:5000

Остановить: `docker compose down` (данные PostgreSQL сохранятся в volume `markersdb_data`).
Удалить вместе с данными: `docker compose down -v`.

## Переменные окружения (пример `.env`)

Скопируйте [`.env.example`](.env.example) в `.env` и укажите свои значения.
Файл `.env` игнорируется git — секреты в репозиторий не попадают.

```dotenv
# ---- PostgreSQL ----
POSTGRES_DB=markersdb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change-me

# ---- pgAdmin (web UI: http://localhost:5050) ----
PGADMIN_EMAIL=admin@example.com
PGADMIN_PASSWORD=change-me

# ---- dufs (file server: http://localhost:5000) ----
DUFS_USER=admin
DUFS_PASSWORD=change-me
FILE_UPLOAD_PATH=/app/uploads
FILE_BASE_URL=http://localhost:5000

# ---- JWT ----
JWT_KEY=change-me-to-a-long-random-secret
JWT_ISSUER=markersAPI
JWT_AUDIENCE=markersClient
JWT_REFRESH_TOKEN_DAYS=7

# ---- Admin seed account ----
ADMIN_SEED_EMAIL=admin
ADMIN_SEED_PASSWORD=change-me

# ---- Application ----
ASPNETCORE_ENVIRONMENT=Development
```

### Описание переменных

| Переменная                 | Обязательна | Описание                                                            |
|----------------------------|-------------|---------------------------------------------------------------------|
| `POSTGRES_DB`              | да          | Имя базы данных PostgreSQL                                         |
| `POSTGRES_USER`            | да          | Пользователь PostgreSQL                                            |
| `POSTGRES_PASSWORD`        | да          | Пароль пользователя PostgreSQL                                     |
| `PGADMIN_EMAIL`            | да          | Email для входа в pgAdmin                                          |
| `PGADMIN_PASSWORD`         | да          | Пароль для входа в pgAdmin                                         |
| `DUFS_USER`                | да          | Логин для записи (rw) в файловый сервер dufs                       |
| `DUFS_PASSWORD`            | да          | Пароль для записи в dufs                                           |
| `FILE_UPLOAD_PATH`         | нет         | Путь к каталогу загрузок внутри контейнера бэкенда (по умолчанию `/app/uploads`) |
| `FILE_BASE_URL`            | нет         | Базовый URL файлового сервера (используется в ответах API)         |
| `JWT_KEY`                  | да          | Секретный ключ подписи JWT (длинная случайная строка!)             |
| `JWT_ISSUER`               | нет         | Издатель токена (issuer)                                           |
| `JWT_AUDIENCE`             | нет         | Аудитория токена (audience)                                        |
| `JWT_REFRESH_TOKEN_DAYS`   | нет         | Срок жизни refresh-токена в днях (по умолчанию 7)                  |
| `ADMIN_SEED_EMAIL`         | нет         | Email/логин администратора, создаваемого при первом запуске        |
| `ADMIN_SEED_PASSWORD`      | нет         | Пароль администратора                                               |
| `ASPNETCORE_ENVIRONMENT`   | нет         | Окружение ASP.NET Core (`Development` / `Production`)               |

> ⚠️ Для продакшена обязательно замените `JWT_KEY`, пароли и секреты на длинные случайные значения.

## Структура проекта

```
├── docker-compose.yml        # Оркестрация всех сервисов
├── .env.example              # Пример переменных окружения
├── uploads/                  # Загруженные файлы (dufs + бэкенд), в git только .gitkeep
├── markersAPI/               # Бэкенд ASP.NET Core
│   ├── markersAPI/           #   Web API: контроллеры, Program.cs, Dockerfile
│   ├── Application/          #   Сервисы, DTO (use-case слой)
│   ├── Domain/               #   Сущности
│   └── Infrastructure/       #   EF Core, репозитории, миграции
└── frontend/                 # Фронтенд React + Vite + Cesium
    ├── src/                  #   Исходники приложения
    ├── Dockerfile            #   Multi-stage: node build -> nginx
    └── nginx.conf            #   Статика + прокси /api на бэкенд
```

## Локальная разработка без Docker

**Фронтенд** (нужен Node.js 20+):

```bash
cd frontend
npm ci
npm run dev        # запустит copy:cesium + vite dev server
```

API бэкенда при этом ожидается на `VITE_API_BASE` (по умолчанию `/api`).

**Бэкенд** (нужен .NET SDK):

```bash
cd markersAPI
dotnet restore
dotnet run --project markersAPI