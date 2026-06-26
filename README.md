# Finance Monitor

Personal financial tracking web app. Allows tracking upcoming and past income and expenses, viewing charts related to those, and adding new ones.
It is a .NET rewrite of a fastApi version of the same app available at https://github.com/Gaudy035/fin_cal

## Features

- **Transaction history** - Viewing history of past income and expenses.
- **Transaction calendar** - List of upcoming transactions.
- **Automatic recurring transactions** - Adding recurring transactions via Quartz.NET background job.
- **Charts** - Donut expenses by category and bar income/spending balance charts generated with Chart.js.
- **JWT authentication** - User authentication using JWT.

## Tech Stack

- **FRONTEND:** React, TypeScript, Vite, TailwindCSS, Chart.js
- **BACKEND:** ASP.NET Core, Entity Framework Core, Quartz.NET
- **DATABASE:** PostgreSQL
- **DEPLOYMENT:** Docker, Nginx

## Prerequisites

**Docker:**

- [Docker](https://www.docker.com/)

**Local dev:**

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Bun](https://bun.sh)
- [PostgreSQL](https://www.postgresql.org/download/)

## Environment variables:

For app to run you need to create `.env` files in frontend folder and root directory of the project.
Each `.env` has its corresponding `.env.example` file.

### `.env` - Database (for Docker)

| Variable            | Description            |
| ------------------- | ---------------------- |
| `POSTGRES_DB`       | Database name          |
| `POSTGRES_USER`     | Database user          |
| `POSTGRES_PASSWORD` | Database user password |

### `frontend/.env` (local dev server) | `frontend/.env.production` (Docker)

| Variable       | Description     |
| -------------- | --------------- |
| `VITE_API_URL` | Backend API url |

Here you need to create two files, **.env** for local dev server and **.env.production** for Docker.

### `backend/appsettings.json` (local dev server) | `backend/appsettings.Development.json` (Docker)

Backend doesn't have standard `.env` file, instead it uses `appsettings.json` and `appsettings.Development.json`.
`appsettings.example.json` is provided as well.

For database connection you need to provide a connection string

```json
"ConnectionStrings": {
    "DbConnection": "Host=<HOST>; Port=<PORT>; Database=<DB>; Username=<DB_USER>; Password=<PASS>"
},
```

You need to replace the variables in <>

| Variable | Description                                       |
| -------- | ------------------------------------------------- |
| HOST     | Host where the database server runs               |
| PORT     | Port on which database server runs                |
| DB       | Name of the database used by app                  |
| DB_USER  | User that backend will use to connect to database |
| PASS     | Database user password                            |

Besides database variables you also need to provide:

```json
  "Cors": {
    "AllowedOrigins": ["<ORIGIN_URL>"]
  },
  "Jwt": {
    "Key": "<JWT_KEY>"
  }
```

| Variable   | Description                                        |
| ---------- | -------------------------------------------------- |
| JWT_KEY    | Secret key used by JWT, minimum 32 characters long |
| ORIGIN_URL | URL of frontend                                    |

## Starting

Before starting make sure files containing environment variables exist and are filled correctly according to this guide and `.example` files,
Also make sure that prerequisites are installed on your computer.

### Docker

When starting for the first time run:

```bash
docker-compose up --build
```

On later start-ups run

```bash
docker-compose up
```

To stop the app run

```bash
docker-compose down
```

## Local dev server

### DATABASE

Make sure PostgreSQL server is running, then create the database and run the setup scripts:

```bash
psql -U postgres -c "CREATE DATABASE fin_calc_db;"
psql -U postgres -d fin_calc_db -f db/skrypt.sql
```

You can either use the provided database user from `db/user.sql`:

```bash
psql -U postgres -d fin_calc_db -f db/user.sql
```

Or create your own user and update the connection string in `appsettings.json` accordingly.

### BACKEND

Inside `backend` folder run:

```bash
dotnet watch
```

### FRONTEND

Inside `frontend` folder install dependencies using:

```bash
bun install
```

And start the server using:

```bash
bun dev
```

## Project structure

```bash
fin_cal/
├── backend         # .NET Backend
├── frontend        # React Frontend
├── db              # Database SQL scripts
└── docker-compose.yaml
```
