# ASP.NET Core Identity Overview

## 🧠 What Is ASP.NET Core Identity?
ASP.NET Core Identity is a built-in system for managing user authentication and authorization in ASP.NET Core applications. It handles login, registration, password reset, roles, and claims, and works with Entity Framework Core to store user data in a database. It easily integrates with MVC, Razor Pages, Blazor, and APIs.

---

## 🧩 Core Components
| Component      | Purpose                                                        |
|---------------|----------------------------------------------------------------|
| IdentityUser   | Represents a user. Includes username, email, password hash, etc.|
| IdentityRole   | Represents a role like Admin, User, etc. Used for role-based access.|
| UserManager    | Handles user creation, password management, and profile updates.|
| RoleManager    | Manages roles—create, delete, assign to users.                 |
| SignInManager  | Handles login, logout, and external login logic.               |
| DbContext      | Connects Identity to your database using EF Core.              |

---

## 🗃️ Default Tables Created
| Table Name         | What It Stores                                         |
|--------------------|-------------------------------------------------------|
| AspNetUsers        | User data: username, email, password hash, etc.       |
| AspNetRoles        | Role data: role names and concurrency info.           |
| AspNetUserRoles    | Links users to roles (many-to-many relationship).     |
| AspNetUserClaims   | Stores claims for each user (e.g., department, age).  |
| AspNetRoleClaims   | Stores claims for each role.                          |
| AspNetUserLogins   | External login info (Google, Facebook, etc.).         |
| AspNetUserTokens   | Tokens for password reset, email confirmation, etc.   |

---

## 🔄 How It Works
- **User Registers** → Info saved in AspNetUsers → Email confirmation token sent
- **User Logs In** → SignInManager checks credentials → Cookie or token issued
- **Authorization** → Role or claim checked → Access granted or denied
- **Password Reset / 2FA** → Token generated → Verified via email/SMS

---
## 🧠 Visual Diagram
![ASP.NET Core Identity Flow](https://devblogs.microsoft.com/dotnet/improvements-auth-identity-aspnetcore-8/identity.svg)

---

## 🛠️ Services Used
- **UserManager**: Provides APIs for managing user accounts. Examples: Create a new user, change passwords, update profile info.
- **RoleManager**: Manages roles in the system. Examples: Create roles like Admin, assign roles to users.
- **SignInManager**: Handles user sign-in and sign-out. Examples: Validate credentials, manage external logins (Google, Facebook).
- **DbContext**: Connects the Identity system to the database. Examples: Stores user data, roles, claims, and tokens.
