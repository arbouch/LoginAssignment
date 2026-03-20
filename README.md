# MiniPortal - Simple Run Guide

!!!!!!!I chose Minimal API because this project has a small scope and only a few endpoints, so it keeps the implementation simple, fast to deliver, and easy to evolve later if needed.!!!!!!!!!
This guide explains, step by step, how to start this project even if you are not from a technical background.

## What this project has
- **Backend**: the "engine" (API) that handles login and data.
- **Frontend**: the web page you open in your browser.
- **Tests**: automatic checks to confirm core features are working.

## Before you start (one-time setup)
1. Install **.NET 8 SDK** (required).
2. Open **PowerShell**.
3. Go to the **root folder of this repository** on your computer — the folder that contains **`MiniPortal.sln`** (that file is the solution; the folder is *not* required to be named `MiniPortal`).

   - If you used **Git clone**, the folder is usually named like the repo (for example **`LoginAssignment`**), not `MiniPortal`.
   - If you unzipped a **ZIP**, use whatever folder name you have, as long as **`MiniPortal.sln`** is inside it.

```powershell
cd "YOUR_PATH_TO_FOLDER_THAT_CONTAINS_MiniPortal.sln"
```

Example (your path will differ):

```powershell
cd "C:\Users\You\source\repos\LoginAssignment"
```

Optional check:

```powershell
dotnet --version
```

If you see a version number (for example `8.x.x`), you are ready.

## Exact ports used
- Backend URL: `https://localhost:7100`
- Frontend URL: `https://localhost:7200`

## How to run the backend
In a terminal, go to the **repository root** (the folder that contains `MiniPortal.sln`), then run:

```powershell
dotnet run --project src/Backend.Api/Backend.Api.csproj --launch-profile http -c Debug
```

**Important:** Use **forward slashes** (`/`) as above. If you use Windows backslashes (`\`) inside **Git Bash** or some terminals, `\s` and `\B` can be treated as escape sequences and the path breaks (you may see errors like `Project file does not exist` or a mangled path).

What to expect:
- It starts the backend service.
- Keep this terminal open while using the app.

## How to run the frontend
Open a **second** terminal window and go to the **same** folder (the one that contains `MiniPortal.sln`):

```powershell
cd "YOUR_PATH_TO_FOLDER_THAT_CONTAINS_MiniPortal.sln"
```

Then run:

```powershell
dotnet run --project src/Frontend.Blazor/Frontend.Blazor.csproj --launch-profile http -c Debug
```

What to expect:
- Your browser should open the frontend.
- The first page is the login page.
- If it does not open automatically, manually open `https://localhost:7200`.

## Login credentials (for testing)
- Username: `admin`
- Password: `Admin123!`

## How to run automated tests
Stop running apps first (recommended), then in PowerShell from project folder run:

```powershell
dotnet test tests/Backend.Api.Tests/Backend.Api.Tests.csproj -c Debug
```

What to expect:
- You should see a result like "Passed" when tests are successful.

## Quick full workflow (recommended)
1. Start backend (first terminal).
2. Start frontend (second terminal).
3. Open `https://localhost:7200`.
4. Login with `admin / Admin123!`.
5. Run tests with `dotnet test` when needed.

## If something does not work
- Check both terminals are still open and running.
- Make sure backend is running before frontend login.
- Re-run commands exactly as written above.

## JWT Claims used (and why)
- `sub`: stores the user ID as a stable identifier of the authenticated user.
- `username`: stores the username to make debugging and simple UI display easier.

I chose these two claims because they are enough for the current scope: identify who the user is and carry basic display information without adding unnecessary complexity.

## Pending / Improvements (if applicable)
- Add Swagger documentation so anyone can test API endpoints from the browser and understand request/response formats quickly.
- Replace simple SHA256 password hashing with a **strong adaptive hash** (for example BCrypt or PBKDF2 with salt and many iterations).
- Add **refresh tokens** and a token revocation strategy for safer authentication in production.
- Add more automated tests (invalid payloads, token expiration, and end-to-end login/data flow).
- Improve logging and error handling with structured logs and user-friendly error messages.

