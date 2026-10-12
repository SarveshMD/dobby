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
  > Skipped `User` model for now
- [ ] **Phase 2: Base62 Encoding Engine**
  - Implement bidirectional Base62 converter (numeric ID $\leftrightarrow$ short string)
  - Write unit tests verifying edge cases, boundary collisions, and reversibility
- [ ] **Phase 3: The Teleportation Engine (Redirection)**
  - Implement `GET /{code}` redirect endpoint
  - Return HTTP 302 Found (or 301 Moved Permanently)
  - Increment click counters asynchronously
- [ ] **Phase 4: Creation & Validation API**
  - Implement `POST /api/urls` with FluentValidation for URL URI schemes
  - Handle idempotency (return existing short code if identical long URL exists)
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
```bash
# Clone the repository
git clone https://github.com/SarveshMD/dobby.git
cd dobby

# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update

# Launch the API
dotnet run
```

Gemini wrote this.
