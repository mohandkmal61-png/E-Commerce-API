# E-Commerce API agent guide

This repository is an ASP.NET Core 9 API with a clean-architecture layout, but it also contains duplicate legacy folders and a few consistent spelling typos that should be preserved when extending code.

## What matters most

- Primary codebase: `ECommerce.*` folders and namespaces
- Legacy duplicates also exist under `E-Commerce.*`, but they should not be treated as the main source of truth
- The app entry point is `E-CommereceAPI/Program.cs`
- The current API uses dependency injection, EF Core, Identity, AutoMapper, and Redis-backed basket support

## Architecture map

- API host: `E-CommereceAPI/`
- Domain: `ECommerce.Domin/`
- Persistence: `ECommerce.Persistence/`
- Presentation: `ECommerce.Presentation/`
- Services: `ECommerce.Services/`
- Service contracts: `ECommerce.ServicesAbstraction/`

## Project conventions to keep

- Use the current naming style already present in code, even when it is misspelled:
  - `Servise` instead of `Service`
  - `Dbcontest` instead of `DbContext`
  - `Confugrations` instead of `Configurations`
  - `Domin` instead of `Domain`
- Prefer `AddScoped` registrations in `Program.cs` for services and infrastructure
- Keep repository interfaces in `ECommerce.Domin/Contracts/` and implementations in `ECommerce.Persistence/Repositoris/`
- Keep controllers thin and push business logic into services
- Use `async`/`Task<T>` patterns for EF Core and database work
- Prefer AutoMapper profiles from `ECommerce.Services/MapingProfile/`

## Important runtime setup

`E-CommereceAPI/Program.cs` is the key file for configuration. It registers:

- MVC controllers and Swagger/OpenAPI
- SQL Server DbContexts for application data and Identity
- ASP.NET Identity with EF Core stores
- `IAuthenticationService`, `IProductServise`, `IBasketServise`, `IUnitOfWork`
- AutoMapper profiles
- Redis basket extension
- Data seeding and migration execution on startup

## Build and validation guidance

- Standard solution build: `dotnet build "E-Commerece.sln"`
- No dedicated test project was found in the workspace root during repo review, so validation should rely on the solution build unless a new test project is added

## Coding rules for AI agents

1. Match the existing repo structure before introducing new abstractions.
2. Prefer `ECommerce.*` over `E-Commerce.*` when creating or editing code.
3. Follow the naming typos already used in this project instead of “correcting” them globally.
4. Keep changes local to the relevant module unless the pattern clearly requires cross-cutting updates.
5. When adding a new entity, service, or controller, mirror the established pattern in the surrounding folder instead of inventing a new architecture style.
6. For API work, update the controller, service, and relevant contract together so the DI and interface flow remains consistent.

## Useful entry points

- `E-CommereceAPI/Program.cs` — startup, DI, app pipeline, data seeding
- `ECommerce.Domin/Contracts/` — repository contracts
- `ECommerce.Persistence/Repositoris/` — data access implementations
- `ECommerce.Services/` — business logic and AutoMapper profiles
- `ECommerce.Presentation/Controler/` — HTTP endpoints
- `ECommerce.ServicesAbstraction/` — public service contracts and result wrappers

## Quick summary for language-model work

When generating or editing code in this repo:

- do not rewrite the project toward cleaner names unless a specific file is intentionally being refactored
- keep the existing conventions and folder layout intact
- maintain compatibility with the ASP.NET Core DI and Identity patterns already in use
- prefer minimal, pattern-matching changes over broad architectural cleanup
