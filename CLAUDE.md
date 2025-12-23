# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Solution Architecture

MyAccounts is a .NET 8 financial account management application with multiple frontends sharing a common backend.

**Project Structure:**
- **MyAccounts** - ASP.NET Core 8 Web API backend with OData support
- **MyAccounts.Shared** - Shared domain models/DTOs used across all projects
- **MyAccounts.Shared.Blazor** - Razor Class Library with reusable Blazor components and services
- **MyAccounts.Blazor** - Blazor WebAssembly client
- **MyAccounts.Maui.Blazor** - Cross-platform MAUI desktop/mobile application

All UI projects reference MyAccounts.Shared.Blazor for components and MyAccounts.Shared for models.

## Build & Run Commands

### Backend API
```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run the API server (includes Blazor WASM)
dotnet run --project MyAccounts/MyAccounts.csproj

# Watch mode for development
dotnet watch --project MyAccounts/MyAccounts.csproj
```

The API serves:
- Swagger UI at `/swagger`
- OData endpoints at `/odata/{entity}`
- REST endpoints at `/api/{controller}`
- Blazor WASM client at root `/`

### MAUI Desktop App
```bash
# Windows
dotnet build MyAccounts.Maui.Blazor/MyAccounts.Maui.Blazor.csproj -f net8.0-windows10.0.19041.0

# macOS
dotnet build MyAccounts.Maui.Blazor/MyAccounts.Maui.Blazor.csproj -f net8.0-maccatalyst
```

### Database Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName --project MyAccounts

# Apply migrations
dotnet ef database update --project MyAccounts

# Generate SQL script
dotnet ef migrations script --project MyAccounts
```

## Technology Stack

**Backend:**
- ASP.NET Core 8.0.16 with Kestrel
- Entity Framework Core 8.0.16
- PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL 8.0.4)
- ASP.NET Core Identity with Bearer token authentication (7-day expiration)
- OData v4 (Microsoft.AspNetCore.OData 9.4.1)
- Rate limiting (fixed window, sliding window, token bucket, concurrency)

**Frontend:**
- Blazor WebAssembly 8.0.16 / .NET MAUI 8.0.92
- MudBlazor 8.15.0 (Material Design components)
- Blazor ApexCharts 3.5.0
- BlazorDatasheet 0.9.3
- ClosedXML 0.105.0 (Excel export)
- CsvHelper 33.1.0

## Database Schema

**Core Financial Entities:**
- `AccountType` - Master data for account types
- `Account` - Financial accounts (checking, savings, credit cards, etc.)
  - Has unique constraint on Name
  - Many-to-many relationship with Category
  - Has one AccountType
- `Category` - Transaction categories
  - Has unique constraint on Name
  - Many-to-many relationship with Account
- `Transaction` - Individual transactions
  - Belongs to one Account and one Category
  - Uses DateOnly for dates
  - Decimal(19,4) for Amount and Balance
- `TransactionSplit` - Split transactions across categories
  - Belongs to one Transaction and one Category

**Budget Entities:**
- `BudgetAccount`, `BudgetMonth`, `BudgetExpense`, `BudgetIncome`

**Identity:**
- ASP.NET Core Identity tables with extended ApplicationUser (FirstName, LastName, Title, CompanyName, Photo)

**Database Configuration:**
- PostgreSQL with Npgsql identity column strategy for primary keys
- BigInt auto-increment PKs
- Decimal precision (19,4) for all monetary values
- Connection string in appsettings.json: `ConnectionStrings:ApplicationDbContext`
- User secrets ID: `a6a59fd0-bbee-40c1-a13b-bea1a9b75eba`

## Authentication & Authorization

**Authentication Flow:**
1. Client POSTs to `/identity/login?useCookies=false` with email/password
2. Receives `AccessTokenResponse` with Bearer token (7-day expiration)
3. Token stored in browser local storage via BrowserStorageService
4. All API requests include `Authorization: Bearer {token}` header
5. User info retrieved from `/api/user/@me`

**Roles:**
- Administrator (adminUser@example.com / testUser123!)
- Normal User (normalUser@example.com / testUser123!)

**Controllers:**
- All decorated with `[Authorize]` attribute
- Admin-only routes use `[Authorize(Roles = "Administrator")]`

## OData Implementation

**Endpoint Pattern:**
```
/odata/Account?$filter=Name eq 'Checking'&$orderby=Date desc&$top=10&$skip=0&$expand=AccountType,Category&$count=true
```

**Configured Entity Sets:**
- Account, Transaction, Category, TransactionSplit
- BudgetAccount, BudgetMonth, BudgetExpense, BudgetIncome
- User (ApplicationUserDto)

**Query Building:**
The `ODataHelpers.cs` utility in MyAccounts.Shared.Blazor constructs OData queries for MudDataGrid:
- Converts MudDataGrid state (filters, sort, pagination) to OData query strings
- Uses DataMember attributes to map C# properties to API names
- Handles $filter, $orderby, $top, $skip, $expand, $count

**Example:**
```csharp
// In List components
private async Task<GridData<Account>> ServerReload(GridState<Account> state)
{
    var url = ODataHelpers.BuildODataQueryUrl("Account", state, "AccountType,Category");
    var result = await AppService.GetODataAsync<Account>(...);
    return new GridData<Account>
    {
        Items = result?.Value ?? [],
        TotalItems = (int)(result?.Count ?? 0)
    };
}
```

## Blazor Component Architecture

**Layout Hierarchy:**
```
Main.razor (root component)
└── Router with AuthorizeRouteView
    └── MainLayout (MudLayout)
        ├── NavMenu (sidebar with MudNavMenu)
        └── MudMainContent (@Body)
```

**Page Routing:**
- Pages use `@page "/route"` directive
- Key routes: `/`, `/account`, `/transaction/{AccountName}`, `/category`, `/budget`
- Authorization: `@attribute [Authorize]` or `@attribute [Authorize(Roles = "Administrator")]`

**Component Patterns:**

1. **List Pages (MudDataGrid with server-side data):**
   - `ServerData="@ServerReload"` binding for pagination/filtering
   - OData query building via ODataHelpers
   - Export to CSV/Excel functionality
   - Add/Edit/Delete via dialogs

2. **Form Pages:**
   - `EditForm` with `DataAnnotationsValidator`
   - MudBlazor components (MudTextField, MudSelect, MudDatePicker)
   - Server-side validation with CustomValidation component
   - Navigation after save via NavigationService

3. **Shared Components:**
   - `AccountSelectorDialog` - Entity selection
   - `ExcelExporter` - Excel workbook generation
   - `LoadingSpinner` - Loading indicators
   - `ThemesMenu` - Theme management

**Key Services:**

- **AppService** - Primary API client, OData query execution, HTTP requests
- **IdentityAuthenticationStateProvider** - Authentication state, token management
- **NavigationService** - Component communication (e.g., notify list to reload after edit)
- **BrowserStorageService** - Local storage abstraction for tokens/claims
- **ThemeService** - MudBlazor theme management
- **TransactionService** - Business logic for transaction balance calculations

## Import/Export Features

**CSV Import:**
- Quicken export format: `CsvQuickenImport.razor` uses `/api/csv/import`
- Bank CSV: `BankImport.razor` uses `/api/banksv/import`
- Credit Card CSV: Similar pattern

**Export:**
- Excel via ClosedXML in `ExcelExporter.razor`
- CSV via CsvHelper in list components

**Transaction Balance Calculation:**
- `TransactionService.CalculateBalances()` recalculates running balances after import
- Called after batch operations to ensure data integrity

## Important Conventions

**Model Naming:**
- Shared models in MyAccounts.Shared use singular names (Account, Transaction, Category)
- OData entity sets use same names (Account, not Accounts)
- REST controller routes use `[controller]` token (e.g., AccountController → /api/account)

**JSON Serialization:**
- `ReferenceHandler.IgnoreCycles` prevents circular reference issues
- Custom `DateOnlyConverter` for DateOnly type (format: "yyyy-MM-dd")
- Decimal precision maintained through EF Core double mapping

**Rate Limiting:**
- Fixed window: 1000 requests/minute
- Sliding window: 4 requests/10 seconds
- Controllers use `[EnableRateLimiting("Fixed")]`

**File Organization:**
- Backend controllers in MyAccounts/Controllers/
- Shared UI in MyAccounts.Shared.Blazor/Pages/
- Services in each project's Services/ folder
- Models in MyAccounts.Shared/Models/

## Configuration

**appsettings.json:**
- ConnectionStrings:ApplicationDbContext (PostgreSQL)
- JWT token expiration (7 days)
- Rate limiting policies
- CORS policies

**Development:**
- Use user secrets for sensitive config (ID: a6a59fd0-bbee-40c1-a13b-bea1a9b75eba)
- appsettings.Development.json for dev-specific overrides

## Known Patterns

**Controller Injection:**
```csharp
// Primary constructor pattern (preferred)
public AccountController(ApplicationDbContext ctx) : ControllerBase

// Traditional pattern (also used)
private readonly ApplicationDbContext _ctx;
public AccountController(ApplicationDbContext ctx)
{
    _ctx = ctx;
}
```

**OData Query Execution:**
```csharp
[HttpGet]
[EnableQuery]
public ActionResult<IQueryable<Account>> GetAccounts()
{
    return Ok(_ctx.Account
        .Include(a => a.AccountType)
        .Include(a => a.Category));
}
```

**Blazor Service Registration:**
- Extensions.cs in MyAccounts.Shared.Blazor contains `AddAppServices()` extension
- Registers all shared services (AppService, NavigationService, etc.)
- Called from Program.cs in both WASM and MAUI projects
