# MyApp — E-commerce Order Management API

.NET 8 Web API layihəsi: JWT authentication, role-based authorization, transaction-safe order yaratma, concurrency-safe stock idarəetməsi və status keçid qaydaları ilə.

## Texnologiyalar

- .NET 8 Web API
- Entity Framework Core 8 + PostgreSQL (Npgsql)
- JWT Bearer Authentication
- FluentValidation
- Swagger / OpenAPI
- xUnit + FluentAssertions (Unit Tests)
- Docker + Docker Compose (konfiqurasiya hazırdır, bax "Docker" bölməsi)

## Layihə Strukturu

MyApp/
- MyApp.Api/              Controllers, Program.cs, Middleware
- MyApp.Application/      DTOs, Validators, Interfaces, Business rules
- MyApp.Domain/           Entities (Domain Model)
- MyApp.Infrastructure/   DbContext, Configurations, JWT Service
- MyApp.Tests/            Unit Tests

## Qurulma (Local)

### Tələblər
- .NET 8 SDK
- PostgreSQL 16+ (lokal quraşdırılmış)

### Addımlar

1. Repository-ni klonla:
git clone repo-url
cd MyApp

2. MyApp.Api/appsettings.json-da connection string-i öz PostgreSQL məlumatlarınla yenilə.

3. Migration-ları tətbiq et:
dotnet ef database update --project MyApp.Infrastructure --startup-project MyApp.Api

4. API-ni işə sal:
cd MyApp.Api
dotnet run

5. Swagger UI: http://localhost:5174/swagger

## Docker

Layihədə Dockerfile və docker-compose.yml mövcuddur:
docker compose up --build

Migration-lar API başlayanda avtomatik tətbiq olunur.

Qeyd: Bu konfiqurasiya lokal mühitdə Docker Desktop-ın texniki məhdudiyyəti səbəbindən tam test edilə bilməyib, amma Dockerfile standart multi-stage build strukturunu izləyir.

## Testlərin İşə Salınması

dotnet test

## Authentication

- POST /api/auth/register - Yeni istifadəçi qeydiyyatı (default rol: Customer)
- POST /api/auth/login - Giriş, JWT token qaytarır

Admin rolunu təyin etmək üçün verilənlər bazasında:
UPDATE "Users" SET "Role" = 1 WHERE "Email" = 'admin@example.com';

## Əsas Endpoint-lər

GET /api/products - Hər kəs - Pagination, search, filter, sorting
POST /api/products - Admin
PUT /api/products/id - Admin
DELETE /api/products/id - Admin (soft delete)
POST /api/orders - Customer/Admin
GET /api/orders - Customer: öz sifarişləri, Admin: hamısı
PUT /api/orders/id/status - Admin

## Həll Edilmiş Əsas Problemlər

### Transaction-safe Order yaratma
Sifariş yaradılması BeginTransactionAsync daxilində aparılır, xəta zamanı RollbackAsync ilə geri qaytarılır.

### Concurrency həlli
Stock azaltma atomic SQL sorğusu ilə aparılır: UPDATE Products SET Stock = Stock - qty WHERE Id = id AND Stock >= qty. Bu, paralel sifarişlərdə stock mənfiyə düşməsinin qarşısını alır.

### Order Status Transition qaydaları
OrderStatusTransitionRules icazə verilən keçidləri idarə edir: Pending -> Confirmed/Cancelled, Confirmed -> Shipped/Delivered/Cancelled, Shipped -> Delivered/Cancelled, Delivered/Cancelled terminal statuslardır.

### Global Exception Handling
GlobalExceptionMiddleware bütün xətaları mərkəzləşdirilmiş JSON formatında qaytarır.

### Authorization
Admin-only endpoint-lər [Authorize(Roles = "Admin")] ilə qorunur. Customer yalnız öz sifarişlərini görə bilər.

## Buraxılmış Hissələr

- Bonus bölmə (Redis, background job, Idempotency-Key) tətbiq olunmayıb.
- Docker konfiqurasiyası lokal mühitdə tam test edilə bilməyib.
