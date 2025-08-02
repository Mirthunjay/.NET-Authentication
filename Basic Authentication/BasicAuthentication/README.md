# Basic Authentication Example (.NET 8)

## What is Basic Authentication?
Basic Authentication is a simple authentication scheme built into the HTTP protocol. It requires the client to send a username and password with each request, encoded in Base64, in the `Authorization` header.

---

## When to Use
- For simple APIs or internal tools.
- When using HTTPS to encrypt traffic.
- When you need quick, stateless authentication.

## When NOT to Use
- For public-facing or production applications.
- When you need strong security (use OAuth, JWT, etc. instead).
- If you cannot guarantee HTTPS (credentials are sent in every request).

---

## Advantages
- Easy to implement.
- Stateless (no server-side session required).
- Supported by most HTTP clients and browsers.

## Disadvantages
- Credentials sent with every request (must use HTTPS).
- No password hashing or advanced security features.
- Not suitable for complex or public applications.

---

## Project Structure & Implementation
```
BasicAuthentication/
├── Controllers/
│   └── UsersController.cs         # User API endpoints
├── Common/
│   └── BasicAuthenticationHandler.cs # Authentication logic
├── Service/
│   ├── Interface/IUserRepository.cs  # User data interface
│   └── RepositoryImplementation/UserRepository.cs # User data implementation
├── Program.cs                    # Configures services and middleware
├── BasicAuthentication.http      # Example HTTP requests
└── README.md                     # Project documentation
```

---

## Key Classes & Methods – Authentication Flow

![Basic Authentication Flow](https://learn.microsoft.com/en-us/aspnet/web-api/overview/security/basic-authentication/_static/image1.png)

**Step-by-Step Authentication Process:**

| Step | Description |
|------|-------------|
| 1 | **Client Request:** Sends `Authorization: Basic <Base64(username:password)>` header. |
| 2 | **HandleAuthenticateAsync:** Decodes credentials, validates user. |
| 3 | **User Valid?**<br>• Yes: Create Claims → ClaimsIdentity → ClaimsPrincipal → AuthenticationTicket → Access Granted.<br>• No: Go to HandleChallengeAsync. |
| 4 | **HandleChallengeAsync:** Sends 401 Unauthorized and `WWW-Authenticate` header. |

**Class Roles:**
- **Claim:** Represents user info (e.g., username, user ID).
- **ClaimsIdentity:** Holds a set of claims for a user.
- **ClaimsPrincipal:** Represents the authenticated user (with one or more identities).
- **AuthenticationTicket:** Contains the principal and authentication scheme.

---

## How Authentication Works (Step-by-Step)
1. **Client sends request** with `Authorization: Basic <Base64(username:password)>` header.
2. **HandleAuthenticateAsync** decodes credentials and validates user.
3. If valid, creates claims and authentication ticket.
4. If invalid, **HandleChallengeAsync** sends a 401 Unauthorized response.
5. Middleware checks authentication before allowing access to protected endpoints.

---

## Registering in Program.cs
```csharp
builder.Services.AddControllers();
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options => { });
builder.Services.AddSingleton<IUserRepository, UserRepository>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

## Example HTTP Requests (User API)
See `BasicAuthentication.http` for sample requests.
