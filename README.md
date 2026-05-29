# Freelance Marketplace Platform — Backend API

ASP.NET Core Web API для платформи фріланс-маркетплейсу. Реалізує повний цикл: реєстрація та автентифікація (JWT + Google OAuth), управління користувачами/профілями, проєктами, заявками/квотами, контрактами та майлстоунами, повідомленнями, відгуками, спорами, сповіщеннями в реальному часі (SignalR), а також **гаманцем користувача та депозитами через Stripe**.

---

## 🚀 Стек технологій

| Категорія | Технологія |
|---|---|
| Framework | ASP.NET Core 8 (.NET 8) |
| Мова | C# |
| ORM | Entity Framework Core 8 + Npgsql |
| База даних | PostgreSQL 16 |
| CQRS / Mediator | MediatR 13 |
| Валідація | FluentValidation 12 |
| Маппінг | AutoMapper 14 |
| Автентифікація | JWT Bearer + Google OAuth 2.0 |
| Payments | Stripe (PaymentIntents) |
| Real-time | SignalR (`NotificationHub`) |
| Документація | Swagger / OpenAPI (Swashbuckle) |
| Логування | Serilog (Console sink) |
| DI / scan | Scrutor |
| Контейнеризація | Docker + Docker Compose |
| CI/CD | GitHub Actions → GHCR |
| Тестування | xUnit (Integration Tests) |

---

## 📁 Структура проекту

Актуальна структура репозиторію:

```
freelance-api/
├── src/
│   ├── API/        # Presentation layer — Controllers, Program.cs, Middleware
│   ├── BLL/        # Business Logic — Commands/Queries (CQRS), Services, Hubs, ViewModels
│   ├── DAL/        # Data Access — AppDbContext, Repositories, Migrations
│   └── Domain/     # Domain models, enums, base abstractions
├── tests/
│   ├── Api.Tests.Integration/   # Integration tests (xUnit + WebApplicationFactory)
│   ├── Tests.Common/            # BaseIntegrationTest, TestFactory, helpers
│   └── TestsData/               # Фікстури та тестові дані
├── .github/workflows/ci-cd.yml  # CI/CD pipeline (Build → Test → Docker Push)
├── Dockerfile
├── docker-compose.yml
└── .env.example
```

---

## 🔐 Автентифікація та ролі

### Ролі
| Роль | Опис |
|---|---|
| `admin` | Повний доступ до системи |
| `employer` | Управління проєктами/контрактами |
| `freelancer` | Подача заявок, виконання контрактів, портфоліо |
| `moderator` | Розгляд та вирішення спорів |

### Авторизаційні політики
| Політика | Ролі |
|---|---|
| `AdminOrEmployer` | admin, employer |
| `AdminOrFreelancer` | admin, freelancer |
| `AdminOrModerator` | admin, moderator |
| `AnyAuthenticated` | будь-яка авторизована роль |

### Підтримувані методи входу
- **JWT** — стандартний sign-up / sign-in
- **Google OAuth 2.0** — зовнішній вхід через Google

---

## 🔌 API Контролери (актуальні endpoints)

> У проекті використано `[Route("[controller]")]`, тобто базовий шлях — назва контролера без суфікса `Controller`.

### **AccountController** `/Account`
| Метод | Endpoint | Доступ |
|---|---|---|
| POST | `/Account/sign-up` | Публічний |
| POST | `/Account/sign-in` | Публічний |
| POST | `/Account/external-login` | Публічний (Google OAuth) |

---

### **UserController** `/User`
> Контролер під JWT-авторизацією за замовчуванням.

| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/User/get-myself` | Авторизований |
| PATCH | `/User/update-avatar` | Авторизований |
| GET | `/User/roles` | Авторизований |
| GET | `/User/proficiency-levels` | Авторизований |
| POST | `/User/languages` | Авторизований |
| PUT | `/User/languages` | Авторизований |
| DELETE | `/User/languages/{languageId}` | Авторизований |
| PUT | `/User` | Авторизований (оновлення власного профілю) |
| GET | `/User` | Admin only |
| GET | `/User/{id}` | Admin only |
| GET | `/User/paginated` | Admin only |
| POST | `/User` | Admin only |
| PUT | `/User/{id}` | Admin only |
| DELETE | `/User/{id}` | Admin only |

---

### **FreelancerController** `/Freelancer`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Freelancer` | Admin / Freelancer |
| GET | `/Freelancer/{id}` | Admin / Freelancer |
| GET | `/Freelancer/{email}` | Admin / Freelancer |
| PUT | `/Freelancer` | Admin / Freelancer |
| PUT | `/Freelancer/skills` | Admin / Freelancer |

---

### **EmployerController** `/Employer`
> Контролер під JWT-авторизацією за замовчуванням та policy `AdminOrEmployer`, але має **публічні** GET-ендпоїнти.

| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Employer` | Admin / Employer (поточний employer за токеном) |
| PUT | `/Employer` | Admin / Employer (оновлення власного профілю employer) |
| GET | `/Employer/{id}` | **Публічний** (пошук employer за `id:guid`) |
| GET | `/Employer/{email}` | **Публічний** (пошук employer за email) |

---

### **ProjectController** `/Project`
> За замовчуванням вимагає JWT + policy `AdminOrEmployer`, але частина ендпоїнтів **відкрита**.

| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Project` | **Публічний** |
| GET | `/Project/{id}` | **Публічний** |
| GET | `/Project/search` | **Публічний** (фільтрація + пагінація) |
| GET | `/Project/by-contract/{contractId}` | **Публічний** |
| PATCH | `/Project/categories/{projectId}` | Admin / Employer |
| GET | `/Project/by-employer` | Admin / Employer |
| POST | `/Project` | Admin / Employer |
| PUT | `/Project/{id}` | Admin / Employer |
| DELETE | `/Project/{id}` | Admin / Employer |

---

### **ProjectMilestoneController** `/ProjectMilestone`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/ProjectMilestone/{id}` | Admin / Employer |
| POST | `/ProjectMilestone` | Admin / Employer |
| PUT | `/ProjectMilestone/{id}` | Admin / Employer |
| DELETE | `/ProjectMilestone/{id}` | Admin / Employer |
| GET | `/ProjectMilestone/by-project/{projectId}` | Публічний |
| GET | `/ProjectMilestone/milestone-status-enums` | Admin / Employer |

---

### **BidController** `/Bid`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Bid/{id}` | Публічний |
| GET | `/Bid/by-project/{projectId}` | Публічний |
| POST | `/Bid` | Admin / Freelancer |
| PUT | `/Bid/{id}` | Admin / Freelancer |
| DELETE | `/Bid/{id}` | Admin / Freelancer |

---

### **QuoteController** `/Quote`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Quote/{id}` | Публічний |
| GET | `/Quote/by-project/{projectId}` | Публічний |
| POST | `/Quote` | Admin / Freelancer |
| PUT | `/Quote/{id}` | Admin / Freelancer |
| DELETE | `/Quote/{id}` | Admin / Freelancer |

---

### **ContractController** `/Contract`
| Метод | Endpoint | Доступ |
|---|---|---|
| POST | `/Contract/{quoteId}` | Employer only |
| PUT | `/Contract` | Employer only |
| PUT | `/Contract/update-status/{contractId}` | Employer only |
| GET | `/Contract/status-enums` | Авторизований |
| GET | `/Contract/by-user` | Авторизований |
| GET | `/Contract/completed-by-freelancer-id/{freelancerId}` | Авторизований |

---

### **ContractMilestoneController** `/ContractMilestone`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/ContractMilestone/{id}` | Авторизований |
| POST | `/ContractMilestone` | Авторизований |
| PUT | `/ContractMilestone/{id}` | Авторизований |
| DELETE | `/ContractMilestone/{id}` | Авторизований |
| GET | `/ContractMilestone/by-contract/{contractId}` | Публічний |
| GET | `/ContractMilestone/milestone-status-enums` | Авторизований |
| GET | `/ContractMilestone/status-freelancer-enums` | Авторизований |
| GET | `/ContractMilestone/status-employer-enums` | Авторизований |
| PUT | `/ContractMilestone/status/{id}/freelancer` | Freelancer only |
| PUT | `/ContractMilestone/status/{id}/employer` | Employer only |

---

### **MessageController** `/Message`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Message/{id}` | Авторизований |
| POST | `/Message` | Авторизований |
| PUT | `/Message/{id}` | Авторизований |
| DELETE | `/Message/{id}` | Авторизований |
| POST | `/Message/without-contract` | Авторизований |
| GET | `/Message/by-user` | Авторизований |
| GET | `/Message/by-contract/{contractId}` | Авторизований |

---

### **ReviewController** `/Review`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Review/{id}` | Авторизований |
| POST | `/Review` | AnyAuthenticated |
| PUT | `/Review/{id}` | AnyAuthenticated |
| DELETE | `/Review/{id}` | AnyAuthenticated |
| GET | `/Review/by-reviewed-user/{email}` | AnyAuthenticated |
| GET | `/Review/average-rating/{email}` | AnyAuthenticated |
| GET | `/Review/by-user` | AnyAuthenticated |

---

### **NotificationController** `/Notification`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Notification/is-not-read` | Авторизований |
| GET | `/Notification/paginated` | Авторизований |
| GET | `/Notification/filtered` | Авторизований |
| GET | `/Notification/type-employer-enums` | Авторизований |
| GET | `/Notification/type-freelancer-enums` | Авторизований |
| PATCH | `/Notification/{id}/toggle-read` | Авторизований |
| PATCH | `/Notification/read-all` | Авторизований |

---

### **DisputeController** `/Dispute`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Dispute` | Admin / Moderator |
| GET | `/Dispute/{id}` | Admin / Moderator |
| GET | `/Dispute/by-user` | Авторизований |
| POST | `/Dispute` | Авторизований |
| DELETE | `/Dispute/{id}` | Admin / Moderator |
| GET | `/Dispute/status-moderator-enums` | Admin / Moderator |
| PUT | `/Dispute/{id}/status` | Admin / Moderator |

---

### **DisputeResolutionController** `/DisputeResolution`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/DisputeResolution` | Admin / Moderator |
| GET | `/DisputeResolution/{id}` | Admin / Moderator |
| POST | `/DisputeResolution` | Admin / Moderator |
| DELETE | `/DisputeResolution/{id}` | Admin / Moderator |

---

### **FreelancerPortfolioController** `/FreelancerPortfolio`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/FreelancerPortfolio/{id}` | Freelancer only |
| POST | `/FreelancerPortfolio` | Freelancer only |
| PUT | `/FreelancerPortfolio/{id}` | Freelancer only |
| DELETE | `/FreelancerPortfolio/{id}` | Freelancer only |
| GET | `/FreelancerPortfolio/get-by-freelancer/{freelancerId}` | Публічний |

---

### **CategoryController** `/Category`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Category` | Публічний |
| GET | `/Category/{id}` | Публічний |
| GET | `/Category/paginated` | Публічний |
| POST | `/Category` | Admin only |
| PUT | `/Category/{id}` | Admin only |
| DELETE | `/Category/{id}` | Admin only |

---

### **SkillController** `/Skill`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Skill` | Публічний |
| GET | `/Skill/{id}` | Публічний |
| GET | `/Skill/paginated` | Публічний |
| POST | `/Skill` | Admin only |
| PUT | `/Skill/{id}` | Admin only |
| DELETE | `/Skill/{id}` | Admin only |

---

### **CountryController** `/Country`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Country` | Публічний |
| GET | `/Country/{id}` | Публічний |
| GET | `/Country/paginated` | Публічний |
| POST | `/Country` | Admin only |
| PUT | `/Country/{id}` | Admin only |
| DELETE | `/Country/{id}` | Admin only |

---

### **LanguageController** `/Language`
| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Language` | Публічний |
| GET | `/Language/{id}` | Публічний |
| GET | `/Language/paginated` | Публічний |
| POST | `/Language` | Admin only |
| PUT | `/Language/{id}` | Admin only |
| DELETE | `/Language/{id}` | Admin only |

---

### **WalletController** `/Wallet`
> Контролер під JWT-авторизацією за замовчуванням. Депозит через Stripe дозволений policy `AdminOrEmployer`.

| Метод | Endpoint | Доступ |
|---|---|---|
| GET | `/Wallet/balance` | Авторизований |
| POST | `/Wallet/create-payment-intent` | Admin / Employer |
| POST | `/Wallet/confirm-deposit` | Admin / Employer |

---

## 📡 SignalR Hub

### `NotificationHub` — `/notifications`
Push-only хаб для сповіщень у реальному часі. Сервер надсилає подію `ReceiveNotification` клієнту через `IHubContext<NotificationHub>`.

Ідентифікація користувача — через JWT claim `id` (User Guid), що реалізовано в `NotificationUserIdProvider`.

**Типи сповіщень для роботодавця:** `NewBidReceived`, `NewMessage`, `DisputeOpened`, `ReviewLeft`, `SystemAnnouncement`, `ProjectDeadlineReminder`

**Типи сповіщень для фрілансера:** `NewMessage`, `MilestoneApproved`, `MilestoneRejected`, `ContractCreated`, `PaymentReceived`, `DisputeOpened`, `ReviewLeft`, `SystemAnnouncement`, ...

---

## 🏗️ Архітектура

### Clean Architecture (4 шари)
- **Domain** — чисті моделі та enums, без залежностей
- **DAL** — EF Core DbContext, міграції, репозиторії
- **BLL** — CQRS, сервіси, маппінг, валідація, хаби
- **API** — контролери, middleware, конфігурація

### CQRS з MediatR
- **Commands** — зміна стану (Create, Update, Delete)
- **Queries** — читання даних (GetAll, GetById, GetFiltered, GetPaginated)
- **Generic CRUD** — базові handlers для типових операцій
- **ValidationBehaviour** — pipeline behavior для автоматичної валідації FluentValidation перед кожним запитом

### Repository Pattern
Кожна сутність має власний інтерфейс репозиторію в BLL, реалізований у DAL.

---

## 🛡️ Middleware

### `MiddlewareExceptionsHandling`
Глобальна обробка винятків:
| Виняток | HTTP статус |
|---|---|
| `SecurityTokenException` | `426 Upgrade Required` |
| `ValidationException` | `400 Bad Request` |
| Інші `Exception` | `500 Internal Server Error` |

---

## 🧪 Тестування

Покриття через **Integration Tests** із `WebApplicationFactory`:

| Папка/модуль | Що тестує |
|---|---|
| `tests/Api.Tests.Integration/` | Інтеграційні тести API |
| `tests/Tests.Common/` | Базові класи/фабрика/хелпери |
| `tests/TestsData/` | Фікстури та тестові дані |

---

## 🐳 Docker

### Запуск через Docker Compose (рекомендовано)

**1. Скопіюйте файл зі змінними оточення:**
```bash
cp .env.example .env
```

**2. Запустіть сервіси:**
```bash
docker compose up -d
```

Запустяться два контейнери:
- `freelance-db` — PostgreSQL 16 (в docker `5432`, на хості **`5433`**)
- `freelance-api` — API сервер (порт **`8080`**)

API буде доступне за адресою: **http://localhost:8080/swagger**

### Змінні оточення (`.env`)
| Змінна | Опис |
|---|---|
| `GOOGLE_CLIENT_ID` | Client ID Google OAuth 2.0 |
| `JWT_KEY` | Секретний ключ для підпису JWT |
| `JWT_ISSUER` | Видавець токена |
| `JWT_AUDIENCE` | Аудиторія токена |
| `STRIPE_SECRET_KEY` | Stripe Secret Key |
| `STRIPE_PUBLISHABLE_KEY` | Stripe Publishable Key |

### Docker Image
Образ автоматично публікується в **GitHub Container Registry** як:
```
ghcr.io/andromaan/freelance-api:latest
```

---

## ⚙️ Локальний запуск (без Docker)

**Вимоги:** .NET 8 SDK, PostgreSQL 16

```bash
# Клонування репозиторію
git clone https://github.com/andromaan/freelance-api.git
cd freelance-api

# Відновлення залежностей
dotnet restore FreelanceBack.sln

# Запуск проекту (приклад)
dotnet run --project src/API

# Swagger UI
# http://localhost:{port}/swagger
```

---

## 🔄 CI/CD

GitHub Actions пайплайн (`.github/workflows/ci-cd.yml`):

| Job | Тригер | Дії |
|---|---|---|
| `Build & Test` | push / PR → `main` | `dotnet restore` → `build` → `dotnet test` → публікація TRX |
| `Build & Push Docker Image` | push → `main` | login до GHCR → build image → push `latest` + `sha-*` |

---

## 📝 Ліцензія

Цей проект є приватним і належить [@andromaan](https://github.com/andromaan).
