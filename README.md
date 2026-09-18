# MyApp — E-commerce Order Management API

.NET 8 Web API layihəsi: JWT authentication, role-based authorization, transaction-safe order yaratma, concurrency-safe stock idarəetməsi, status keçid qaydaları, kupon/endirim sistemi və idempotent ödəniş mexanizmi ilə.

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
- MyApp.Infrastructure/   DbContext, Configurations, JWT/Payment/Idempotency Services
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

(35 test: order status transition, validation qaydaları, coupon discount hesablama, payment status transition, mock payment service)

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
POST /api/orders - Customer/Admin - Sifariş yarat
GET /api/orders - Customer: öz sifarişləri, Admin: hamısı
POST /api/orders/id/apply-coupon - Customer/Admin - Kupon tətbiq et
PUT /api/orders/id/status - Admin - Status dəyiş
POST /api/coupons - Admin - Yeni kupon yarat
GET /api/coupons - Admin - Bütün kuponlar
POST /api/payments/orders/id/pay - Customer/Admin - Ödəniş et (Idempotency-Key header tələb olunur)
GET /api/payments/id - Sahib/Admin - Ödəniş məlumatı

## Həll Edilmiş Əsas Problemlər

### Transaction-safe Order yaratma
Sifariş yaradılması BeginTransactionAsync daxilində aparılır, xəta zamanı RollbackAsync ilə geri qaytarılır.

### Concurrency həlli
Stock azaltma atomic SQL sorğusu ilə aparılır: UPDATE Products SET Stock = Stock - qty WHERE Id = id AND Stock >= qty. Bu, paralel sifarişlərdə stock mənfiyə düşməsinin qarşısını alır. Eyni yanaşma kupon istifadə sayğacı (UsedCount) üçün də tətbiq olunub.

### Order Status Transition qaydaları
OrderStatusTransitionRules icazə verilən keçidləri idarə edir: Pending -> Confirmed/Cancelled, Confirmed -> Shipped/Delivered/Cancelled, Shipped -> Delivered/Cancelled, Delivered/Cancelled terminal statuslardır.

### Coupon / Endirim Sistemi
CouponDiscountCalculator (pure, test edilə bilən sinif) kuponun aktivliyini, bitmə tarixini, istifadə limitini və minimum sifariş məbləğini yoxlayır, faiz və ya sabit məbləğ əsasında endirimi hesablayır. Endirim sifariş məbləğini keçə bilməz (0-a düşür, mənfiyə düşmür).

### Payment Modeli və Statusları
Payment entity-si Pending -> Completed/Failed -> Refunded keçidlərini PaymentStatusTransitionRules ilə idarə edir. MockPaymentService real ödəniş sistemini simulyasiya edir (müsbət məbləğ üçün uğurlu, sıfır/mənfi üçün uğursuz).

### Order-Payment İnteqrasiyası
Ödəniş uğurlu olduqda sifarişin statusu avtomatik Confirmed-ə keçir və OrderStatusHistory-ə qeyd əlavə olunur. Artıq ödənilmiş və ya Pending olmayan sifarişlərə təkrar ödəniş qəbul edilmir.

### Idempotency-Key Mexanizmi
Hər ödəniş sorğusu Idempotency-Key HTTP header-i tələb edir. Açar ayrıca IdempotencyKey cədvəlində unique constraint ilə saxlanılır - eyni açarla təkrar sorğu gələrsə, əməliyyat təkrarlanmır, saxlanmış cavab birbaşa qaytarılır. Bu, şəbəkə xətası səbəbindən eyni ödənişin iki dəfə aparılmasının qarşısını alır.

### Global Exception Handling
GlobalExceptionMiddleware bütün xətaları mərkəzləşdirilmiş JSON formatında qaytarır.

### Authorization
Admin-only endpoint-lər [Authorize(Roles = "Admin")] ilə qorunur. Customer yalnız öz sifarişlərini və ödənişlərini görə bilər.

## Buraxılmış Hissələr

- Redis caching və background notification job tətbiq olunmayıb - vaxt məhdudiyyəti səbəbindən, Idempotency-Key mexanizmi bonus olaraq seçilib və tam tətbiq olunub.
- Docker konfiqurasiyası lokal mühitdə tam test edilə bilməyib.
