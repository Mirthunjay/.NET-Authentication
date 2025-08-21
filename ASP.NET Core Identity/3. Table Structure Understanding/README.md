# ASP.NET Core Identity Tables - A Developer's Guide

This guide provides a clear understanding of ASP.NET Core Identity tables and their relationships using real-world scenarios.

## Core Tables Overview

### 1. AspNetUsers
This is the main table that stores user information.

**Key Fields:**
- Id: Unique identifier for each user
- UserName: User's login name
- Email: User's email address
- PasswordHash: Encrypted password
- PhoneNumber: User's contact number

**Real-world Scenario:**
```
When John signs up for your e-commerce website:
- Id: "a12b3c4d-5e6f-7g8h-9i0j"
- UserName: "john.doe"
- Email: "john.doe@email.com"
- PasswordHash: "[encrypted-password]"
- PhoneNumber: "+1234567890"
```

### 2. AspNetRoles
Stores different roles available in your application.

**Key Fields:**
- Id: Unique identifier for each role
- Name: Name of the role
- NormalizedName: Uppercase version of the role name

**Real-world Scenario:**
```
In an e-commerce website:
1. Admin Role:
   - Id: "1a2b3c4d-5e6f-7g8h-9i0j"
   - Name: "Admin"
   - NormalizedName: "ADMIN"

2. Customer Role:
   - Id: "2b3c4d5e-6f7g-8h9i-0j1k"
   - Name: "Customer"
   - NormalizedName: "CUSTOMER"
```

### 3. AspNetUserRoles
Links users to their roles (Many-to-Many relationship).

**Key Fields:**
- UserId: References AspNetUsers.Id
- RoleId: References AspNetRoles.Id

**Real-world Scenario:**
```
When John is assigned as both customer and store manager:
1. Entry 1:
   - UserId: [John's User Id]
   - RoleId: [Customer Role Id]

2. Entry 2:
   - UserId: [John's User Id]
   - RoleId: [Store Manager Role Id]
```

### 4. AspNetUserClaims
Stores additional user information as claims.

**Key Fields:**
- Id: Unique identifier for the claim
- UserId: References AspNetUsers.Id
- ClaimType: Type of the claim
- ClaimValue: Value of the claim

**Real-world Scenario:**
```
John's premium membership details:
- Id: 1
- UserId: [John's User Id]
- ClaimType: "SubscriptionTier"
- ClaimValue: "Premium"

John's shipping address:
- Id: 2
- UserId: [John's User Id]
- ClaimType: "ShippingAddress"
- ClaimValue: "123 Main St, City, Country"
```

### 5. AspNetUserLogins
Manages external login providers (like Google, Facebook).

**Key Fields:**
- LoginProvider: Name of the external provider
- ProviderKey: User's key at the provider
- UserId: References AspNetUsers.Id

**Real-world Scenario:**
```
When John uses Google to login:
- LoginProvider: "Google"
- ProviderKey: "[Google-provided-unique-id]"
- UserId: [John's Local User Id]
```

### 6. AspNetUserTokens
Stores tokens for users (like reset password tokens).

**Key Fields:**
- UserId: References AspNetUsers.Id
- LoginProvider: Provider name
- Name: Token name
- Value: Token value

**Real-world Scenario:**
```
When John requests a password reset:
- UserId: [John's User Id]
- LoginProvider: "DefaultProvider"
- Name: "ResetPassword"
- Value: "[encrypted-reset-token]"
```

## Common Operations and Their Table Impact

### 1. User Registration
```
When a new user registers:
1. AspNetUsers → New row added with user details
2. AspNetUserRoles → New row added with default role (if configured)
```

### 2. Role Assignment
```
When assigning an admin role to a user:
1. AspNetUserRoles → New row linking user to admin role
```

### 3. Password Reset
```
During password reset process:
1. AspNetUserTokens → New reset token stored
2. AspNetUsers → Password hash updated after reset
```

### 4. Social Login
```
When user logs in with Google:
1. AspNetUserLogins → New row for Google login
2. AspNetUsers → New user created (if first time)
```

## Security Best Practices
1. Never store plain passwords - ASP.NET Core Identity automatically handles password hashing
2. Use claims for temporary privileges instead of creating new roles
3. Regular cleanup of expired tokens from AspNetUserTokens
4. Implement proper token expiration for password reset and email confirmation

## Common Queries Examples

### 1. Find All Users in a Role
```sql
SELECT u.*
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Admin'
```

### 2. Get User's Claims
```sql
SELECT ClaimType, ClaimValue
FROM AspNetUserClaims
WHERE UserId = '[user-id]'
```

### 3. Find Users with External Logins
```sql
SELECT u.*
FROM AspNetUsers u
JOIN AspNetUserLogins ul ON u.Id = ul.UserId
WHERE ul.LoginProvider = 'Google'
```

## Troubleshooting Tips
1. User can't login? Check AspNetUsers for correct username/email
2. Role not working? Verify AspNetUserRoles has the correct mapping
3. External login issues? Check AspNetUserLogins for proper provider setup
4. Token invalid? Verify expiration in AspNetUserTokens

Remember: These tables are automatically managed by ASP.NET Core Identity. Direct table manipulation should be avoided unless absolutely necessary.
