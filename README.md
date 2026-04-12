# Saints & Miracles

Saints & Miracles is a full-stack web platform for exploring saints, miracles, and prayers, with a role-based admin area for content and account management.

## Features

- Public pages for saints, miracles, and prayers
- Detail pages with markdown-rendered long-form content
- Search, filtering, sorting, and pagination across catalog pages
- Home highlights with saints of the day and upcoming feasts
- Account system with login, invite-based registration, email confirmation, and password reset
- Admin dashboard with totals and recent activity
- Admin CRUD for saints, miracles, prayers, tags, and religious orders
- SuperAdmin-only account management and invite token generation

## Tech Stack

- Frontend: Angular 19, Angular Material, Tailwind CSS
- Backend: ASP.NET Core 9 Web API, ASP.NET Identity (cookie auth), Serilog
- Database: MySQL 8 with Entity Framework Core (Pomelo)
- DevOps: Docker Compose, GitHub Actions CI

## Repository Structure

```text
.
|-- Client/                # Angular SPA
|-- Server/
|   |-- API/               # ASP.NET Core API + static SPA hosting
|   |-- Core/              # Domain models, DTOs, interfaces, validation
|   |-- Infrastructure/    # EF Core context, repositories, services, seed data
|   `-- Tests/             # API, Core, and Infrastructure test suites
|-- docker-compose.yml
`-- SaintsAndMiracles.sln
```

## Architecture Overview

- The Angular app runs as a SPA in development and is served from `wwwroot` by the API in production.
- The backend exposes REST endpoints under `api/*`.
- Authentication uses ASP.NET Identity cookies.
- Data is stored in MySQL via EF Core repositories/services.

## Getting Started

### Prerequisites

- Node.js 20+
- .NET SDK 9.0+
- MySQL 8+ (or Docker)

### 1. Clone and install dependencies

```bash
git clone <your-repo-url>
cd SaintsAndMiracles
cd Client && npm install
cd ..
dotnet restore SaintsAndMiracles.sln
```

### 2. Configure environment variables

Set these values for local/dev or containerized runs:

- `ConnectionStrings__DefaultConnection`
- `Frontend__BaseUrl`
- `Smtp__Host`
- `Smtp__Port`
- `Smtp__User`
- `Smtp__Pass`
- `Smtp__From`
- `Smtp__FromName`
- `Bootstrap__Enabled`
- `Bootstrap__TokenTtlHours`

Note: never commit real secrets. Use placeholders in config files and inject credentials via environment variables.

### 3. Run in development mode

Frontend:

```bash
cd Client
npm start
```

Backend:

```bash
dotnet run --project Server/API/API.csproj
```

By default:

- Frontend: `http://localhost:4200`
- Backend: `http://localhost:5215` (or configured ASP.NET URL)

## Run with Docker

```bash
docker compose up --build
```

Services:

- API: `http://localhost:8080`
- MySQL: internal container network + persisted `mysql_data` volume

## Testing

Frontend tests:

```bash
cd Client
npm test -- --watch=false --browsers=ChromeHeadless
```

Backend tests:

```bash
dotnet test Server/Tests/API.Tests/API.Tests.csproj --framework net9.0
dotnet test Server/Tests/Core.Tests/Core.Tests.csproj --framework net9.0
dotnet test Server/Tests/Infrastructure.Tests/Infrastructure.Tests.csproj --framework net9.0
```

## CI

GitHub Actions workflow (`.github/workflows/ci.yml`) runs:

- Angular install, test, and production build
- .NET restore and release build
- API/Core/Infrastructure test projects

## Security Notes

- Do not commit real SMTP or database credentials.
- Rotate any secrets that may have been exposed.
- Use environment variables or a secret manager for production.

## Deployment Cache Behavior (No Hard Refresh Required)

To prevent users from needing Ctrl+F5 after each deployment, the API now serves cache headers with this strategy:

- `index.html` (and SPA fallback HTML): `Cache-Control: no-cache, no-store, must-revalidate`
- Hashed Angular bundles (`*.js`, `*.css` with hash in filename): `Cache-Control: public, max-age=31536000, immutable`

Why this works:

- Angular production builds use hashed filenames (`outputHashing: all`), so new deploys generate new bundle names.
- Browsers always revalidate/reload `index.html`, so they pick up new bundle names immediately.
- Bundles stay highly cacheable for performance and are safely invalidated by filename changes.

### How to test cache headers

After deploying, verify headers from your public URL:

```bash
curl -sI https://your-domain/ | grep -iE 'cache-control|pragma|expires'
curl -s https://your-domain/ | grep -oE '(main|polyfills|styles)-[A-Z0-9]+\.(js|css)' | head -n 1
curl -sI https://your-domain/<bundle-from-previous-command> | grep -i 'cache-control'
```

Expected results:

- `/` returns `no-cache, no-store, must-revalidate` (or equivalent no-store policy).
- Hashed bundle returns `public, max-age=31536000, immutable`.

### How to test deployment behavior end-to-end

1. Open the app in a normal browser window (not incognito) and authenticate.
2. Deploy a new release (`ng build` + docker image/container update as you already do).
3. In the existing tab, do a regular refresh (F5) or navigate to another route and back.
4. Confirm the app loads without Ctrl+F5.
5. Confirm the auth session remains valid.
6. Confirm unsaved form state is not lost due to forced hard refresh.

If you still see old UI after deploy, check for an external cache/CDN layer overriding origin headers.

## Contributing

Contributions are welcome.

1. Open an issue to discuss major changes.
2. Create a feature branch.
3. Add or update tests for behavior changes.
4. Run formatting/tests before submitting a pull request.

## License

This project is licensed under the MIT License. See `LICENSE` for details.
