# Claude Code Brief: HexWriter — Extraction from Seonyx

## Background

HexWriter is a new multi-user collaborative book editing application being spun out of **Seonyx**, an existing single-user ASP.NET MVC 5 book editor. The goal is to produce a clean, independently deployable application that retains all book editing functionality but replaces the single-user admin system with a proper multi-user permissions model supporting individual users and named collaborator groups.

The extraction is being done by **subtraction and replacement**, not by assembling from parts. Start with a full copy of the Seonyx codebase and work by removing what is not needed and replacing what needs to change.

**Target stack:** ASP.NET MVC 5, .NET Framework 4.6.2, Entity Framework 6, SQL Server, C#. Do not introduce additional frameworks or change the target framework version.

This brief is divided into four phases. **Complete each phase fully before beginning the next.** Each phase has explicit acceptance criteria.

---

## Phase 1: Clone, Rename, and Strip

### Objective
Produce a clean HexWriter solution from the Seonyx codebase with all Seonyx-specific branding, content pages, and identity removed, and all namespaces/project references updated to HexWriter.

### 1.1 Namespace and Project Rename

Perform a global rename throughout the entire solution:

- Solution name: `HexWriter`
- Project name: `HexWriter`
- Root namespace: `HexWriter` (replacing `Seonyx` everywhere)
- Assembly name: `HexWriter`

This includes:
- All `.cs` files (`namespace Seonyx.*` → `namespace HexWriter.*`, `using Seonyx.*` → `using HexWriter.*`)
- `.csproj` file
- `Web.config` (assembly references)
- `Global.asax` and `Global.asax.cs`
- `AssemblyInfo.cs`
- Any `RouteConfig`, `BundleConfig`, `FilterConfig` etc. under `App_Start`
- Razor views (any `@using Seonyx.*` directives)
- Layout files

Do not rename database table names, column names, or BookML XML element names — these are data contracts and must remain unchanged.

### 1.2 Remove Seonyx-Specific Content

Remove the following (these are Seonyx-specific and have no place in HexWriter):

- The public-facing website: home page, about page, any marketing or content pages and their controllers/views
- Any Seonyx branding: logos, site name text ("Seonyx") in layout files, favicons
- The `ContentAnalyserCli` project if present in the solution (it belongs to the Seonyx solution only)

**Retain everything that is part of the book editor:**
- All book, chapter, paragraph, draft controllers and views
- BookML import/export
- Character/Entity sketches
- Content Analysis views and controllers
- Draft analysis and work order UI
- All shared layout infrastructure, CSS, JavaScript, bundles

### 1.3 Rebrand for HexWriter

Make minimal branding substitutions:

- Site title in layout: "HexWriter"
- Browser title tag: "HexWriter"
- Any visible "Seonyx" text in navigation or footers: replace with "HexWriter"

Do not redesign the UI — this is a text substitution only.

### 1.4 Connection String

Update `Web.config` connection string name and value:
- Name: `HexWriterContext`
- Update all `DbContext` references accordingly
- Leave the actual server/database values as placeholders: `[DB_SERVER]`, `[DB_NAME]`, `[DB_USER]`, `[DB_PASSWORD]`

### Phase 1 Acceptance Criteria

- Solution builds without errors in Visual Studio 2022
- No remaining references to `Seonyx` in namespaces, using statements, assembly names, layout text, or page titles (database schema names and BookML XML elements excepted)
- The book editor is navigable and functional against the existing database (connection string aside)
- Public-facing content pages are gone; navigating to their former routes returns 404

---

## Phase 2: Authentication Replacement

### Objective

Replace the existing single-user web.config hash authentication with a proper database-backed multi-user authentication system using BCrypt salted password hashes.

### 2.1 Current Auth (to be removed)

Seonyx currently authenticates by comparing a submitted password against a hash stored in `web.config`. This entire mechanism must be removed and replaced.

Remove:
- Any password hash value from `web.config`
- The code that reads and compares that hash
- Any helper classes or utilities used solely for that comparison

### 2.2 Users Table

Add the following table to the database via an EF6 migration:

```sql
CREATE TABLE Users (
    Id              INT IDENTITY PRIMARY KEY,
    Username        NVARCHAR(100) NOT NULL UNIQUE,
    Email           NVARCHAR(255) NOT NULL UNIQUE,
    DisplayName     NVARCHAR(255) NOT NULL,
    PasswordHash    NVARCHAR(255) NOT NULL,   -- BCrypt hash, includes salt
    Role            NVARCHAR(50)  NOT NULL DEFAULT 'Reviewer',
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreatedAt       DATETIME      NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt     DATETIME      NULL
)
```

Roles are a simple string enum with two values: `Admin` and `Reviewer`. Do not implement a separate Roles table — this is intentionally kept simple.

### 2.3 Password Hashing

Use **BCrypt.Net-Next** (NuGet package `BCrypt.Net-Next`) for all password hashing and verification. BCrypt handles salting internally — do not implement a separate salting mechanism.

```csharp
// Hashing (on registration or password set)
string hash = BCrypt.Net.BCrypt.HashPassword(plaintextPassword);

// Verification (on login)
bool valid = BCrypt.Net.BCrypt.Verify(plaintextPassword, storedHash);
```

Work factor: use the default (12).

### 2.4 Login

Replace the existing login page and controller action with a new implementation:

- `GET /Account/Login` — renders login form (Username, Password fields)
- `POST /Account/Login` — validates credentials against the Users table using BCrypt verification, sets a Forms Authentication cookie on success, redirects to the book list on success, returns error message on failure
- `GET /Account/Logout` — clears the Forms Authentication cookie, redirects to login

Use ASP.NET Forms Authentication (`FormsAuthentication`) for the session cookie. Store the user's `Id`, `Username`, and `Role` in the auth cookie's `UserData` field as a pipe-delimited string: `{Id}|{Username}|{Role}`.

### 2.5 Admin Seed User

On application startup (in `Application_Start` in `Global.asax.cs`), check whether any `Admin` user exists in the Users table. If none exists, create one with the following credentials:

- Username: `admin`
- Password: `ChangeMe123!`
- Role: `Admin`
- DisplayName: `Administrator`

This ensures the application is usable immediately after database creation. The admin user should change this password after first login (no enforcement required in this phase).

### 2.6 Protect Existing Routes

All existing book editor routes must require authentication. Apply the `[Authorize]` attribute at controller level to all controllers that previously sat behind the admin login. The login page itself must remain publicly accessible.

### Phase 2 Acceptance Criteria

- No password or hash exists in `web.config`
- BCrypt.Net-Next package is referenced and used for all password operations
- A new user can log in with the seed admin credentials
- Unauthenticated requests to book editor routes redirect to `/Account/Login`
- Successful login redirects to the book list
- Logout clears the session and redirects to login
- Incorrect credentials produce an error message without revealing whether the username or password was wrong

---

## Phase 3: Permissions Layer

### Objective

Introduce group-based and individual book-level access control so that an Admin can share specific books with named collaborator groups or individual Reviewer users, and Reviewers see only the books they have been granted access to.

### 3.1 Schema

Add the following tables via EF6 migrations:

```sql
-- Named collaborator groups (e.g. "Mayfly Translation Team")
CREATE TABLE Groups (
    Id          INT IDENTITY PRIMARY KEY,
    Name        NVARCHAR(255) NOT NULL UNIQUE,
    Description NVARCHAR(1000) NULL,
    CreatedAt   DATETIME NOT NULL DEFAULT GETUTCDATE()
)

-- Users belonging to groups
CREATE TABLE GroupUsers (
    Id          INT IDENTITY PRIMARY KEY,
    GroupId     INT NOT NULL REFERENCES Groups(Id),
    UserId      INT NOT NULL REFERENCES Users(Id),
    AddedAt     DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UNIQUE (GroupId, UserId)
)

-- Groups granted access to a book
CREATE TABLE BookGroups (
    Id          INT IDENTITY PRIMARY KEY,
    BookId      INT NOT NULL REFERENCES Books(Id),
    GroupId     INT NOT NULL REFERENCES Groups(Id),
    AccessLevel NVARCHAR(50) NOT NULL DEFAULT 'Read',  -- 'Read' or 'Edit'
    GrantedAt   DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UNIQUE (BookId, GroupId)
)

-- Individual user access grants (for one-off access outside a group)
CREATE TABLE BookUsers (
    Id          INT IDENTITY PRIMARY KEY,
    BookId      INT NOT NULL REFERENCES Books(Id),
    UserId      INT NOT NULL REFERENCES Users(Id),
    AccessLevel NVARCHAR(50) NOT NULL DEFAULT 'Read',  -- 'Read' or 'Edit'
    GrantedAt   DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UNIQUE (BookId, UserId)
)
```

### 3.2 Effective Access Resolution

A user's effective access level for a book is determined as follows:

1. If the user is `Admin`, full access — no lookup required.
2. Check `BookUsers` for a direct individual grant.
3. Check `BookGroups` for any group the user belongs to via `GroupUsers`.
4. If both a direct grant and one or more group grants exist, take the most permissive access level (`Edit` beats `Read`).
5. If no grant exists, the user has no access to the book.

Implement this as a service method:

```csharp
// Returns null if no access
AccessLevel? GetEffectiveAccess(int userId, int bookId)
```

This method must be called server-side on every book, chapter, and paragraph action — do not rely solely on UI suppression.

### 3.3 Access Rules

**Admin users:**
- See all books unconditionally
- Can create, edit, and delete books, chapters, paragraphs
- Can manage user accounts and groups
- Can grant and revoke book access for groups and individual users

**Reviewer users:**
- See only books where they have an effective access level (via direct grant or group membership)
- `AccessLevel = 'Read'`: can view books, chapters, paragraphs — no create, edit, or delete
- `AccessLevel = 'Edit'`: can edit paragraph content — no structural changes (no adding/removing chapters or paragraphs)
- Cannot see the user management or group management areas
- Cannot see other users' details

### 3.4 Book List

Modify the book list view and controller action:

- Admin: return all books (existing behaviour)
- Reviewer: return only books where `GetEffectiveAccess` returns a non-null result, with the effective access level included in the view model

For Reviewer users, display the book list with a clear visual indicator showing their access level per book (e.g. "Read Only" or "Editor").

### 3.5 Read-Only Enforcement

For Reviewer users with `AccessLevel = 'Read'`:

- Remove edit/delete buttons from book, chapter, and paragraph views — do not hide them with CSS, exclude them from Razor output entirely using role/access checks
- All `POST`, `PUT`, `DELETE` controller actions for books, chapters, and paragraphs must call `GetEffectiveAccess` server-side and return `403 Forbidden` if the user lacks write access — do not rely solely on UI suppression

### 3.6 User Management (Admin Only)

Add a user management area accessible only to Admin users at `/Admin/Users`:

- List all users (Id, Username, DisplayName, Role, IsActive, LastLoginAt)
- Create a new user (Username, Email, DisplayName, Role, initial password)
- Edit user (DisplayName, Role, IsActive)
- Reset a user's password (Admin provides new password; no email flow in this phase)

### 3.7 Group Management (Admin Only)

Add a group management area accessible only to Admin users at `/Admin/Groups`:

- List all groups (Name, Description, member count)
- Create a group (Name, Description)
- Edit a group (Name, Description)
- Delete a group (with confirmation; removes all `GroupUsers` and `BookGroups` records)
- View group members — add and remove users from a group

### 3.8 Book Access Management (Admin Only)

On the book detail view (Admin only), add an access management panel:

**Group access section:**
- List groups currently granted access (Group Name, AccessLevel, GrantedAt)
- Form to grant access to a group: select group from dropdown, choose AccessLevel, submit
- Revoke button per group grant

**Individual access section:**
- List users with direct individual grants (Username, AccessLevel, GrantedAt)
- Form to grant access to an individual user: select user from dropdown, choose AccessLevel, submit
- Revoke button per individual grant

### Phase 3 Acceptance Criteria

- Admin can create a Group, add a Reviewer user to it, and grant the Group read access to a specific book
- Reviewer logs in and sees only that book in their book list, labelled as Read Only
- Reviewer cannot access any other book — direct URL navigation returns 403
- Reviewer with Read access cannot edit or delete anything — both UI suppression and server-side enforcement confirmed
- Admin can also grant access to an individual user directly without a group
- Where a user has both a direct grant and a group grant, the most permissive level applies
- Admin user and group management pages return 403 if accessed by a Reviewer
- All passwords stored as BCrypt hashes — no plaintext anywhere

---

## Phase 4: Self-Service Password Reset and TOTP 2FA

### Objective

Add self-service password reset via email and optional TOTP-based two-factor authentication (e.g. Google Authenticator).

### 4.1 SMTP Configuration

Add the following keys to `Web.config` under `<appSettings>`:

```xml
<add key="Smtp:Host" value="[SMTP_HOST]" />
<add key="Smtp:Port" value="587" />
<add key="Smtp:Username" value="[SMTP_USERNAME]" />
<add key="Smtp:Password" value="[SMTP_PASSWORD]" />
<add key="Smtp:FromAddress" value="noreply@hexwriter.com" />
<add key="Smtp:FromName" value="HexWriter" />
<add key="App:BaseUrl" value="[BASE_URL]" />
```

Implement an `EmailService` class that wraps `System.Net.Mail.SmtpClient` using these config values. All email sending goes through this service.

### 4.2 Password Reset Schema

Add the following table via an EF6 migration:

```sql
CREATE TABLE PasswordResetTokens (
    Id          INT IDENTITY PRIMARY KEY,
    UserId      INT NOT NULL REFERENCES Users(Id),
    Token       NVARCHAR(255) NOT NULL UNIQUE,  -- cryptographically random, URL-safe
    ExpiresAt   DATETIME NOT NULL,
    UsedAt      DATETIME NULL,
    CreatedAt   DATETIME NOT NULL DEFAULT GETUTCDATE()
)
```

### 4.3 Password Reset Flow

**Request reset:**
- `GET /Account/ForgotPassword` — renders form with Email field
- `POST /Account/ForgotPassword` — looks up user by email; if found, generates a cryptographically random URL-safe token (use `System.Security.Cryptography.RandomNumberGenerator`), stores it in `PasswordResetTokens` with a 1-hour expiry, and sends a reset email. Always return the same success message regardless of whether the email was found — do not reveal whether an account exists for a given email address.

**Reset email:**
Plain text email (no HTML required) containing:
```
You requested a password reset for your HexWriter account.

Click the link below to reset your password. This link expires in 1 hour.

[reset link]

If you did not request this, you can ignore this email.
```

**Complete reset:**
- `GET /Account/ResetPassword?token={token}` — validates token (exists, not expired, not used); renders new password form if valid; renders error page if invalid or expired
- `POST /Account/ResetPassword` — validates token again, hashes new password with BCrypt, updates `Users.PasswordHash`, marks token as used (`UsedAt = GETUTCDATE()`), redirects to login with success message

### 4.4 TOTP Two-Factor Authentication

Add the following columns to the `Users` table via an EF6 migration:

```sql
ALTER TABLE Users ADD TotpSecret   NVARCHAR(255) NULL  -- Base32-encoded TOTP secret
ALTER TABLE Users ADD TotpEnabled  BIT NOT NULL DEFAULT 0
```

Use the NuGet package **OtpNet** for TOTP generation and verification.

**Enrolment flow (authenticated users only):**
- `GET /Account/Enable2FA` — generates a new TOTP secret, stores it temporarily in session, renders a QR code and the manual entry key. Use a QR code library or generate a `otpauth://` URI and render the QR client-side using a JavaScript QR library already present in the project or a CDN-hosted one.
- `POST /Account/Enable2FA` — user submits a 6-digit code to confirm enrolment; verify against the session-stored secret using OtpNet; on success, persist the secret to `Users.TotpSecret` and set `TotpEnabled = 1`

**Login flow with 2FA:**
- If a user has `TotpEnabled = 1`, after successful username/password verification do not issue the auth cookie immediately. Instead redirect to `GET /Account/TwoFactor`, which renders a 6-digit code entry form.
- `POST /Account/TwoFactor` — verify the submitted code against the user's `TotpSecret` using OtpNet with a ±1 step tolerance window; on success issue the auth cookie and redirect to book list; on failure return error message.
- Store the pending authenticated user's Id in a short-lived session key during the 2FA step — do not issue a partial auth cookie.

**Disable 2FA:**
- `POST /Account/Disable2FA` — requires the user to confirm their current password; on success clears `TotpSecret` and sets `TotpEnabled = 0`

### Phase 4 Acceptance Criteria

- A user can request a password reset, receive an email, follow the link, and set a new password
- Expired or already-used reset tokens are rejected with an appropriate error message
- The forgot password form never reveals whether an email address has an account
- A user can enrol a TOTP authenticator app, and subsequent logins require the 6-digit code after password entry
- An invalid TOTP code is rejected; a valid code issues the session
- A user can disable 2FA after confirming their password
- No TOTP secret is ever exposed in a view after enrolment is complete
- All SMTP config values are in `web.config` as placeholders — no hardcoded credentials

---

## General Notes for All Phases

- **Audit before touching:** before writing any code in each phase, examine the existing codebase to understand the conventions in use — naming, EF6 patterns, Razor layout structure, JavaScript approach, CSS framework. New code must be consistent with existing code. Report anything unexpected before proceeding.
- **EF6 migrations:** all schema changes must be implemented as EF6 Code First migrations, not raw SQL scripts or manual schema changes.
- **No new frontend frameworks:** do not introduce React, Vue, Angular, or any frontend framework not already present in the project.
- **XML throughout:** any configuration or data format choices should prefer XML over JSON, consistent with the project's existing conventions. JSON is acceptable only for client-side JavaScript state.
- **Token efficiency:** do not generate speculative code, unused scaffolding, or test data beyond what the acceptance criteria require. Ask before doing anything that seems outside the scope of the current phase.
