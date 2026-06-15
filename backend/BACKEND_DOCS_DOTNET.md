# Backend Documentation — Monitor Finansowy (fin_cal) — .NET 8

This document describes the server layer of the **Monitor Finansowy** application rewritten in **.NET 8 (ASP.NET Core Web API)**. It covers the PostgreSQL database schema, data models, authentication, background scheduling, and the full REST API required by the existing React frontend. Following this document you can build a backend from scratch that is fully compatible with the original frontend and database.

> **Note:** The database schema and all JSON field names remain **identical** to the original FastAPI implementation. Only the server-side technology changes.

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Technology Stack](#2-technology-stack)
3. [Project Structure](#3-project-structure)
4. [Environment Configuration](#4-environment-configuration)
5. [PostgreSQL Database](#5-postgresql-database)
6. [Entity Models (EF Core)](#6-entity-models-ef-core)
7. [DTO Contracts (API Contract)](#7-dto-contracts-api-contract)
8. [Authentication & Authorization (JWT)](#8-authentication--authorization-jwt)
9. [Background Scheduler (IHostedService)](#9-background-scheduler-ihostedservice)
10. [API Reference — All Endpoints](#10-api-reference--all-endpoints)
11. [Frontend ↔ Backend Mapping](#11-frontend--backend-mapping)
12. [HTTP Error Handling](#12-http-error-handling)
13. [Docker & Nginx Integration](#13-docker--nginx-integration)
14. [Implementation Checklist](#14-implementation-checklist)

---

## 1. System Overview

The application is a personal finance monitor. The backend is responsible for:

- User registration and login (JWT Bearer),
- CRUD for one-time transactions (income / expenses),
- Management of recurring transactions,
- Automatic generation of transactions from recurring entries (background service),
- Statistics and summaries for charts,
- A shared category dictionary.

**Communication:** REST JSON over HTTP. The frontend sends the `Authorization: Bearer <token>` header on protected endpoints.

**Database:** PostgreSQL 16. EF Core runs `EnsureCreated()` / migrations on startup — tables are created if they do not exist. In Docker the schema is also initialized by `db/skrypt.sql`.

**Frontend compatibility contract:**
- JSON field names must use the original Polish snake_case names (e.g. `id_uzytkownika`, `nastepny_termin`).
- Use `JsonPropertyName` attributes or configure `JsonNamingPolicy` accordingly.
- All dates in JSON: `YYYY-MM-DD` (ISO 8601 date string, no time component).
- Error responses: `{ "detail": "..." }` for 400 / 401 / 403 / 404.

---

## 2. Technology Stack

| Layer | Technology | NuGet Package |
|-------|------------|---------------|
| HTTP Framework | ASP.NET Core Web API | .NET 8 SDK |
| ORM | Entity Framework Core | `Microsoft.EntityFrameworkCore` 8.x |
| DB Provider | Npgsql (PostgreSQL) | `Npgsql.EntityFrameworkCore.PostgreSQL` 8.x |
| Password Hashing | BCrypt.Net | `BCrypt.Net-Next` 4.x |
| JWT | System.IdentityModel | `Microsoft.AspNetCore.Authentication.JwtBearer` 8.x |
| Configuration | `Microsoft.Extensions.Configuration` | built-in |
| Scheduling | `IHostedService` | built-in |
| ISO 8601 Duration | NodaTime or custom parser | `NodaTime` 3.x (optional) |
| CORS | built-in middleware | built-in |

> EF Core replaces SQLAlchemy. `BCrypt.Net-Next` replaces `bcrypt`. `Microsoft.AspNetCore.Authentication.JwtBearer` replaces PyJWT. `IHostedService` replaces APScheduler.

---

## 3. Project Structure

```
FinCalBackend/
├── FinCalBackend.csproj
├── Program.cs                        # App builder, middleware pipeline, DI registration
├── appsettings.json                  # Config (overridden by env vars or appsettings.Development.json)
├── appsettings.Development.json
│
├── Data/
│   └── AppDbContext.cs               # EF Core DbContext — all DbSets + model config
│
├── Entities/                         # EF Core entity classes (map to DB tables)
│   ├── Uzytkownik.cs                 # t_uzytkownik
│   ├── Kategoria.cs                  # t_kategorie
│   ├── Transakcja.cs                 # t_transakcje
│   └── Powtarzalna.cs                # t_t_powtarzalne
│
├── DTOs/                             # Request / Response models (no EF annotations)
│   ├── Users/
│   │   ├── UzytkownikCreateDto.cs
│   │   ├── UzytkownikResponseDto.cs
│   │   ├── LoginResponseDto.cs
│   │   ├── EmailChangeDto.cs
│   │   └── PasswordChangeDto.cs
│   ├── Transactions/
│   │   ├── TransakcjaCreateDto.cs
│   │   └── TransakcjaResponseDto.cs
│   ├── Recurring/
│   │   ├── PowtarzalnaCreateDto.cs
│   │   ├── PowtarzalnaResponseDto.cs
│   │   └── PowtarzalnaUpdateDto.cs
│   └── Stats/
│       ├── StatKategoriaDto.cs
│       └── StatSummaryDto.cs
│
├── Services/
│   ├── IUserService.cs + UserService.cs
│   ├── ITransactionService.cs + TransactionService.cs
│   ├── IRecurringService.cs + RecurringService.cs
│   ├── IStatsService.cs + StatsService.cs
│   ├── ICategoryService.cs + CategoryService.cs
│   ├── IAuthService.cs + AuthService.cs       # JWT create / validate
│   └── RecurringProcessorService.cs           # IHostedService — scheduler
│
└── Controllers/
    ├── UsersController.cs            # /register, /login, /update_email, /update_password
    ├── CategoriesController.cs       # /kategorie
    ├── TransactionsController.cs     # /transakcje, /wplywy, /wydatki, /add_payment
    ├── RecurringController.cs        # /add_recurring, /get_recurring, /modify_recurring/{id}
    └── StatsController.cs            # /get_stats, /get_summary
```

**Pattern:** Controller → Service → DbContext. Controllers handle routing and JWT extraction. Services contain business logic and EF Core queries.

---

## 4. Environment Configuration

### 4.1. `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=fin_calc_db;Username=fin_calc_api;Password=fin_calc_pass"
  },
  "Jwt": {
    "SecretKey": "your-long-secret-key-at-least-32-chars",
    "ExpiresInMinutes": 60
  },
  "AllowedHosts": "*"
}
```

### 4.2. Environment Variable Overrides

All values can be overridden via environment variables using the standard .NET double-underscore syntax:

| Variable | `appsettings.json` key | Docker example |
|----------|------------------------|----------------|
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings:DefaultConnection` | `Host=db;Port=5432;...` |
| `Jwt__SecretKey` | `Jwt:SecretKey` | `supersecretkey` |
| `Jwt__ExpiresInMinutes` | `Jwt:ExpiresInMinutes` | `60` |

### 4.3. Local Development Run

```bash
cd FinCalBackend
dotnet restore
dotnet run
# API available at http://localhost:8000
# Swagger UI at http://localhost:8000/swagger (Development only)
```

To bind to port 8000 (matching the original FastAPI port expected by the frontend):

```json
// Properties/launchSettings.json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://0.0.0.0:8000"
    }
  }
}
```

Or via `appsettings.json`:

```json
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://0.0.0.0:8000" }
  }
}
```

### 4.4. CORS

The backend must accept requests from any origin with credentials, matching the original FastAPI config:

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Pipeline:
app.UseCors();
```

> **Note:** `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined in ASP.NET Core (security restriction). Since the original app uses `allow_credentials=True` with `allow_origins=['*']`, in practice the frontend does not send cookies — only the `Authorization` header. `AllowAnyOrigin()` without `AllowCredentials()` is sufficient and correct here.

---

## 5. PostgreSQL Database

The schema is **identical** to the original. Use the existing `db/skrypt.sql` to initialize tables and seed data, or let EF Core create them via `EnsureCreated()`.

### 5.1. ER Diagram

```
t_uzytkownik (1) ──< (N) t_transakcje
t_uzytkownik (1) ──< (N) t_t_powtarzalne
t_kategorie  (1) ──< (N) t_transakcje
t_kategorie  (1) ──< (N) t_t_powtarzalne
```

### 5.2. Table `t_uzytkownik`

| Column | SQL Type | Nullable | Default | Notes |
|--------|----------|----------|---------|-------|
| `id_uzytkownika` | SERIAL | NO | auto | PK |
| `imie` | VARCHAR(30) | NO | — | First name |
| `nazwisko` | VARCHAR(30) | NO | — | Last name |
| `email` | VARCHAR(100) | NO | — | Unique, used as login |
| `haslo` | VARCHAR(255) | NO | — | BCrypt hash — never plain text |
| `data_zalozenia` | TIMESTAMP | NO | `CURRENT_TIMESTAMP` | Registration date |
| `czy_aktywny` | BOOLEAN | NO | `TRUE` | Account active flag |
| `data_usuniecia` | TIMESTAMP | YES | NULL | Soft-delete (unused by API) |

### 5.3. Table `t_kategorie`

Shared category dictionary — read-only via API.

| Column | SQL Type | Nullable | Notes |
|--------|----------|----------|-------|
| `id_kategorii` | SERIAL | NO | PK |
| `nazwa` | VARCHAR(30) | NO | Unique category name |

**Seed data (from `db/skrypt.sql`):**

| id_kategorii | nazwa |
|--------------|-------|
| 1 | rozrywka |
| 2 | transport |
| 3 | jedzenie |
| 4 | oplaty |
| 5 | pozostale |

### 5.4. Table `t_transakcje`

| Column | SQL Type | Nullable | Default | Notes |
|--------|----------|----------|---------|-------|
| `id_transakcji` | SERIAL | NO | auto | PK |
| `id_uzytkownika` | INTEGER | NO | — | FK → `t_uzytkownik` ON DELETE CASCADE |
| `id_kategorii` | INTEGER | YES | — | FK → `t_kategorie` ON DELETE CASCADE |
| `typ` | VARCHAR(10) | NO | — | `'wplyw'` or `'wydatek'` |
| `tytul` | VARCHAR(100) | NO | — | Transaction title |
| `opis` | TEXT | YES | — | Optional description |
| `kwota` | NUMERIC(12,2) | NO | — | Amount (always positive in DB) |
| `metoda` | VARCHAR(10) | NO | — | `'gotowka'` or `'przelew'` |
| `konto` | VARCHAR(50) | YES | — | Bank account number (transfer only) |
| `wlasciciel_konta` | VARCHAR(100) | YES | — | Account owner |
| `data` | DATE | NO | `CURRENT_DATE` | Transaction date |

**CHECK constraints:** `typ IN ('wplyw', 'wydatek')` and `metoda IN ('gotowka', 'przelew')`.

**Important:** `id_uzytkownika` on INSERT is **always set from the JWT token**, never from the request body.

### 5.5. Table `t_t_powtarzalne`

| Column | SQL Type | Nullable | Default | Notes |
|--------|----------|----------|---------|-------|
| `id_t_powtarzalnej` | SERIAL | NO | auto | PK |
| `id_uzytkownika` | INTEGER | NO | — | FK → `t_uzytkownik` ON DELETE CASCADE |
| `id_kategorii` | INTEGER | YES | — | FK → `t_kategorie` |
| `typ` | VARCHAR(10) | NO | — | `'wplyw'` or `'wydatek'` |
| `tytul` | VARCHAR(100) | NO | — | Title |
| `opis` | TEXT | YES | — | Description |
| `kwota` | NUMERIC(12,2) | NO | — | Amount |
| `metoda` | VARCHAR(10) | NO | — | `'gotowka'` or `'przelew'` |
| `konto` | VARCHAR(50) | YES | — | Account number |
| `wlasciciel_konta` | VARCHAR(100) | YES | — | Account owner |
| `co_ile` | VARCHAR(10) | YES | — | ISO 8601 interval (e.g. `P7D`, `P30D`, `P1Y`) |
| `nastepny_termin` | DATE | NO | — | Next scheduled date |
| `czy_aktywna` | BOOLEAN | NO | `TRUE` | Whether scheduler should process this entry |

### 5.6. Database User SQL (`db/user.sql`)

```sql
CREATE USER fin_calc_api WITH PASSWORD 'fin_calc_pass';
GRANT CONNECT ON DATABASE fin_calc_db TO fin_calc_api;
GRANT USAGE ON SCHEMA public TO fin_calc_api;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO fin_calc_api;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO fin_calc_api;
```

---

## 6. Entity Models (EF Core)

File: `Data/AppDbContext.cs` and `Entities/*.cs`.

### 6.1. `Uzytkownik.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("t_uzytkownik")]
public class Uzytkownik
{
    [Key]
    [Column("id_uzytkownika")]
    public int IdUzytkownika { get; set; }

    [Column("imie"), MaxLength(30)]
    public string Imie { get; set; } = "";

    [Column("nazwisko"), MaxLength(30)]
    public string Nazwisko { get; set; } = "";

    [Column("email"), MaxLength(100)]
    public string Email { get; set; } = "";

    [Column("haslo"), MaxLength(255)]
    public string Haslo { get; set; } = "";

    [Column("data_zalozenia")]
    public DateTime DataZalozenia { get; set; } = DateTime.UtcNow;

    [Column("czy_aktywny")]
    public bool CzyAktywny { get; set; } = true;

    [Column("data_usuniecia")]
    public DateTime? DataUsuniecia { get; set; }

    // Navigation properties
    public ICollection<Transakcja> Transakcje { get; set; } = new List<Transakcja>();
    public ICollection<Powtarzalna> Powtarzalne { get; set; } = new List<Powtarzalna>();
}
```

### 6.2. `Kategoria.cs`

```csharp
[Table("t_kategorie")]
public class Kategoria
{
    [Key]
    [Column("id_kategorii")]
    public int IdKategorii { get; set; }

    [Column("nazwa"), MaxLength(30)]
    public string Nazwa { get; set; } = "";
}
```

### 6.3. `Transakcja.cs`

```csharp
[Table("t_transakcje")]
public class Transakcja
{
    [Key]
    [Column("id_transakcji")]
    public int IdTransakcji { get; set; }

    [Column("id_uzytkownika")]
    public int IdUzytkownika { get; set; }

    [Column("id_kategorii")]
    public int? IdKategorii { get; set; }

    [Column("typ"), MaxLength(10)]
    public string Typ { get; set; } = "";      // "wplyw" | "wydatek"

    [Column("tytul"), MaxLength(100)]
    public string Tytul { get; set; } = "";

    [Column("opis")]
    public string? Opis { get; set; }

    [Column("kwota")]
    public decimal Kwota { get; set; }

    [Column("metoda"), MaxLength(10)]
    public string Metoda { get; set; } = "";   // "gotowka" | "przelew"

    [Column("konto"), MaxLength(50)]
    public string? Konto { get; set; }

    [Column("wlasciciel_konta"), MaxLength(100)]
    public string? WlascicielKonta { get; set; }

    [Column("data")]
    public DateOnly Data { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // Navigation
    [ForeignKey("IdUzytkownika")]
    public Uzytkownik? Uzytkownik { get; set; }

    [ForeignKey("IdKategorii")]
    public Kategoria? Kategoria { get; set; }
}
```

### 6.4. `Powtarzalna.cs`

```csharp
[Table("t_t_powtarzalne")]
public class Powtarzalna
{
    [Key]
    [Column("id_t_powtarzalnej")]
    public int IdTPowtarzalnej { get; set; }

    [Column("id_uzytkownika")]
    public int IdUzytkownika { get; set; }

    [Column("id_kategorii")]
    public int? IdKategorii { get; set; }

    [Column("typ"), MaxLength(10)]
    public string Typ { get; set; } = "";

    [Column("tytul"), MaxLength(100)]
    public string Tytul { get; set; } = "";

    [Column("opis")]
    public string? Opis { get; set; }

    [Column("kwota")]
    public decimal Kwota { get; set; }

    [Column("metoda"), MaxLength(10)]
    public string Metoda { get; set; } = "";

    [Column("konto"), MaxLength(50)]
    public string? Konto { get; set; }

    [Column("wlasciciel_konta"), MaxLength(100)]
    public string? WlascicielKonta { get; set; }

    [Column("co_ile"), MaxLength(10)]
    public string? CoIle { get; set; }         // ISO 8601: "P7D", "P30D", "P1Y"

    [Column("nastepny_termin")]
    public DateOnly NastepnyTermin { get; set; }

    [Column("czy_aktywna")]
    public bool CzyAktywna { get; set; } = true;

    // Navigation
    [ForeignKey("IdUzytkownika")]
    public Uzytkownik? Uzytkownik { get; set; }

    [ForeignKey("IdKategorii")]
    public Kategoria? Kategoria { get; set; }
}
```

### 6.5. `AppDbContext.cs`

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Uzytkownik> Uzytkownicy => Set<Uzytkownik>();
    public DbSet<Kategoria> Kategorie => Set<Kategoria>();
    public DbSet<Transakcja> Transakcje => Set<Transakcja>();
    public DbSet<Powtarzalna> Powtarzalne => Set<Powtarzalna>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique constraint on email
        modelBuilder.Entity<Uzytkownik>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Unique constraint on category name
        modelBuilder.Entity<Kategoria>()
            .HasIndex(k => k.Nazwa)
            .IsUnique();

        // CHECK constraints mirroring the original SQL
        modelBuilder.Entity<Transakcja>()
            .ToTable(t => t.HasCheckConstraint("chk_typ", "typ IN ('wplyw', 'wydatek')"))
            .ToTable(t => t.HasCheckConstraint("chk_metoda", "metoda IN ('gotowka', 'przelew')"));

        modelBuilder.Entity<Powtarzalna>()
            .ToTable(t => t.HasCheckConstraint("chk_typ", "typ IN ('wplyw', 'wydatek')"))
            .ToTable(t => t.HasCheckConstraint("chk_metoda", "metoda IN ('gotowka', 'przelew')"));

        // decimal precision
        modelBuilder.Entity<Transakcja>()
            .Property(t => t.Kwota)
            .HasColumnType("numeric(12,2)");

        modelBuilder.Entity<Powtarzalna>()
            .Property(p => p.Kwota)
            .HasColumnType("numeric(12,2)");
    }
}
```

### 6.6. `Program.cs` — DbContext Registration

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Create tables on startup (replaces SQLAlchemy create_all)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();   // or db.Database.Migrate() if using migrations
}
```

---

## 7. DTO Contracts (API Contract)

All DTOs use `System.Text.Json.Serialization.JsonPropertyName` to produce the exact Polish snake_case field names the frontend expects.

### 7.1. User DTOs

```csharp
// UzytkownikCreateDto.cs — used for /register and /login
public class UzytkownikCreateDto
{
    [JsonPropertyName("imie")]
    public string Imie { get; set; } = "";

    [JsonPropertyName("nazwisko")]
    public string Nazwisko { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("haslo")]
    public string Haslo { get; set; } = "";
}

// UzytkownikResponseDto.cs — returned after /register
public class UzytkownikResponseDto
{
    [JsonPropertyName("id_uzytkownika")]
    public int IdUzytkownika { get; set; }

    [JsonPropertyName("imie")]
    public string Imie { get; set; } = "";

    [JsonPropertyName("nazwisko")]
    public string Nazwisko { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("czy_aktywny")]
    public bool? CzyAktywny { get; set; }
}

// LoginResponseDto.cs — returned after /login
public class LoginResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "bearer";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "zalogowano pomyslnie";
}

// EmailChangeDto.cs
public class EmailChangeDto
{
    [JsonPropertyName("current_pass")]
    public string CurrentPass { get; set; } = "";

    [JsonPropertyName("new_email")]
    public string NewEmail { get; set; } = "";
}

// PasswordChangeDto.cs
public class PasswordChangeDto
{
    [JsonPropertyName("current_pass")]
    public string CurrentPass { get; set; } = "";

    [JsonPropertyName("new_pass")]
    public string NewPass { get; set; } = "";
}
```

### 7.2. Transaction DTOs

```csharp
// TransakcjaCreateDto.cs
public class TransakcjaCreateDto
{
    [JsonPropertyName("id_uzytkownika")]
    public int? IdUzytkownika { get; set; }    // Ignored — always set from JWT

    [JsonPropertyName("id_kategorii")]
    public int? IdKategorii { get; set; }

    [JsonPropertyName("typ")]
    public string Typ { get; set; } = "";      // "wplyw" | "wydatek"

    [JsonPropertyName("tytul")]
    public string Tytul { get; set; } = "";

    [JsonPropertyName("opis")]
    public string? Opis { get; set; }

    [JsonPropertyName("kwota")]
    public decimal Kwota { get; set; }

    [JsonPropertyName("metoda")]
    public string Metoda { get; set; } = "";   // "gotowka" | "przelew"

    [JsonPropertyName("konto")]
    public string? Konto { get; set; }

    [JsonPropertyName("wlasciciel_konta")]
    public string? WlascicielKonta { get; set; }

    [JsonPropertyName("data")]
    public DateOnly? Data { get; set; }        // Optional — defaults to today
}

// TransakcjaResponseDto.cs
public class TransakcjaResponseDto
{
    [JsonPropertyName("id_transakcji")]
    public int IdTransakcji { get; set; }

    [JsonPropertyName("id_uzytkownika")]
    public int IdUzytkownika { get; set; }

    [JsonPropertyName("id_kategorii")]
    public int? IdKategorii { get; set; }

    [JsonPropertyName("typ")]
    public string Typ { get; set; } = "";

    [JsonPropertyName("tytul")]
    public string Tytul { get; set; } = "";

    [JsonPropertyName("opis")]
    public string? Opis { get; set; }

    [JsonPropertyName("kwota")]
    public decimal Kwota { get; set; }

    [JsonPropertyName("metoda")]
    public string Metoda { get; set; } = "";

    [JsonPropertyName("konto")]
    public string? Konto { get; set; }

    [JsonPropertyName("wlasciciel_konta")]
    public string? WlascicielKonta { get; set; }

    [JsonPropertyName("data")]
    public DateOnly Data { get; set; }
}
```

### 7.3. Recurring Transaction DTOs

```csharp
// PowtarzalnaCreateDto.cs
public class PowtarzalnaCreateDto
{
    [JsonPropertyName("id_kategorii")]
    public int? IdKategorii { get; set; }

    [JsonPropertyName("typ")]
    public string Typ { get; set; } = "";

    [JsonPropertyName("tytul")]
    public string Tytul { get; set; } = "";

    [JsonPropertyName("opis")]
    public string? Opis { get; set; }

    [JsonPropertyName("kwota")]
    public decimal Kwota { get; set; }

    [JsonPropertyName("metoda")]
    public string Metoda { get; set; } = "";

    [JsonPropertyName("konto")]
    public string? Konto { get; set; }

    [JsonPropertyName("wlasciciel_konta")]
    public string? WlascicielKonta { get; set; }

    [JsonPropertyName("co_ile")]
    public string CoIle { get; set; } = "";    // "P7D" | "P30D" | "P1Y"

    [JsonPropertyName("nastepny_termin")]
    public DateOnly NastepnyTermin { get; set; }
}

// PowtarzalnaUpdateDto.cs — includes czy_aktywna
public class PowtarzalnaUpdateDto : PowtarzalnaCreateDto
{
    [JsonPropertyName("czy_aktywna")]
    public bool CzyAktywna { get; set; }
}

// PowtarzalnaResponseDto.cs
public class PowtarzalnaResponseDto : PowtarzalnaCreateDto
{
    [JsonPropertyName("id_t_powtarzalnej")]
    public int IdTPowtarzalnej { get; set; }

    [JsonPropertyName("id_uzytkownika")]
    public int IdUzytkownika { get; set; }

    [JsonPropertyName("czy_aktywna")]
    public bool CzyAktywna { get; set; }
}
```

### 7.4. Stats DTOs

```csharp
// StatKategoriaDto.cs — for /get_stats
public class StatKategoriaDto
{
    [JsonPropertyName("kategoria")]
    public string Kategoria { get; set; } = "";

    [JsonPropertyName("kwota")]
    public decimal Kwota { get; set; }
}

// StatSummaryDto.cs — for /get_summary
public class StatSummaryDto
{
    [JsonPropertyName("typ")]
    public string Typ { get; set; } = "";

    [JsonPropertyName("kwota")]
    public decimal Kwota { get; set; }
}
```

### 7.5. `DateOnly` JSON serialization

`System.Text.Json` in .NET 8 supports `DateOnly` natively with the `YYYY-MM-DD` format, but you must explicitly enable it in `Program.cs`:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // DateOnly serializes as "YYYY-MM-DD" — matching the frontend contract
        opts.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

// DateOnlyJsonConverter.cs
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateOnly.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
}
```

### 7.6. Enumeration values (frontend contract)

| Field | Allowed values |
|-------|---------------|
| `typ` | `wplyw`, `wydatek` |
| `metoda` | `gotowka`, `przelew` |
| `co_ile` | `P7D` (weekly), `P30D` (monthly), `P1Y` (yearly) |

---

## 8. Authentication & Authorization (JWT)

### 8.1. JWT Configuration — `Program.cs`

```csharp
var jwtKey = builder.Configuration["Jwt:SecretKey"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,      // No issuer in original token
            ValidateAudience = false,    // No audience in original token
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero    // Tokens expire exactly at exp
        };

        // Return { "detail": "..." } on 401 (frontend parses this field)
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"detail\":\"Nieautoryzowany dostep\"}");
            }
        };
    });

builder.Services.AddAuthorization();

// Pipeline (order matters):
app.UseAuthentication();
app.UseAuthorization();
```

### 8.2. JWT Token Structure

- **Algorithm:** HS256
- **Secret:** `Jwt:SecretKey` from configuration
- **Payload:**
  - `sub`: `id_uzytkownika` as a string (e.g. `"3"`) — matches the Python original
  - `exp`: Unix timestamp UTC, `Jwt:ExpiresInMinutes` minutes from issue time

```csharp
// AuthService.cs
public string CreateToken(int userId, int expiresInMinutes, string secretKey)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### 8.3. Extracting Current User from JWT

In protected controllers, extract the user ID from the `sub` claim:

```csharp
// Helper method — add to a base controller or extension
private int GetCurrentUserId()
{
    var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
              ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? throw new UnauthorizedAccessException();
    return int.Parse(sub);
}
```

Protect endpoints with `[Authorize]`:

```csharp
[ApiController]
[Authorize]              // Requires valid JWT Bearer token
public class TransactionsController : ControllerBase { ... }
```

Public endpoints (no `[Authorize]`): `/register`, `/login`, `/kategorie`.

### 8.4. Password Hashing (BCrypt)

```csharp
// Install: dotnet add package BCrypt.Net-Next

// Hash on registration / password change:
string hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

// Verify on login / email+password change:
bool valid = BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
```

### 8.5. Error Response Format

The frontend reads `error.response.data.detail`. All error responses must follow this format:

```json
{ "detail": "Human-readable message here" }
```

Create a helper or use `ProblemDetails` override:

```csharp
// Returns { "detail": "..." } — call from any controller action
private IActionResult DetailError(int statusCode, string message)
    => StatusCode(statusCode, new { detail = message });
```

---

## 9. Background Scheduler (`IHostedService`)

### 9.1. Purpose

Automatically create one-time transactions (`t_transakcje`) from active recurring entries (`t_t_powtarzalne`) where `nastepny_termin <= today`.

### 9.2. Registration in `Program.cs`

```csharp
builder.Services.AddHostedService<RecurringProcessorService>();
```

### 9.3. `RecurringProcessorService.cs`

```csharp
public class RecurringProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecurringProcessorService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(12);

    public RecurringProcessorService(IServiceScopeFactory scopeFactory,
                                     ILogger<RecurringProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run immediately on startup (matches APScheduler original behavior)
        await ProcessRecurringAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);
            await ProcessRecurringAsync();
        }
    }

    private async Task ProcessRecurringAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var dueEntries = await db.Powtarzalne
            .Where(p => p.CzyAktywna && p.NastepnyTermin <= today)
            .ToListAsync();

        foreach (var entry in dueEntries)
        {
            try
            {
                // 1. Parse ISO 8601 duration
                var interval = ParseIso8601Duration(entry.CoIle);
                if (interval == null)
                {
                    _logger.LogWarning("Invalid co_ile value '{CoIle}' for id={Id}", entry.CoIle, entry.IdTPowtarzalnej);
                    continue;
                }

                // 2. INSERT into t_transakcje (copy fields, date = nastepny_termin)
                var transaction = new Transakcja
                {
                    IdUzytkownika    = entry.IdUzytkownika,
                    IdKategorii      = entry.IdKategorii,
                    Typ              = entry.Typ,
                    Tytul            = entry.Tytul,
                    Opis             = entry.Opis,
                    Kwota            = entry.Kwota,
                    Metoda           = entry.Metoda,
                    Konto            = entry.Konto,
                    WlascicielKonta  = entry.WlascicielKonta,
                    Data             = entry.NastepnyTermin   // date of due date, not today
                };
                db.Transakcje.Add(transaction);

                // 3. UPDATE nastepny_termin += interval (advance by ONE interval only)
                entry.NastepnyTermin = AddInterval(entry.NastepnyTermin, interval.Value);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring id={Id}", entry.IdTPowtarzalnej);
            }
        }
    }

    // Parses "P7D" → 7 days, "P30D" → 30 days, "P1Y" → 1 year
    private static (int days, int months, int years)? ParseIso8601Duration(string? input)
    {
        if (string.IsNullOrEmpty(input)) return null;
        var match = Regex.Match(input, @"^P(?:(\d+)Y)?(?:(\d+)M)?(?:(\d+)D)?$");
        if (!match.Success) return null;

        int years  = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
        int months = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        int days   = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
        return (days, months, years);
    }

    private static DateOnly AddInterval(DateOnly date, (int days, int months, int years) interval)
    {
        var dt = date.ToDateTime(TimeOnly.MinValue);
        dt = dt.AddYears(interval.years).AddMonths(interval.months).AddDays(interval.days);
        return DateOnly.FromDateTime(dt);
    }
}
```

**Key behavior:** If `nastepny_termin` is far in the past, the scheduler creates **one** transaction per run and advances the date by one interval — it does not backfill in a loop (matching the original Python behavior).

---

## 10. API Reference — All Endpoints

**Base URL (dev):** `http://localhost:8000`  
**Base URL (Docker + Nginx):** `/api` (proxy to backend — see section 13)

All endpoints are at **root path** — no `/api` prefix in the .NET app itself.

Legend:
- 🔓 — public (no token required)
- 🔒 — requires `Authorization: Bearer <token>`

---

### 10.1. Users — `UsersController.cs`

#### `POST /register` 🔓

**Description:** Register a new user.

**Request body** (`application/json`):

```json
{
  "imie": "Jan",
  "nazwisko": "Kowalski",
  "email": "jan@example.com",
  "haslo": "secret123"
}
```

**Response `200`:**

```json
{
  "id_uzytkownika": 1,
  "imie": "Jan",
  "nazwisko": "Kowalski",
  "email": "jan@example.com",
  "czy_aktywny": true
}
```

**Errors:**
- `400` — email already taken: `{ "detail": "Uzytkownik z adresem jan@example.com juz istnieje" }`

**Implementation notes:**
- Hash `haslo` with BCrypt before INSERT.
- Check for duplicate email before INSERT (catch `DbUpdateException` on unique constraint violation, or pre-query).
- Never return the `haslo` field.

```csharp
[HttpPost("/register")]
public async Task<IActionResult> Register([FromBody] UzytkownikCreateDto dto)
{
    var exists = await _db.Uzytkownicy.AnyAsync(u => u.Email == dto.Email);
    if (exists)
        return DetailError(400, $"Uzytkownik z adresem {dto.Email} juz istnieje");

    var user = new Uzytkownik
    {
        Imie     = dto.Imie,
        Nazwisko = dto.Nazwisko,
        Email    = dto.Email,
        Haslo    = BCrypt.Net.BCrypt.HashPassword(dto.Haslo)
    };
    _db.Uzytkownicy.Add(user);
    await _db.SaveChangesAsync();

    return Ok(new UzytkownikResponseDto
    {
        IdUzytkownika = user.IdUzytkownika,
        Imie          = user.Imie,
        Nazwisko      = user.Nazwisko,
        Email         = user.Email,
        CzyAktywny    = user.CzyAktywny
    });
}
```

---

#### `POST /login` 🔓

**Description:** Login — returns JWT.

**Request body** (frontend sends empty `imie`/`nazwisko`):

```json
{
  "imie": "",
  "nazwisko": "",
  "email": "jan@example.com",
  "haslo": "secret123"
}
```

**Response `200`:**

```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "token_type": "bearer",
  "message": "zalogowano pomyslnie"
}
```

**Errors:**
- `403` — bad credentials: `{ "detail": "Nieprawidlowy email lub haslo" }`

**Implementation notes:**
- Find user by `email`. If not found → `403`.
- `BCrypt.Net.BCrypt.Verify(dto.Haslo, user.Haslo)`. If false → `403`.
- Create JWT with `sub = user.IdUzytkownika.ToString()`.

```csharp
[HttpPost("/login")]
public async Task<IActionResult> Login([FromBody] UzytkownikCreateDto dto)
{
    var user = await _db.Uzytkownicy.FirstOrDefaultAsync(u => u.Email == dto.Email);
    if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Haslo, user.Haslo))
        return DetailError(403, "Nieprawidlowy email lub haslo");

    var token = _authService.CreateToken(
        user.IdUzytkownika,
        _config.GetValue<int>("Jwt:ExpiresInMinutes"),
        _config["Jwt:SecretKey"]!);

    return Ok(new LoginResponseDto { AccessToken = token });
}
```

---

#### `PUT /update_email` 🔒

**Request body:**

```json
{
  "current_pass": "secret123",
  "new_email": "nowy@example.com"
}
```

**Response `200`:** `{ "message": "email zmienono pomyslnie" }`

**Errors:**
- `400` — wrong password: `{ "detail": "Niepoprawne haslo" }`
- `400` — email taken: `{ "detail": "Email jest juz zajety" }`
- `401` — missing/invalid token

---

#### `PUT /update_password` 🔒

**Request body:**

```json
{
  "current_pass": "secret123",
  "new_pass": "newsecret456"
}
```

**Response `200`:** `{ "message": "Zmiana hasla pomyslna" }`

**Errors:**
- `400`: `{ "detail": "Nieprawidlowe haslo" }`
- `401` — missing/invalid token

---

### 10.2. Categories — `CategoriesController.cs`

#### `GET /kategorie` 🔓

**Response `200`** — array of `Kategoria[]`:

```json
[
  { "id_kategorii": 1, "nazwa": "rozrywka" },
  { "id_kategorii": 2, "nazwa": "transport" },
  { "id_kategorii": 3, "nazwa": "jedzenie" },
  { "id_kategorii": 4, "nazwa": "oplaty" },
  { "id_kategorii": 5, "nazwa": "pozostale" }
]
```

```csharp
[HttpGet("/kategorie")]
public async Task<IActionResult> GetKategorie()
    => Ok(await _db.Kategorie.ToListAsync());
```

> **Note:** This is a public endpoint — no `[Authorize]` attribute. The frontend (`PaymentForm`, `RecurringForm`) calls it without a token.

---

### 10.3. Transactions — `TransactionsController.cs`

All endpoints `[Authorize]` — filtered by `id_uzytkownika` from JWT.

#### `GET /transakcje` 🔒

**Description:** All user transactions, sorted `data DESC`.

**Response `200`** — `TransakcjaResponseDto[]`:

```json
[
  {
    "id_transakcji": 10,
    "id_uzytkownika": 1,
    "id_kategorii": 3,
    "typ": "wydatek",
    "tytul": "Zakupy",
    "opis": "Biedronka",
    "kwota": 150.50,
    "metoda": "gotowka",
    "konto": null,
    "wlasciciel_konta": null,
    "data": "2026-06-01"
  }
]
```

```csharp
[HttpGet("/transakcje")]
public async Task<IActionResult> GetTransakcje()
{
    var userId = GetCurrentUserId();
    var items = await _db.Transakcje
        .Where(t => t.IdUzytkownika == userId)
        .OrderByDescending(t => t.Data)
        .Select(t => MapToDto(t))
        .ToListAsync();
    return Ok(items);
}
```

---

#### `GET /wplywy` 🔒

Identical to `/transakcje` but filtered by `typ == "wplyw"`. Used by `Dashboard/LeftPanel.tsx`.

```csharp
[HttpGet("/wplywy")]
public async Task<IActionResult> GetWplywy()
{
    var userId = GetCurrentUserId();
    var items = await _db.Transakcje
        .Where(t => t.IdUzytkownika == userId && t.Typ == "wplyw")
        .OrderByDescending(t => t.Data)
        .Select(t => MapToDto(t))
        .ToListAsync();
    return Ok(items);
}
```

---

#### `GET /wydatki` 🔒

Identical to `/transakcje` but filtered by `typ == "wydatek"`. Used by `Dashboard/RightPanel.tsx`.

---

#### `POST /add_payment` 🔒

**Request body** (`TransakcjaCreateDto`):

```json
{
  "id_kategorii": 3,
  "typ": "wydatek",
  "tytul": "Zakupy",
  "opis": "Biedronka",
  "kwota": 150.50,
  "metoda": "gotowka",
  "konto": null,
  "wlasciciel_konta": null,
  "data": "2026-06-15"
}
```

**Response `200`** — the created `TransakcjaResponseDto` (including `id_transakcji` and `id_uzytkownika`).

**Implementation notes:**
- `id_uzytkownika` is **always** set from `GetCurrentUserId()` — the value in `dto.IdUzytkownika` is ignored.
- If `dto.Data` is null → EF Core sends no value for `data` and PostgreSQL uses `CURRENT_DATE`.
- To avoid CHECK constraint errors from PostgreSQL, validate `typ` and `metoda` in the service layer first and return `400` with `{ "detail": "..." }`.

```csharp
[HttpPost("/add_payment")]
public async Task<IActionResult> AddPayment([FromBody] TransakcjaCreateDto dto)
{
    var entity = new Transakcja
    {
        IdUzytkownika   = GetCurrentUserId(),  // JWT — ignore dto value
        IdKategorii     = dto.IdKategorii,
        Typ             = dto.Typ,
        Tytul           = dto.Tytul,
        Opis            = dto.Opis,
        Kwota           = dto.Kwota,
        Metoda          = dto.Metoda,
        Konto           = dto.Konto,
        WlascicielKonta = dto.WlascicielKonta,
        Data            = dto.Data ?? DateOnly.FromDateTime(DateTime.Today)
    };
    _db.Transakcje.Add(entity);
    await _db.SaveChangesAsync();
    return Ok(MapToDto(entity));
}
```

---

### 10.4. Recurring Transactions — `RecurringController.cs`

All endpoints `[Authorize]`.

#### `POST /add_recurring` 🔒

**Request body** (`PowtarzalnaCreateDto`):

```json
{
  "id_kategorii": 4,
  "typ": "wydatek",
  "tytul": "Czynsz",
  "opis": "Miesieczny czynsz",
  "kwota": 2500.00,
  "metoda": "przelew",
  "konto": "1234567890",
  "wlasciciel_konta": "Jan Kowalski",
  "co_ile": "P30D",
  "nastepny_termin": "2026-07-01"
}
```

**Response `200`** — `PowtarzalnaResponseDto` with `id_t_powtarzalnej`, `id_uzytkownika`, `czy_aktywna: true`.

**Frontend note:** `PaymentForm` sends to `/add_recurring` when the recurring checkbox is checked, mapping the `data` form field to `nastepny_termin`.

---

#### `GET /get_recurring` 🔒

**Response `200`** — `PowtarzalnaResponseDto[]` sorted by `nastepny_termin ASC`.

Used by `Calendar/Upcoming.tsx`.

```csharp
[HttpGet("/get_recurring")]
public async Task<IActionResult> GetRecurring()
{
    var userId = GetCurrentUserId();
    var items = await _db.Powtarzalne
        .Where(p => p.IdUzytkownika == userId)
        .OrderBy(p => p.NastepnyTermin)
        .Select(p => MapToDto(p))
        .ToListAsync();
    return Ok(items);
}
```

---

#### `PUT /modify_recurring/{id_t_powtarzalnej}` 🔒

**Path parameter:** `id_t_powtarzalnej` — integer.

**Request body** (`PowtarzalnaUpdateDto` — all fields + `czy_aktywna`):

```json
{
  "id_kategorii": 4,
  "typ": "wydatek",
  "tytul": "Czynsz",
  "opis": "Miesieczny czynsz",
  "kwota": 2600.00,
  "metoda": "przelew",
  "konto": "1234567890",
  "wlasciciel_konta": "Jan Kowalski",
  "co_ile": "P30D",
  "nastepny_termin": "2026-08-01",
  "czy_aktywna": true
}
```

**Response `200`** — updated `PowtarzalnaResponseDto`.

**Errors:**
- `404`: `{ "detail": "Nie znaleziono transakcji" }` (wrong ID or belongs to different user)
- `401` — missing token

**Implementation notes:**
- Always filter by `id_uzytkownika == GetCurrentUserId()` to prevent access to another user's records.
- `id_uzytkownika` on the entity must NOT be overwritten from the DTO.

```csharp
[HttpPut("/modify_recurring/{id}")]
public async Task<IActionResult> ModifyRecurring(int id, [FromBody] PowtarzalnaUpdateDto dto)
{
    var userId = GetCurrentUserId();
    var entity = await _db.Powtarzalne
        .FirstOrDefaultAsync(p => p.IdTPowtarzalnej == id && p.IdUzytkownika == userId);

    if (entity == null)
        return DetailError(404, "Nie znaleziono transakcji");

    entity.IdKategorii      = dto.IdKategorii;
    entity.Typ              = dto.Typ;
    entity.Tytul            = dto.Tytul;
    entity.Opis             = dto.Opis;
    entity.Kwota            = dto.Kwota;
    entity.Metoda           = dto.Metoda;
    entity.Konto            = dto.Konto;
    entity.WlascicielKonta  = dto.WlascicielKonta;
    entity.CoIle            = dto.CoIle;
    entity.NastepnyTermin   = dto.NastepnyTermin;
    entity.CzyAktywna       = dto.CzyAktywna;

    await _db.SaveChangesAsync();
    return Ok(MapToDto(entity));
}
```

---

### 10.5. Stats — `StatsController.cs`

All endpoints `[Authorize]`.

#### `GET /get_stats` 🔒

**Description:** Sum of expenses (`typ = 'wydatek'`) grouped by category — data for the pie chart.

**Response `200`:**

```json
[
  { "kategoria": "jedzenie", "kwota": 1250.00 },
  { "kategoria": "transport", "kwota": 400.00 }
]
```

**SQL logic (EF Core LINQ equivalent):**

```csharp
[HttpGet("/get_stats")]
public async Task<IActionResult> GetStats()
{
    var userId = GetCurrentUserId();
    var stats = await _db.Kategorie
        .Join(_db.Transakcje,
              k => k.IdKategorii,
              t => t.IdKategorii,
              (k, t) => new { k.Nazwa, t.Typ, t.Kwota, t.IdUzytkownika })
        .Where(x => x.IdUzytkownika == userId && x.Typ == "wydatek")
        .GroupBy(x => x.Nazwa)
        .Select(g => new StatKategoriaDto
        {
            Kategoria = g.Key,
            Kwota     = g.Sum(x => x.Kwota)
        })
        .ToListAsync();

    return Ok(stats);
}
```

> **Note:** Transactions without a category (`id_kategorii IS NULL`) do not appear in results — this uses INNER JOIN, matching the original behavior.

Used by `Charts/PieChart.tsx` — labels: `kategoria`, values: `kwota`.

---

#### `GET /get_summary` 🔒

**Description:** Total income and expenses for the user — data for the bar chart and balance.

**Response `200`:**

```json
[
  { "typ": "wplyw",   "kwota": 8000.00 },
  { "typ": "wydatek", "kwota": 5200.00 }
]
```

```csharp
[HttpGet("/get_summary")]
public async Task<IActionResult> GetSummary()
{
    var userId = GetCurrentUserId();
    var summary = await _db.Transakcje
        .Where(t => t.IdUzytkownika == userId)
        .GroupBy(t => t.Typ)
        .Select(g => new StatSummaryDto
        {
            Typ   = g.Key,
            Kwota = g.Sum(t => t.Kwota)
        })
        .ToListAsync();

    return Ok(summary);
}
```

Used by `Charts/BarChart.tsx` — searches for elements with `typ === 'wplyw'` and `typ === 'wydatek'`, calculates balance as `wplywy - wydatki`.

---

## 11. Frontend ↔ Backend Mapping

### 11.1. HTTP Client Configuration

```typescript
// frontend/src/api.ts
const api = axios.create({ baseURL: import.meta.env.VITE_API_URL });
// Request interceptor: adds Authorization: Bearer ${localStorage.getItem('token')}
// Response interceptor: 401 → removes token, reloads page
```

| Environment | `VITE_API_URL` |
|-------------|----------------|
| Dev (Vite) | `http://localhost:8000` |
| Docker (Nginx) | `/api` (proxied → backend:8000) |

### 11.2. Endpoint Call Table

| Frontend Component | Method | Endpoint | Auth | Purpose |
|--------------------|--------|----------|------|---------|
| `RegisterForm` | POST | `/register` | 🔓 | Registration |
| `LoginForm` | POST | `/login` | 🔓 | Login, token storage |
| `EmailChange` | PUT | `/update_email` | 🔒 | Change email |
| `PassChange` | PUT | `/update_password` | 🔒 | Change password |
| `PaymentForm` | GET | `/kategorie` | 🔓 | Category list |
| `PaymentForm` | POST | `/add_payment` | 🔒 | New transaction |
| `PaymentForm` | POST | `/add_recurring` | 🔒 | New recurring |
| `RecurringForm` | GET | `/kategorie` | 🔓 | Category list |
| `RecurringForm` | PUT | `/modify_recurring/{id}` | 🔒 | Edit recurring |
| `LeftPanel` | GET | `/wplywy` | 🔒 | Income list |
| `RightPanel` | GET | `/wydatki` | 🔒 | Expense list |
| `Calendar/Past` | GET | `/transakcje` | 🔒 | Transaction history |
| `Calendar/Upcoming` | GET | `/get_recurring` | 🔒 | Upcoming recurring |
| `PieChart` | GET | `/get_stats` | 🔒 | Expenses by category |
| `BarChart` | GET | `/get_summary` | 🔒 | Income/expense totals |

### 11.3. Session Storage

- JWT token: `localStorage.setItem('token', access_token)` after login.
- Removed on: 401 from API, password/email change (frontend forces re-login).
- No backend logout endpoint — logout is client-side only.

### 11.4. TypeScript Types Expected by Frontend

```typescript
// Dashboard/Transakcja.tsx
interface Transakcja {
  id_transakcji: number;
  id_uzytkownika: number;
  id_kategorii: number | null;
  typ: 'wplyw' | 'wydatek';
  tytul: string;
  opis: string | null;
  kwota: number;
  metoda: string;
  konto: string | null;
  wlasciciel_konta: string | null;
  data: string;  // "YYYY-MM-DD"
}

// Dashboard/TPowtarzalna.tsx
interface TPowtarzalna {
  id_t_powtarzalnej: number;
  id_uzytkownika: number;
  id_kategorii: number | null;
  typ: 'wplyw' | 'wydatek';
  tytul: string;
  opis: string | null;
  kwota: number;
  metoda: string;
  konto: string | null;
  wlasciciel_konta: string | null;
  nastepny_termin: string;
  co_ile: string;
  czy_aktywna: boolean;
}
```

The .NET backend must return `kwota` as a JSON number (not a string) and `czy_aktywna` as a JSON boolean. Both serialize correctly from `decimal` and `bool` respectively.

---

## 12. HTTP Error Handling

### 12.1. Error Response Format

All error responses must use `{ "detail": "..." }` (no `ProblemDetails` format — the frontend parses the `detail` field directly):

```csharp
// Global error shape — use consistently across all controllers
private IActionResult DetailError(int statusCode, string message)
    => StatusCode(statusCode, new { detail = message });
```

### 12.2. Error Code Table

| Code | When | Response |
|------|------|----------|
| `200` | Success | JSON body matching the DTO schema |
| `400` | Business validation error (email taken, wrong password) | `{ "detail": "..." }` |
| `401` | Missing token, expired/invalid token, user not found | `{ "detail": "..." }` |
| `403` | Invalid login credentials | `{ "detail": "Nieprawidlowy email lub haslo" }` |
| `404` | Resource not found (`modify_recurring`) | `{ "detail": "Nie znaleziono transakcji" }` |
| `422` | Model validation failure (missing required field, wrong type) | `{ "errors": {...} }` — ASP.NET Core default |
| `500` | Server / DB error | Standard ASP.NET Core error response |

### 12.3. Suppress Default ProblemDetails

To keep 400/422 responses in the `{ "detail": "..." }` shape:

```csharp
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Validation error";
            return new BadRequestObjectResult(new { detail = errors });
        };
    });
```

---

## 13. Docker & Nginx Integration

### 13.1. `docker-compose.yml`

| Service | Host port | Description |
|---------|-----------|-------------|
| `db` | 5433 → 5432 | PostgreSQL, initialized from `db/skrypt.sql` |
| `backend` | 8000 | ASP.NET Core / .NET 8 |
| `frontend` | 80 | Nginx serving React static build |

```yaml
services:
  db:
    image: postgres:16
    ports: ["5433:5432"]
    environment:
      POSTGRES_DB: fin_calc_db
      POSTGRES_USER: fin_calc_api
      POSTGRES_PASSWORD: fin_calc_pass
    volumes:
      - ./db/skrypt.sql:/docker-entrypoint-initdb.d/skrypt.sql

  backend:
    build: ./FinCalBackend
    ports: ["8000:8000"]
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=fin_calc_db;Username=fin_calc_api;Password=fin_calc_pass
      - Jwt__SecretKey=your-long-secret-key
      - Jwt__ExpiresInMinutes=60
    depends_on:
      - db

  frontend:
    build: ./frontend
    ports: ["80:80"]
    depends_on:
      - backend
```

### 13.2. Nginx Proxy (`frontend/nginx.conf`)

```nginx
location /api/ {
    proxy_pass http://backend:8000/;
}
```

A request for `GET /api/kategorie` → backend receives `GET /kategorie`. This is unchanged from the original FastAPI setup.

### 13.3. Dockerfile (Backend)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY FinCalBackend.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8000
ENV ASPNETCORE_URLS=http://0.0.0.0:8000
ENTRYPOINT ["dotnet", "FinCalBackend.dll"]
```

---

## 14. Implementation Checklist

Use this list to build a backend compatible with the existing frontend and database from scratch.

### Database
- [ ] PostgreSQL 16
- [ ] Run `db/skrypt.sql` (tables + category seed data)
- [ ] Optionally: `db/user.sql` (API database user)

### Backend — Core
- [ ] .NET 8 ASP.NET Core Web API project (`dotnet new webapi`)
- [ ] NuGet packages: `Npgsql.EntityFrameworkCore.PostgreSQL`, `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] `AppDbContext` with all 4 `DbSet`s and model configuration
- [ ] `db.Database.EnsureCreated()` on startup
- [ ] CORS: `AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()`
- [ ] JWT Bearer authentication middleware configured with `ValidateIssuer = false`, `ValidateAudience = false`
- [ ] `DateOnly` JSON converter producing `YYYY-MM-DD` strings
- [ ] Error responses always `{ "detail": "..." }` for 4xx codes
- [ ] `RecurringProcessorService` registered as `IHostedService`

### Backend — Endpoints (exact paths, no prefix)
- [ ] `POST /register` 🔓
- [ ] `POST /login` 🔓
- [ ] `PUT /update_email` 🔒
- [ ] `PUT /update_password` 🔒
- [ ] `GET /kategorie` 🔓
- [ ] `GET /transakcje` 🔒
- [ ] `GET /wplywy` 🔒
- [ ] `GET /wydatki` 🔒
- [ ] `POST /add_payment` 🔒
- [ ] `POST /add_recurring` 🔒
- [ ] `GET /get_recurring` 🔒
- [ ] `PUT /modify_recurring/{id_t_powtarzalnej}` 🔒
- [ ] `GET /get_stats` 🔒
- [ ] `GET /get_summary` 🔒

### Backend — Business Rules
- [ ] `id_uzytkownika` on transaction/recurring INSERT always from JWT `sub` claim, never from body
- [ ] All reads filtered by `current_user.IdUzytkownika`
- [ ] `nastepny_termin` in `/get_recurring` sorted ASC
- [ ] `data` in `/transakcje`, `/wplywy`, `/wydatki` sorted DESC
- [ ] Background service runs every 12h + immediately on startup
- [ ] Scheduler creates ONE transaction per due entry per run (no backfill loop)
- [ ] `/get_stats`: INNER JOIN on `id_kategorii`, only `wydatek` type, GROUP BY `nazwa`
- [ ] `/get_summary`: SUM grouped by `typ` for logged-in user
- [ ] `modify_recurring`: verify record belongs to current user before update

### Frontend Contract
- [ ] Login returns `{ access_token, token_type, message }`
- [ ] Protected endpoints: `Authorization: Bearer <token>` header
- [ ] JWT `sub` claim = string of `id_uzytkownika`
- [ ] Dates in JSON: `YYYY-MM-DD` (no time component)
- [ ] `kwota` serialized as JSON number
- [ ] `czy_aktywna` serialized as JSON boolean
- [ ] Errors: `{ "detail": "..." }` for 400 / 401 / 403 / 404
- [ ] All JSON field names in Polish snake_case (use `[JsonPropertyName]`)

### Smoke Test

```bash
# 1. Register
curl -X POST http://localhost:8000/register \
  -H "Content-Type: application/json" \
  -d '{"imie":"Test","nazwisko":"User","email":"test@test.com","haslo":"pass123"}'

# 2. Login
TOKEN=$(curl -s -X POST http://localhost:8000/login \
  -H "Content-Type: application/json" \
  -d '{"imie":"","nazwisko":"","email":"test@test.com","haslo":"pass123"}' \
  | jq -r .access_token)

# 3. Categories (public)
curl http://localhost:8000/kategorie

# 4. Add expense
curl -X POST http://localhost:8000/add_payment \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id_kategorii":3,"typ":"wydatek","tytul":"Test","opis":"opis","kwota":100,"metoda":"gotowka","data":"2026-06-15"}'

# 5. Stats
curl -H "Authorization: Bearer $TOKEN" http://localhost:8000/get_stats
curl -H "Authorization: Bearer $TOKEN" http://localhost:8000/get_summary
```

---

## Data Flow Summary

```
┌─────────────┐     JSON/HTTP      ┌───────────────────┐     EF Core      ┌────────────┐
│   Frontend  │ ◄──────────────►  │  ASP.NET Core 8   │ ◄─────────────► │ PostgreSQL │
│   (React)   │   Bearer JWT      │  Web API Backend  │                  │            │
└─────────────┘                   └────────┬──────────┘                  └────────────┘
                                           │
                               RecurringProcessorService
                               (IHostedService — every 12h)
                                           │
                                           ▼
                                 t_t_powtarzalne ──► t_transakcje
                                 (auto-generation)
```

**System input (from user):** registration, login, transactions, recurring entries, account settings.  
**System output (to user):** transaction lists, categories, statistics, JWT token.  
**Internal flow:** the `RecurringProcessorService` moves data from `t_t_powtarzalne` to `t_transakcje` without frontend involvement.

---

*Documentation based on the fin_cal project source (FastAPI backend + React frontend), adapted for .NET 8 ASP.NET Core Web API. Version: June 2026.*
