# ASP.NET Core Identity (Beginner Friendly Guide)

A simple, built-in system in ASP.NET Core that helps you:
- Register users
- Log them in / out
- Store hashed passwords
- Assign Roles (Admin, User, etc.)
- Use Claims (extra info about a user)
- Handle password reset, email confirm, MFA, external logins (Google, etc.)

It uses Entity Framework Core to create and manage tables in your database automatically.

---

## 1. When To Use It
Use ASP.NET Core Identity when you need:
- Secure, production-ready login with hashed passwords
- Role / claim based authorization
- Token + cookie support
- Extensible user model

Do NOT reinvent login with manual tables or plain Basic Authentication for real apps. Identity already solves most of it safely.

---

## 2. How It Fits (High Level Flow)
```
+---------+      HTTP        +----------------+     Uses API       +-------------------+
| Browser |  ---> Request -->| Controller     |--> UserManager --> | Identity DB (EF)  |
| / Client|                  | (e.g. AuthController)                 | AspNetUsers, etc. |
+---------+<-- Cookie/Auth --+----------------+<-- SignInManager --+-------------------+
      |                                 |
      |  Sends credentials (register/login)
      |  Receives cookie or JWT (your choice)
```
Authorization Pipeline (simplified):
```
[User Request] -> Authentication (cookie/JWT validated) -> User Principal (Claims) ->
Policy / Role / Claim checks -> Allowed or 403 Forbidden
```

---

## 3. Default Tables (Created by Migrations)
| Table | Purpose |
|-------|---------|
| AspNetUsers | Users (username, email, password hash, security stamps) |
| AspNetRoles | Role names (Admin, User, etc.) |
| AspNetUserRoles | Links users ? roles |
| AspNetUserClaims | Extra claims per user |
| AspNetRoleClaims | Claims attached to roles |
| AspNetUserLogins | External login providers |
| AspNetUserTokens | Tokens (reset password, email confirm, etc.) |

---

## 4. Add Identity To This Project (Step By Step)
1. Add NuGet packages:
   - Microsoft.AspNetCore.Identity.EntityFrameworkCore
   - Microsoft.EntityFrameworkCore.SqlServer (or other provider)
   - Microsoft.EntityFrameworkCore.Tools (for migrations)
2. Create a User model (optional if default OK):
```csharp
public class ApplicationUser : IdentityUser { /* Add custom props e.g. FirstName */ }
```
3. Create DbContext:
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
}
```
4. Register services in Program.cs:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
# ASP.NET Core Identity Overview

builder.Services.AddAuthentication(); // Identity adds cookie handler
```
5. Add connection string to appsettings.json:
```json
"ConnectionStrings": {
  "Default": "Server=.;Database=MyIdentityDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
6. Create migrations + database:
```
dotnet ef migrations add InitIdentity
dotnet ef database update
```
7. Build simple AuthController (Register / Login).
8. Protect endpoints with [Authorize].

---

## 5. Sample Minimal Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager; _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return Ok("Registered");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(dto.UserName, dto.Password, true, false);
        if (!result.Succeeded) return Unauthorized();
        return Ok("Logged in (cookie issued)");
    }
}
```
DTOs:
```csharp
public record RegisterDto(string UserName, string Email, string Password);
public record LoginDto(string UserName, string Password);
```

---

## 6. Authorize Endpoints
```csharp
[Authorize]
[HttpGet("me")]
public IActionResult Me() => Ok(User.Identity?.Name);

[Authorize(Roles = "Admin")]
[HttpGet("admin-area")]
public IActionResult AdminOnly() => Ok("Secret");

[Authorize(Policy = "CanViewReports")]
public IActionResult Reports() => Ok();
```
Add a policy:
```csharp
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("CanViewReports", p => p.RequireClaim("permission", "reports:view"));
});
```
Add a claim to a user:
```csharp
await _userManager.AddClaimAsync(user, new Claim("permission", "reports:view"));
```

---

## 7. Seeding Roles (Example)
```csharp
async Task SeedAsync(IServiceProvider sp)
{
    using var scope = sp.CreateScope();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var r in new[] { "Admin", "User" })
        if (!await roleMgr.RoleExistsAsync(r)) await roleMgr.CreateAsync(new IdentityRole(r));

    var admin = await userMgr.FindByNameAsync("admin");
    if (admin == null)
    {
        admin = new ApplicationUser { UserName = "admin", Email = "admin@test.local" };
        await userMgr.CreateAsync(admin, "Admin#123");
        await userMgr.AddToRoleAsync(admin, "Admin");
        await userMgr.AddClaimAsync(admin, new Claim("permission", "reports:view"));
    }
}
```
Call after build:
```csharp
await SeedAsync(app.Services);
```

---

## 8. Identity vs Basic Authentication (Current Project)
| Basic Auth | Identity |
|------------|----------|
| Sends username:password every request (Base64) | Sends cookie or token (password only at login) |
| No password hashing logic by default (you implement) | Built-in secure hashing + user lockout + stamps |
| No roles/claims out of box | Full roles, claims, policies |
| Stateless, simple | Feature rich, extensible |
| Not ideal for production | Recommended for real apps |

---

## ?? What Is ASP.NET Core Identity?
ASP.NET Core Identity is a built-in system for managing user authentication and authorization in ASP.NET Core applications. It handles login, registration, password reset, roles, and claims, and works with Entity Framework Core to store user data in a database. It easily integrates with MVC, Razor Pages, Blazor, and APIs.
## 9. Switching Strategy
You can keep BasicAuthenticationHandler for learning, but in production prefer Identity + Cookie (for web) or Identity + JWT (with custom token service). Identity does NOT force MVC; it can power pure Web APIs.

---

## ?? Core Components
| Component      | Purpose                                                        |
|---------------|----------------------------------------------------------------|
| IdentityUser   | Represents a user. Includes username, email, password hash, etc.|
| IdentityRole   | Represents a role like Admin, User, etc. Used for role-based access.|
| UserManager    | Handles user creation, password management, and profile updates.|
| RoleManager    | Manages roles—create, delete, assign to users.                 |
| SignInManager  | Handles login, logout, and external login logic.               |
| DbContext      | Connects Identity to your database using EF Core.              |
## 10. Extending the User
Add columns by adding properties to ApplicationUser and running a new migration.
```csharp
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName  { get; set; }
}
```
Run:
```
dotnet ef migrations add AddUserNames
dotnet ef database update
```

---

## ??? Default Tables Created
| Table Name         | What It Stores                                         |
|--------------------|-------------------------------------------------------|
| AspNetUsers        | User data: username, email, password hash, etc.       |
| AspNetRoles        | Role data: role names and concurrency info.           |
| AspNetUserRoles    | Links users to roles (many-to-many relationship).     |
| AspNetUserClaims   | Stores claims for each user (e.g., department, age).  |
| AspNetRoleClaims   | Stores claims for each role.                          |
| AspNetUserLogins   | External login info (Google, Facebook, etc.).         |
| AspNetUserTokens   | Tokens for password reset, email confirmation, etc.   |
## 11. Token (JWT) Option (Brief)
Identity can still manage users while you issue JWTs manually:
```csharp
var user = await _userManager.FindByNameAsync(dto.UserName);
if (await _userManager.CheckPasswordAsync(user, dto.Password))
{
   var claims = await _userManager.GetClaimsAsync(user);
   // build JWT with claims + roles
}
```

---

## ?? How It Works
- **User Registers** ? Info saved in AspNetUsers ? Email confirmation token sent
- **User Logs In** ? SignInManager checks credentials ? Cookie or token issued
- **Authorization** ? Role or claim checked ? Access granted or denied
- **Password Reset / 2FA** ? Token generated ? Verified via email/SMS
## 12. Troubleshooting Quick Tips
| Issue | Fix |
|-------|-----|
| Login always fails | Ensure cookies middleware order: UseAuthentication before UseAuthorization and MapControllers after both |
| Tables not created | Did you run migrations + update? Correct connection string? |
| Role check fails | Did you assign the role? Inspect claims via debugger | 
| 403 instead of 401 | Authenticated but forbidden (role/policy mismatch) |

---
## ?? Visual Diagram
![ASP.NET Core Identity Flow](https://devblogs.microsoft.com/dotnet/improvements-auth-identity-aspnetcore-8/identity.svg)

## 13. Visual (External) Diagram Reference
Official conceptual flow (external SVG):
https://devblogs.microsoft.com/dotnet/improvements-auth-identity-aspnetcore-8/identity.svg

---

## ??? Services Used
- **UserManager**: Provides APIs for managing user accounts. Examples: Create a new user, change passwords, update profile info.
- **RoleManager**: Manages roles in the system. Examples: Create roles like Admin, assign roles to users.
- **SignInManager**: Handles user sign-in and sign-out. Examples: Validate credentials, manage external logins (Google, Facebook).
- **DbContext**: Connects the Identity system to the database. Examples: Stores user data, roles, claims, and tokens.
## 14. Summary In One Sentence
ASP.NET Core Identity = Ready-made, secure user + role + claim system that saves you from building authentication plumbing yourself.

Happy Coding!
