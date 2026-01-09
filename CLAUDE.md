# BasicBlazor - Blazor .NET 10 Authentication PoC

## 1. Project Overview

This is a proof-of-concept Blazor application built on .NET 10 that demonstrates role-based authentication and authorization with static server-side rendering.

### Key Objectives
- Implement secure, password-based authentication
- Demonstrate role-based access control with dynamic page authorization
- Maximize performance using static SSR (no client-side interactivity)
- Abstract data access for testability and maintainability
- Provide comprehensive unit test coverage

### Technology Stack
- **Framework**: Blazor Server (.NET 10)
- **Rendering**: Static SSR (no InteractiveServer components)
- **Database**: SQLite with Entity Framework Core
- **Testing**: xUnit, Moq, FluentAssertions
- **Authentication**: Cookie-based with BCrypt password hashing

### Design Philosophy
- Static-first: All pages use static SSR; forms use POST handlers
- Security-focused: Proper password hashing, secure cookies, CSRF protection
- Configuration-driven: JSON-based authorization for flexibility
- Test-driven: Unit tests for all business logic

---

## 2. Architecture Overview

### Rendering Strategy
**Static SSR for all pages and components** - No Server interactivity required. All forms (login, sign-out) use standard form POST handlers, eliminating SignalR overhead entirely.

### Project Structure
Multi-project solution:
- **BasicBlazor.Web**: Main Blazor application (UI, pages, components)
- **BasicBlazor.Data**: Data abstraction library (repositories, services, models)
- **BasicBlazor.Tests**: Unit tests for repositories and services

### Authentication
Cookie-based authentication using ASP.NET Core's authentication middleware with form POST handlers for login/logout.

### Authorization
Role-based access control driven by a JSON configuration file that maps pages to allowed roles. A singleton service caches this configuration for performance.

### Data Access
Repository pattern with Entity Framework Core abstracts all database operations, enabling easy testing and future database migrations.

---

## 3. Project Structure

```
BasicBlazor/
├── BasicBlazor.Web/              # Main Blazor application
│   ├── Components/
│   │   ├── Layout/               # MainLayout, NavMenu
│   │   ├── Pages/                # Login, AccessDenied, Page1-4
│   │   └── Shared/               # Reusable components
│   ├── wwwroot/                  # Public static files only
│   ├── Program.cs                # App configuration, DI, middleware
│   ├── appsettings.json          # Configuration settings
│   └── BasicBlazor.Web.csproj
│
├── BasicBlazor.Data/             # Data abstraction library
│   ├── Models/                   # User, Role entities
│   ├── Data/                     # AppDbContext
│   ├── Repositories/             # IUserRepository, UserRepository
│   ├── Services/                 # AuthService, PageAccessService
│   ├── Configuration/            # Configuration files
│   │   └── page-access.json      # Page-role mapping config (private)
│   └── BasicBlazor.Data.csproj
│
└── BasicBlazor.Tests/            # Unit tests
    ├── Services/                 # Service tests
    ├── Repositories/             # Repository tests
    └── BasicBlazor.Tests.csproj
```

---

## 4. Database Schema

### Entities

**Roles Table**
- `Id`: int (Primary Key, auto-increment)
- `RoleName`: string (unique, required, max length 50)

**Users Table**
- `Id`: int (Primary Key, auto-increment)
- `Username`: string (unique, required, max length 100)
- `PasswordHash`: string (required, max length 500)
- `RoleId`: int (Foreign Key to Roles.Id)

**Relationship**: Many Users to One Role

### Seed Data

**Roles** (3 roles):
1. Admin
2. Manager
3. User

**Users** (3 users - 1 per role):

| Username   | Password     | Role    |
|------------|--------------|---------|
| admin      | Admin123!    | Admin   |
| manager    | Manager123!  | Manager |
| user       | User123!     | User    |

**Password Hashing**: Use BCrypt.Net-Next or ASP.NET Core Identity's `PasswordHasher<T>` to hash passwords before storing in the database. Never store plain text passwords.

### Database Initialization
- Use EF Core migrations for schema creation
- Seed data in `AppDbContext.OnModelCreating()` or via a separate seeder class
- Apply migrations automatically on startup (acceptable for PoC; use explicit migrations in production)

---

## 5. Configuration Files

### page-access.json

**Location**: `BasicBlazor.Data/Configuration/page-access.json`

This file defines which roles can access which pages. It is NOT in `wwwroot` (not publicly accessible).

```json
{
  "pageAccess": [
    {
      "pagePath": "/page1",
      "allowedRoles": ["User"],
      "displayName": "Page 1",
      "order": 1
    },
    {
      "pagePath": "/page2",
      "allowedRoles": ["Manager"],
      "displayName": "Page 2",
      "order": 2
    },
    {
      "pagePath": "/page3",
      "allowedRoles": ["User", "Manager"],
      "displayName": "Page 3",
      "order": 3
    },
    {
      "pagePath": "/page4",
      "allowedRoles": ["User", "Manager", "Admin"],
      "displayName": "Page 4",
      "order": 4
    }
  ]
}
```

**Access Rules Summary**:
- **Page1**: User only
- **Page2**: Manager only
- **Page3**: User + Manager
- **Page4**: All roles (User, Manager, Admin)

**File Deployment**:
- Set `page-access.json` to "Copy to Output Directory" = "Copy if newer" in `.csproj`
- Alternative: Embed as resource and read via assembly stream
- File must NOT be in `wwwroot` (security risk)

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=basicblazor.db"
  },
  "PageAccessConfigPath": "Configuration/page-access.json"
}
```

**Key Settings**:
- **ConnectionStrings:DefaultConnection**: SQLite database file path
- **PageAccessConfigPath**: (Optional) Path to page-access.json if configurable location needed

---

## 6. Authentication & Authorization

### Authentication Flow

1. **Anonymous Access**: All unauthenticated users are automatically redirected to `/login`
2. **Login Page**: Uses static SSR with a form POST handler
3. **Form Submission**: User submits credentials via standard HTML form (`method="post"`)
4. **Validation**: Page's POST handler invokes `AuthService.ValidateCredentials(username, password)`
5. **Success Path**:
   - Create `ClaimsPrincipal` with claims: username, role
   - Call `HttpContext.SignInAsync()` with cookie authentication scheme
   - Use `PageAccessService.GetFirstAllowedPage(role)` to determine redirect target
   - Redirect user to their first allowed page
6. **Failure Path**: Display error message on login page

### Authorization Strategy

**PageAccessService** (Singleton):
- Reads `page-access.json` on initialization
- Caches parsed configuration in memory for performance
- Provides methods:
  - `IsPageAccessible(pagePath, roleName)`: Returns `bool` indicating if role can access page
  - `GetAllowedPagesForRole(roleName)`: Returns list of accessible pages with metadata (for nav menu)
  - `GetFirstAllowedPage(roleName)`: Returns default landing page after login

**Page-Level Authorization**:
- Each protected page checks authorization in `OnInitialized()` or `OnInitializedAsync()`
- If user lacks access, redirect to `/access-denied`
- If user is anonymous, authentication middleware redirects to `/login`

**Dynamic Navigation**:
- NavMenu component calls `PageAccessService.GetAllowedPagesForRole(currentUserRole)`
- Renders only menu items the user can access
- Updates automatically based on authenticated user's role

**Permission Policy Usage**:

Permission-based policies use the `PERM:` prefix to distinguish them from other authorization policies:

- **Database Storage**: Permission names stored without prefix (e.g., `Page3:See_Button`)
- **Policy Usage in UI**: Use `PERM:` prefix (e.g., `Policy="PERM:Page3:See_Button"`)
- **Constant Reference**: Use `PermissionPolicyProvider.PREFIX` constant in code

Example:
```razor
<AuthorizeView Policy="PERM:Page3:See_Button">
    <Authorized>
        <!-- Content for users with permission -->
    </Authorized>
</AuthorizeView>
```

The `PERM:` prefix is automatically stripped before checking against database permission names, making the system more explicit and preventing conflicts with other custom policies.

### Sign-Out Flow

1. User clicks sign-out (form with POST handler or button in layout)
2. Handler method calls `HttpContext.SignOutAsync()`
3. Authentication cookie is cleared
4. User is redirected to `/login`

---

## 7. Component & Page Patterns

### Static SSR Pages (All Pages)

All pages, including Login, use **static SSR** (no `@rendermode` directive).

**Pattern**:
```razor
@page "/page1"
@inject AuthenticationStateProvider AuthStateProvider
@inject PageAccessService PageAccessService
@inject NavigationManager Navigation

@* Page content *@

@code {
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            // Middleware should handle this, but defensively check
            Navigation.NavigateTo("/login", forceLoad: true);
            return;
        }

        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (!PageAccessService.IsPageAccessible("/page1", role))
        {
            Navigation.NavigateTo("/access-denied", forceLoad: true);
        }
    }
}
```

### Form Handling (Login, Sign-Out)

**Login Page** (`/login`):
- Static SSR page with form POST handler
- Uses `EditForm` or native `<form method="post">`
- Form fields bound with `[SupplyParameterFromForm]` attribute
- Handler method processes credentials and signs user in

**Example Pattern**:
```razor
@page "/login"
@inject AuthService AuthService
@inject NavigationManager Navigation

<form method="post" @onsubmit="HandleLogin">
    <input type="text" @bind="Username" required />
    <input type="password" @bind="Password" required />
    <button type="submit">Login</button>
</form>

@code {
    [SupplyParameterFromForm]
    public string Username { get; set; } = "";

    [SupplyParameterFromForm]
    public string Password { get; set; } = "";

    private async Task HandleLogin()
    {
        var user = await AuthService.ValidateCredentials(Username, Password);
        if (user != null)
        {
            // Create claims and sign in
            // Redirect to first allowed page
        }
        else
        {
            // Show error
        }
    }
}
```

**Sign-Out**:
- Form POST with handler in MainLayout or dedicated component
- No client interactivity required

### Layout Components

**MainLayout**:
- Wraps all pages
- Cascades `AuthenticationState` to child components
- Includes NavMenu component

**NavMenu**:
- Injects `AuthenticationStateProvider` and `PageAccessService`
- Gets current user's role from claims
- Calls `PageAccessService.GetAllowedPagesForRole(role)` to get accessible pages
- Renders `<NavLink>` elements only for allowed pages
- Updates automatically when authentication state changes

---

## 8. Services & Repositories

### Data Access Layer (BasicBlazor.Data)

**AppDbContext**:
- EF Core `DbContext` with `DbSet<User>` and `DbSet<Role>`
- Configures entity relationships and constraints
- Seeds initial data in `OnModelCreating()`

**IUserRepository** (Interface):
```csharp
public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    // Note: GetAllUsersAsync removed - not needed for this PoC (no user management UI)
}
```

**UserRepository** (Implementation):
- Implements `IUserRepository`
- Uses EF Core for database queries
- Includes `Role` navigation property in queries

**Registration**: Scoped lifetime in DI container

### Business Logic Layer (BasicBlazor.Data)

**AuthService**:
- `ValidateCredentials(username, password)`:
  - Retrieves user via `IUserRepository.GetUserByUsernameAsync(username)`
  - Verifies password hash using BCrypt or `PasswordHasher<T>`
  - Returns `User` object if valid, `null` if invalid
- Registration: Scoped lifetime

**PageAccessService**:
- **Purpose**: Abstracts access to `page-access.json` configuration
- **Initialization**: Loads and parses JSON file in constructor or initialization method
- **Caching**: Stores parsed configuration in memory for fast lookups
- **Methods**:
  - `IsPageAccessible(pagePath, roleName)`: Returns `bool`
  - `GetAllowedPagesForRole(roleName)`: Returns `List<PageAccessInfo>` (path, display name, order)
  - `GetFirstAllowedPage(roleName)`: Returns first page path (sorted by order)
- **Registration**: **Singleton** lifetime for caching efficiency

**Configuration Model Classes**:
```csharp
public class PageAccessConfiguration
{
    public List<PageAccessRule> PageAccess { get; set; } = new();
}

public class PageAccessRule
{
    public string PagePath { get; set; } = "";
    public List<string> AllowedRoles { get; set; } = new();
    public string DisplayName { get; set; } = "";
    public int Order { get; set; }
}
```

---

## 9. Testing Strategy

### Testing Framework
- **xUnit**: Test framework
- **FluentAssertions**: Readable assertions
- **Moq**: Mocking dependencies
- **EF Core InMemory**: In-memory database for repository tests

### Test Categories

#### 1. Repository Tests

**File**: `BasicBlazor.Tests/Repositories/UserRepositoryTests.cs`

**Tests**:
- `GetUserByUsernameAsync_ExistingUser_ReturnsUser()`
- `GetUserByUsernameAsync_NonExistentUser_ReturnsNull()`
- `GetUserByUsernameAsync_IncludesRoleNavigation()`

**Note**: `GetAllUsersAsync_ReturnsAllUsers()` test removed as the method is not needed for this PoC

**Setup**: Use EF Core's InMemory provider or in-memory SQLite

#### 2. AuthService Tests

**File**: `BasicBlazor.Tests/Services/AuthServiceTests.cs`

**Tests**:
- `ValidateCredentials_ValidUsernameAndPassword_ReturnsUser()`
- `ValidateCredentials_ValidUsernameWrongPassword_ReturnsNull()`
- `ValidateCredentials_NonExistentUsername_ReturnsNull()`
- `ValidateCredentials_EmptyUsername_ReturnsNull()`

**Setup**: Mock `IUserRepository` using Moq

#### 3. PageAccessService Tests

**File**: `BasicBlazor.Tests/Services/PageAccessServiceTests.cs`

**Tests**:
- `IsPageAccessible_UserRolePage1_ReturnsTrue()`
- `IsPageAccessible_UserRolePage2_ReturnsFalse()`
- `IsPageAccessible_ManagerRolePage3_ReturnsTrue()`
- `GetAllowedPagesForRole_UserRole_ReturnsCorrectPages()` (Page1, Page3, Page4)
- `GetAllowedPagesForRole_ManagerRole_ReturnsCorrectPages()` (Page2, Page3, Page4)
- `GetAllowedPagesForRole_AdminRole_ReturnsCorrectPages()` (Page4 only)
- `GetFirstAllowedPage_UserRole_ReturnsPage1()`
- `GetFirstAllowedPage_ManagerRole_ReturnsPage2()`

**Setup**:
- Provide test JSON configuration (embedded string or test file)
- Or mock file system access

#### 4. Component Tests (Optional - using bUnit)

**Files**: `BasicBlazor.Tests/Components/NavMenuTests.cs`, etc.

**Tests**:
- `NavMenu_UserRole_ShowsOnlyAllowedLinks()`
- `NavMenu_ManagerRole_ShowsOnlyAllowedLinks()`
- `LoginPage_InvalidCredentials_ShowsError()`
- `ProtectedPage_UnauthorizedUser_RedirectsToAccessDenied()`

**Setup**: Use bUnit library for Blazor component testing

---

## 10. NuGet Packages

### BasicBlazor.Web

```xml
<PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="10.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.*" />
```

### BasicBlazor.Data

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.*" />
```

### BasicBlazor.Tests

```xml
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="bUnit" Version="1.*" /> <!-- Optional -->
```

---

## 11. Program.cs Configuration

Key configurations for `BasicBlazor.Web/Program.cs`:

### Services Registration

```csharp
// Add Blazor components
builder.Services.AddRazorComponents();

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories (Scoped)
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<PageAccessService>(); // Singleton for caching

// Add antiforgery for form POST security
builder.Services.AddAntiforgery();
```

### Middleware Pipeline

```csharp
// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Use antiforgery middleware
app.UseAntiforgery();

// Map Blazor components (static SSR by default)
app.MapRazorComponents<App>();
```

### Database Initialization

```csharp
// Apply migrations and seed database on startup (PoC only)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Apply pending migrations
    // Seed data is handled in OnModelCreating
}
```

### Enhanced Navigation

Enable enhanced navigation for smooth page transitions:
```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); // Enable if needed for future interactivity
```

Note: For pure static SSR, `AddInteractiveServerComponents()` is optional but enables future extensibility.

---

## 12. Security Considerations

### Password Security
- **Never store plain text passwords**
- Use BCrypt.Net-Next or ASP.NET Core Identity's `PasswordHasher<T>`
- BCrypt automatically handles salt generation
- Minimum password complexity recommended (e.g., 8+ chars, mix of letters/numbers/symbols)

### Cookie Security
- `HttpOnly = true`: Prevents JavaScript access to cookies (XSS protection)
- `SecurePolicy = CookieSecurePolicy.Always`: Requires HTTPS
- `SlidingExpiration = true`: Extends expiration on activity
- `SameSite = SameSiteMode.Strict`: CSRF protection (consider for production)

### CSRF Protection
- Antiforgery middleware enabled for form POST handlers
- Blazor automatically includes antiforgery tokens in forms

### Input Validation
- Validate username and password on login form (required, max length)
- Sanitize inputs to prevent injection attacks
- Use data annotations and model validation

### Redirect Loop Prevention
- Ensure `/login` page does NOT require authentication
- Ensure `/access-denied` page does NOT require specific roles
- Use `forceLoad: true` in `NavigateTo()` for authentication redirects

### SQL Injection Prevention
- EF Core uses parameterized queries automatically
- Avoid raw SQL queries; use LINQ

### Page Access Configuration Security
- **Critical**: `page-access.json` must NOT be in `wwwroot` (publicly accessible)
- Store in `BasicBlazor.Data/Configuration/` or similar private location
- File copied to output directory or embedded as resource

---

## 13. Development Workflow

Follow this phased approach for implementation:

### Phase 1 - Foundation

1. **Create Solution Structure**
   - Create blank solution `BasicBlazor.sln`
   - Add `BasicBlazor.Web` project (Blazor Server App template)
   - Add `BasicBlazor.Data` project (Class Library)
   - Add `BasicBlazor.Tests` project (xUnit Test Project)
   - Add project references: Web → Data, Tests → Data

2. **Set Up Data Library**
   - Create `Models/User.cs` and `Models/Role.cs` entity classes
   - Create `Data/AppDbContext.cs` with DbSets and relationships
   - Configure entity constraints (unique indexes, required fields, max lengths)
   - Add seed data in `OnModelCreating()`

3. **Create Initial Migration**
   - Install EF Core tools: `dotnet tool install --global dotnet-ef`
   - Run: `dotnet ef migrations add InitialCreate --project BasicBlazor.Data --startup-project BasicBlazor.Web`
   - Verify migration file contains schema and seed data

4. **Implement Repositories**
   - Create `Repositories/IUserRepository.cs` interface
   - Create `Repositories/UserRepository.cs` implementation
   - Write repository unit tests

### Phase 2 - Core Services

5. **Implement AuthService**
   - Create `Services/AuthService.cs`
   - Implement `ValidateCredentials()` with password hashing
   - Write AuthService unit tests

6. **Create page-access.json**
   - Create `Configuration/page-access.json` in Data project
   - Add JSON content with page-role mappings
   - Set file to "Copy to Output Directory"

7. **Implement PageAccessService**
   - Create `Services/PageAccessService.cs`
   - Load and parse JSON in constructor
   - Implement caching and query methods
   - Write PageAccessService unit tests

### Phase 3 - Authentication

8. **Configure Authentication in Program.cs**
   - Add cookie authentication with redirect paths
   - Register `AuthenticationStateProvider`
   - Add authentication/authorization middleware
   - Configure database initialization

9. **Create Login Page**
   - Create `Components/Pages/Login.razor`
   - Implement form with username/password fields
   - Add form POST handler
   - Handle sign-in logic with `SignInAsync()`
   - Redirect to first allowed page on success

10. **Test Authentication**
    - Run application
    - Verify anonymous users redirect to login
    - Test login with valid/invalid credentials
    - Verify redirect to correct page after login

### Phase 4 - Pages & Navigation

11. **Create Protected Pages**
    - Create `Components/Pages/Page1.razor` (User only)
    - Create `Components/Pages/Page2.razor` (Manager only)
    - Create `Components/Pages/Page3.razor` (User + Manager)
    - Create `Components/Pages/Page4.razor` (All roles)
    - Add authorization checks in `OnInitialized()`

12. **Add Authorization Checks**
    - Inject `AuthenticationStateProvider` and `PageAccessService`
    - Check role and page access in each page
    - Redirect to `/access-denied` if unauthorized

13. **Create AccessDenied Page**
    - Create `Components/Pages/AccessDenied.razor`
    - Simple message: "You do not have permission to access this page"
    - Link back to available pages or logout

14. **Build Dynamic NavMenu**
    - Edit `Components/Layout/NavMenu.razor`
    - Get current user's role from `AuthenticationState`
    - Call `PageAccessService.GetAllowedPagesForRole(role)`
    - Render `<NavLink>` only for accessible pages
    - Test with different user roles

### Phase 5 - Authorization

15. **Implement Page Access Validation**
    - Verify all pages correctly check authorization
    - Test direct URL access to unauthorized pages
    - Ensure redirects work correctly

16. **Add Post-Login Redirect Logic**
    - Update Login page handler
    - Use `PageAccessService.GetFirstAllowedPage(role)`
    - Redirect to first allowed page

17. **Add Sign-Out Functionality**
    - Add sign-out form/button in MainLayout or NavMenu
    - Implement POST handler calling `SignOutAsync()`
    - Redirect to `/login` after sign-out
    - Test sign-out flow

### Phase 6 - Testing

18. **Write Repository Tests**
    - Test `GetUserByUsernameAsync()` with various scenarios
    - Test `GetAllUsersAsync()`
    - Verify navigation properties loaded

19. **Write Service Tests**
    - Test `AuthService.ValidateCredentials()` with valid/invalid credentials
    - Test `PageAccessService` methods for all roles and pages
    - Achieve high code coverage

20. **Write Component Tests (Optional)**
    - Use bUnit to test NavMenu rendering
    - Test Login page form submission
    - Test page redirects

21. **End-to-End Validation**
    - Run all unit tests (`dotnet test`)
    - Manually test all scenarios in testing checklist (Section 14)
    - Verify all acceptance criteria met

---

## 14. Testing Checklist

Use this checklist to validate the implementation:

### Authentication Tests
- [ ] All 3 users can log in with correct credentials (admin, manager, user)
- [ ] Invalid credentials are rejected with appropriate error message
- [ ] Anonymous users attempting to access protected pages are redirected to `/login`
- [ ] Successful login redirects to the first allowed page for the user's role

### Authorization Tests
- [ ] **User role** sees Page1, Page3, Page4 in navigation menu
- [ ] **Manager role** sees Page2, Page3, Page4 in navigation menu
- [ ] **Admin role** sees only Page4 in navigation menu
- [ ] Direct URL access to unauthorized pages redirects to `/access-denied`
  - User accessing `/page2` → `/access-denied`
  - Manager accessing `/page1` → `/access-denied`
  - Admin accessing `/page1`, `/page2`, `/page3` → `/access-denied`

### Navigation Tests
- [ ] Navigation menu dynamically updates based on authenticated user's role
- [ ] All navigation links work correctly
- [ ] Access denied page displays appropriate message

### Sign-Out Tests
- [ ] Sign-out button/form clears authentication
- [ ] After sign-out, user is redirected to `/login`
- [ ] After sign-out, attempting to access protected pages redirects to `/login`

### Unit Tests
- [ ] All repository tests pass
- [ ] All AuthService tests pass
- [ ] All PageAccessService tests pass
- [ ] All component tests pass (if implemented)
- [ ] Overall test coverage > 80% for business logic

### Security Tests
- [ ] Passwords are stored as hashes (not plain text) in database
- [ ] page-access.json is NOT accessible via browser (not in wwwroot)
- [ ] Login form includes antiforgery token
- [ ] Authentication cookies have `HttpOnly` flag

---

## 15. Key Implementation Notes

### Performance
- **Static SSR throughout**: No SignalR overhead at all - pure server-side rendering maximizes performance
- **Singleton PageAccessService**: Configuration loaded once and cached in memory
- **Minimal database queries**: Repository pattern enables efficient querying with navigation properties

### Form Handling
- **Form POST handlers**: Enable authentication/logout without client interactivity
- **Enhanced Navigation**: Provides smooth page transitions while maintaining static SSR benefits
- **Antiforgery protection**: Automatic CSRF protection via middleware

### Architecture Benefits
- **Repository pattern**: Enables easy unit testing by mocking data access
- **JSON-based authorization**: Allows configuration changes without recompilation or code changes
- **Separation of concerns**: Data library abstracts all business logic from UI
- **Cookie authentication**: Production-ready, stateless authentication mechanism

### Testing
- **Unit testing ensures reliability**: All business logic (auth, authorization, data access) has test coverage
- **Mocking enables isolation**: Services tested independently of database and file system
- **In-memory database**: Fast, reliable repository tests without external dependencies

### Extensibility
While the current implementation uses static SSR throughout, the architecture supports adding `@rendermode InteractiveServer` to specific components in the future if dynamic client interactivity is needed (e.g., real-time updates, complex forms with dynamic validation).

**Future Enhancement Examples**:
- Add interactive data grid with sorting/filtering
- Real-time notifications using SignalR
- Drag-and-drop file uploads
- Rich text editors

The static-first approach ensures optimal performance for the majority of pages while allowing targeted interactivity where it adds value.

---

## 16. Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2025 / VS Code / Rider
- SQLite (included with EF Core)

### Setup Steps

1. **Clone/Create Repository**
   ```bash
   mkdir BasicBlazor
   cd BasicBlazor
   git init
   ```

2. **Create Solution and Projects**
   ```bash
   dotnet new sln -n BasicBlazor
   dotnet new blazorserver -n BasicBlazor.Web
   dotnet new classlib -n BasicBlazor.Data
   dotnet new xunit -n BasicBlazor.Tests

   dotnet sln add BasicBlazor.Web/BasicBlazor.Web.csproj
   dotnet sln add BasicBlazor.Data/BasicBlazor.Data.csproj
   dotnet sln add BasicBlazor.Tests/BasicBlazor.Tests.csproj

   dotnet add BasicBlazor.Web reference BasicBlazor.Data
   dotnet add BasicBlazor.Tests reference BasicBlazor.Data
   ```

3. **Install NuGet Packages** (see Section 10)

4. **Follow Development Workflow** (see Section 13)

5. **Run Application**
   ```bash
   cd BasicBlazor.Web
   dotnet run
   ```

6. **Run Tests**
   ```bash
   cd BasicBlazor.Tests
   dotnet test
   ```

---

## 17. Critical Implementation Files

These files form the architectural backbone of the application:

1. **[BasicBlazor.Data/Data/AppDbContext.cs](BasicBlazor.Data/Data/AppDbContext.cs)**
   - Core data model definition with entities, relationships, and seed data
   - Establishes entire database schema

2. **[BasicBlazor.Data/Services/PageAccessService.cs](BasicBlazor.Data/Services/PageAccessService.cs)**
   - Authorization engine that reads page-access.json
   - Determines role-based page access
   - Critical for navigation and security

3. **[BasicBlazor.Data/Configuration/page-access.json](BasicBlazor.Data/Configuration/page-access.json)**
   - Configuration file defining all page-to-role mappings
   - Drives authorization decisions and dynamic navigation
   - Must be kept private (not in wwwroot)

4. **[BasicBlazor.Web/Program.cs](BasicBlazor.Web/Program.cs)**
   - Application bootstrap
   - Configures authentication, authorization, DI, middleware pipeline
   - Registers all services

5. **[BasicBlazor.Web/Components/Pages/Login.razor](BasicBlazor.Web/Components/Pages/Login.razor)**
   - Authentication entry point
   - Implements authentication flow with form submission
   - Handles post-login redirect logic

---

## 18. Troubleshooting

### Common Issues

**Issue**: Users not redirected to login
- **Solution**: Ensure authentication middleware is configured before authorization middleware in `Program.cs`
- **Solution**: Verify `LoginPath` is set in cookie authentication options

**Issue**: Navigation menu shows all pages regardless of role
- **Solution**: Check `PageAccessService` is correctly filtering by role
- **Solution**: Verify `page-access.json` is being loaded (check file path and "Copy to Output Directory")

**Issue**: Access denied page not showing
- **Solution**: Ensure `/access-denied` route is defined
- **Solution**: Verify `AccessDeniedPath` in cookie authentication options

**Issue**: Password validation always fails
- **Solution**: Verify password hashing is using the same algorithm for storage and validation
- **Solution**: Check seed data is creating hashed passwords, not plain text

**Issue**: Database not found
- **Solution**: Verify connection string in `appsettings.json`
- **Solution**: Ensure migrations have been applied (`dotnet ef database update`)

**Issue**: page-access.json not found at runtime
- **Solution**: Set "Copy to Output Directory" = "Copy if newer" in `.csproj`
- **Solution**: Verify path in `PageAccessService` constructor matches file location

---

## 19. Additional Resources

### Official Documentation
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [xUnit Documentation](https://xunit.net/)

### Security Best Practices
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)

### Testing
- [bUnit Documentation](https://bunit.dev/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)

---

**Document Version**: 1.0
**Last Updated**: 2025-12-30
**Target Framework**: .NET 10
