# Dobby 🧦

> *"Dobby has no master. Dobby is a free URL shortener."*

A high-performance, authenticated URL shortener and redirect engine built with ASP.NET Core Minimal APIs, PostgreSQL, and Redis caching. Dobby takes sprawling web addresses and teleports callers instantly to their destination using compact Base62 encoding.

---

## Architecture Overview

- **Runtime**: .NET 8 / 9 (ASP.NET Core Minimal APIs)
- **Database**: PostgreSQL via Entity Framework Core
- **Cache**: Redis / Distributed Cache (sub-millisecond redirects)
- **Identity & Security**: Custom JWT Bearer Auth with BCrypt password hashing
- **Encoding Scheme**: Base62 (`[0-9a-zA-Z]`) mapping numeric sequences to short codes

---

## Roadmap & Implementation Order

- [x] **Phase 1: Project Setup & Domain Modeling**
  - Initialize clean ASP.NET Core Web API solution
  - Configure EF Core with PostgreSQL and snake_case naming conventions
  - Define `ShortenedUrl` and `User` models with relational integrity
- [x] **Phase 2: Base62 Encoding Engine**
  - Implement bidirectional Base62 converter (numeric ID $\leftrightarrow$ short string)
  - Write unit tests verifying edge cases, boundary collisions, and reversibility
- [x] **Phase 3: The Teleportation Engine (Redirection)**
  - Implement `GET /{code}` redirect endpoint
  - Return HTTP 302 Found (or 301 Moved Permanently)
  - Increment click counters asynchronously
  > Skipped click counters (TODO: analytics after User model)
- [x] **Phase 4: Creation & Validation API**
  - Implement `POST /api/urls` with FluentValidation for URL URI schemes
  - Handle idempotency (return existing short code if identical long URL exists)
  > Skip idempotency handling, (tradeoff memory for write speed - handling idempotency will take an extra read for every write)
- [ ] **Phase 5: High-Performance Caching**
  - Integrate Redis cache on the `GET /{code}` path (Cache-Aside pattern)
  - Benchmark database vs. cache-hit redirect response latency
- [ ] **Phase 6: Auth & Analytics Integration**
  - Port zero-trust JWT Bearer authentication pipeline
  - Scope URL management and click metrics (`GET /api/urls/my`) to authenticated user identity

---

## API Contract Preview

### Public / Teleportation
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/{code}` | Redirects immediately to target destination |

### URL Management
| Method | Endpoint | Auth | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/urls` | Optional / Bearer | Shortens a URL (anonymous or claimed) |
| `GET` | `/api/urls/my` | Bearer | Returns all shortened URLs for logged-in user |
| `DELETE` | `/api/urls/{id}` | Bearer | Removes a short URL (scoped to owner) |

### Identity
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Creates a new user profile with hashed credentials |
| `POST` | `/api/auth/login` | Validates credentials and issues signed JWT |

---

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/)
- [PostgreSQL](https://www.postgresql.org/)
- [Redis](https://redis.io/) (via Docker or local install)

### Setup
```powershell
# Clone the repository
git clone https://github.com/SarveshMD/dobby.git
cd dobby

# Restore dependencies
dotnet restore
Copy-Item .env.example .env
```

Replace the placeholder values in `.env`, then start PostgreSQL for the local development loop:

```powershell
docker compose up -d postgres
```

The API itself runs locally for fast reload and debugger support. Store its local secrets with .NET User Secrets:

```powershell
dotnet user-secrets init --project src/Dobby
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dobby_db;Username=sarvesh;Password=<your-password>" --project src/Dobby
dotnet user-secrets set "UrlShortener:SecretKey" "<your-shortener-secret>" --project src/Dobby
dotnet user-secrets set "Jwt:Key" "<your-jwt-secret>" --project src/Dobby
```

Run the API locally:

```powershell
dotnet watch --project src/Dobby --launch-profile http
```

To verify the complete containerized setup instead:

```powershell
docker compose up -d --build
```

Run database migrations against the local PostgreSQL container with the local API configuration:

```powershell
dotnet ef database update --project src/Dobby
```

The root `.env` is read by Docker Compose only. The application does not parse `.env` files itself.

Gemini wrote this.
