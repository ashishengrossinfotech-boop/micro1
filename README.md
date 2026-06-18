# Login Demo (ASP.NET Core MVC, .NET 8)

A self-contained login module built with ASP.NET Core MVC. Authentication is
a lightweight, demo-style cookie login (no ASP.NET Core Identity, no database) —
users live in an in-memory list. It's meant as a clean starting point you can
swap pieces out of (plug in a real database, real email sending, Identity, etc.).

## Features

- Login page with username/password, "remember me", and a **Forgot your password?** link
- Forgot-password flow: enter an email, get a reset link (shown directly on screen
  since there's no SMTP server wired up for this demo — swap in a real email service
  for production)
- Reset-password page: set a new password using the link's token (tokens expire
  after 30 minutes and are single-use)
- Cookie-based authentication, `[Authorize]`-protected home page, logout
- Passwords are hashed (SHA-256), never stored or compared in plain text
- Anti-forgery tokens on every form post

## Local demo accounts

These accounts are seeded only when the app runs in the `Development`
environment. They are not seeded or displayed for production-facing builds.

| Username | Password   |
|----------|------------|
| admin    | Admin@123  |
| demo     | Demo@123   |

## Running it

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd LoginDemo
dotnet run
```

Then open the URL shown in the console (defaults to `http://localhost:5188`).
It will land you on the login page.

## Project layout

```
LoginDemo/
├── Controllers/
│   ├── AccountController.cs   Login, Logout, ForgotPassword, ResetPassword
│   └── HomeController.cs      Protected landing page
├── Models/                    View models + the User entity
├── Services/
│   ├── IUserService.cs
│   └── UserService.cs         In-memory user store, password hashing, reset tokens
├── Views/
│   ├── Account/               Login, ForgotPassword, ResetPassword + confirmation views
│   └── Home/                  Index (protected), Error
└── wwwroot/                   CSS/JS (Bootstrap loaded via CDN)
```

## Notes on the demo simplifications

- **No database** — the `UserService` singleton holds users in memory, so data
  resets whenever the app restarts. Swap it for an EF Core / Dapper-backed
  implementation behind the same `IUserService` interface to persist data.
- **No real email** — `ForgotPassword` builds a real reset link with a real,
  time-limited token, but instead of emailing it, the confirmation page
  displays it directly so you can test the flow. Plug in an email/SMTP service
  to send it instead.
- **Custom cookie auth, not ASP.NET Core Identity** — kept intentionally simple.
  If you need roles, external logins, 2FA, or email confirmation, Identity is
  the better long-term fit.
