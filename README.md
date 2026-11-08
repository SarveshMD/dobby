# Dobby 🧦

> _"Dobby has no master. Dobby is a free URL shortener."_

A high-performance, authenticated URL shortener built with ASP.NET Core Minimal APIs, PostgreSQL, and Redis caching. Dobby takes sprawling web addresses and redirects callers instantly to their destination using compact Base62 encoded 6-character codes.

---

## Architecture Overview

- **Runtime**: .NET 10.0 (ASP.NET Core Minimal APIs)
- **Database**: PostgreSQL 17 via Entity Framework Core
- **Cache**: Redis 7.4
- **Identity & Security**: Custom JWT Bearer Auth with BCrypt password hashing
- **Encoding Scheme**: Base62 (`[0-9a-zA-Z]`) mapping numeric sequences to 6-character short codes

---

## Roadmap & Implementation Order

- [x] **Phase 1: Project Setup & Domain Modeling**
  - Initialize clean ASP.NET Core Web API solution
  - Configure EF Core with PostgreSQL and snake_case naming conventions
  - Define `ShortenedUrl` and `User` models with relational integrity
- [x] **Phase 2: Base62 Encoding Engine**
  - Implement bidirectional Base62 converter (numeric ID <-> short string)
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
- [x] **Phase 5: High-Performance Caching**
  - Integrate Redis cache on the `GET /{code}` path (Cache-Aside pattern)
  - Benchmark database vs. cache-hit redirect response latency
- [x] **Phase 6: Auth & Analytics Integration**
  - Port zero-trust JWT Bearer authentication pipeline
  - Scope URL management and click metrics (`GET /api/urls/my`) to authenticated user identity

---

## API Endpoints

### Public

| Method | Endpoint  | Description                                 | Status Codes    |
| :----- | :-------- | :------------------------------------------ | :-------------- |
| `GET`  | `/{code}` | Redirects immediately to target destination | 302 Found / 404 |

### URL Management

| Method   | Endpoint         | Auth              | Description                                   | Response                   | Status Codes                            |
| :------- | :--------------- | :---------------- | :-------------------------------------------- | :------------------------- | :-------------------------------------- |
| `POST`   | `/api/urls`      | Optional / Bearer | Shortens a URL (anonymous or logged-in)       | `ResponseUrlItemDto`       | 201 Created                             |
| `GET`    | `/api/urls/my`   | Bearer            | Returns all shortened URLs for logged-in user | `List<ResponseUrlItemDto>` | 200 Ok / 401 Unauthorized               |
| `DELETE` | `/api/urls/{id}` | Bearer            | Removes a short URL (scoped to owner)         | -                          | 204 No Content / 401 Unauthorized / 404 |

> TODO: | `GET` | `/api/urls/{id}` | Bearer | Returns details of the specified URL Item | `ResponseUrlItemDto` | 200 Ok / 401 Unauthorized / 404 |

### Identity

| Method | Endpoint             | Description                                        | Response          | Status Codes               |
| :----- | :------------------- | :------------------------------------------------- | :---------------- | :------------------------- |
| `POST` | `/api/auth/register` | Creates a new user profile with hashed credentials | `AuthResponseDto` | 201 Created / 409 Conflict |
| `POST` | `/api/auth/login`    | Validates credentials and issues signed JWT        | `AuthResponseDto` | 200 Ok / 401 Unauthorized  |

---

## Running via Docker (Quick Start)

### Prerequisites

- [Docker Engine / Docker Desktop with Compose](https://docs.docker.com/get-started/get-docker/)

### Setup

1. Clone the repo:

```bash
git clone https://github.com/sarveshmd/dobby.git
cd dobby
```

2. Copy `.env.example` to `.env` and replace the placeholder secrets

- Set `DOBBY_IMAGE_TAG` to `latest`, or [pick one](https://hub.docker.com/r/sarveshmd/dobby/tags)

3. Launch the compose stack:

```bash
docker compose -f compose.yml -f compose.prod.yml up -d
```

Alternatively, use docker to build the app locally from source and run it in developer mode:

- Interactive API Docs: Exposes the [`/scalar`](http://localhost:5000/scalar) endpoint.
- Host Database Access: Maps PostgreSQL (`5432`) and Redis (`6379`) to your host so you can inspect them using clients like pgAdmin, psql, redis-cli or Redis Insight.

```bash
docker compose -f compose.yml -f compose.dev.yml up -d --build
```

--

## Local Development (Running & Testing from Source)

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Docker (recommended for running PostgreSQL 17 and Redis 7.4 via compose)](https://docs.docker.com/get-started/get-docker/)
  - Alternative (Local Download):
    - [PostgreSQL](https://www.postgresql.org/)
    - [Redis](https://redis.io/)

### Setup

1. Clone the repository:

```bash
git clone https://github.com/SarveshMD/dobby.git
cd dobby
```

2. Start backing services:

```bash
docker compose -f compose.yml -f compose.dev.yml up postgres redis -d

# Exposes PostgreSQL (5432) and Redis (6379) so you can connect directly with psql, redis-cli, pgAdmin or Redis Insight
```

3. Apply migrations:

```bash
docker compose -f compose.yml -f compose.dev.yml up db-migrator
```

4. Configure local environment

Store the required local secrets with .NET User Secrets:

```bash
dotnet user-secrets init --project src/Dobby
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dobby_db;Username=<your-username>;Password=<your-password>" --project src/Dobby
dotnet user-secrets set "UrlShortener:SecretKey" "<your-shortener-secret>" --project src/Dobby
dotnet user-secrets set "Jwt:Key" "<your-jwt-secret>" --project src/Dobby
dotnet user-secrets set "Redis:ConnectionString" "localhost:6379" --project src/Dobby
```

5. Run the API locally:

```bash
dotnet run --project src/Dobby

# Interactive API Docs available at http://localhost:5000/scalar
```

To enable hot-reload:

```bash
dotnet watch --project src/Dobby
```

The `.env` is read by Docker Compose only. The application does not read it.

--

## Unit and Integration Tests

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Docker](https://docs.docker.com/get-started/get-docker/) (to support Testcontainers)

From the project's root `dobby/`:

```bash
dotnet test
```
