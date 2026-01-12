# Gamerzilla.Net

Gamerzilla.Net is an open-source game achievement system backend and frontend. It provides a web interface for users to view trophies and an API for games to report progress.

This project has been updated to support **Docker**, **Dynamic BaseURL Configuration**, and **Automatic Database** creation making deployment significantly easier 

## 🚀 Quick Start (Docker)

The easiest way to run Gamerzilla is using Docker. This method handles the frontend, backend, and database setup automatically.

### 1. Run with SQLite (Simplest)
This creates a local `gamerzilla.db` file in the container.

```bash
docker run -d \
  --name gamerzilla \
  -p 8080:8080 \
  -e RequireSslCookies=false \
  -e RegistrationOptions__Allow=true \
  -v $(pwd)/data:/app/data \
  gamerzilla:latest
```

### 2. Run with Docker Compose (Recommended)
Create a `docker-compose.yml` file to manage configuration easily.

```yaml
version: '3.8'

services:
  gamerzilla:
    image: gamerzilla:latest
    container_name: gamerzilla
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - RequireSslCookies=false 
      - BasePath=/trophy
      # Database
      - ConnectionStrings__SqlType=sqlite
      - ConnectionStrings__TrophyConnection=Data Source=/data/gamerzilla.db
      # Features
      - RegistrationOptions__Allow=true
    volumes:
      - ./data:/data
```

### 3. Register initial user

Navigate to the front end and click "Register". The first registered user is automatically approved and made Admin.
---

## ⚙️ Configuration (Environment Variables)

Gamerzilla is fully configurable via Environment Variables. You do not need to rebuild the image to change these settings.

### 🔹 Core Application Settings
| Variable | Default | Description |
| :--- | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Set to `Development` for debug logs and Swagger UI. |
| `ASPNETCORE_URLS` | `http://+:8080` | The ports/protocols the internal server listens on. |
| `BasePath` | `""` (Root) | Run the app in a subfolder (e.g., `/trophy`). |
| `RequireSslCookies` | `true` | **Crucial:** Set `false` if using HTTP (localhost/LAN). Keep `true` for HTTPS. |
| `AppMode` | *(Optional)* | Specific application mode flags. |
| `FrontEnd` | *(Optional)* | Configuration for frontend integration. |

### 🔹 Database Settings
| Variable | Default | Description |
| :--- | :--- | :--- |
| `ConnectionStrings__SqlType` | `sqlite` | Choose `sqlite` or `postgresql`. |
| `ConnectionStrings__TrophyConnection` | *(Required)* | Connection string. <br>SQLite: `Data Source=gamerzilla.db`<br>Postgres: `Host=...;Database=...;Username=...;Password=...` |

### 🔹 Registration & User Policy
| Variable | Default | Description |
| :--- | :--- | :--- |
| `RegistrationOptions__Allow` | `false` | Set `true` to allow new users to sign up. |
| `RegistrationOptions__RequireApproval` | `false` | If `true`, admins must approve new users before they can log in. |
| `RegistrationOptions__RequireEmailVerification` | `false` | If `true`, sends an email link to verify the account. |
| `RegistrationOptions__AdminUsername` | *(None)* | Pre-seeds an admin user (if supported by your DB init logic). |

### 🔹 Email Configuration
*Required if Email Verification is enabled.*

| Variable | Example | Description |
| :--- | :--- | :--- |
| `EmailSettings__Enabled` | `true` | Master switch to enable email features. |
| `EmailSettings__SmtpServer` | `smtp.gmail.com` | Hostname of your SMTP provider. |
| `EmailSettings__Port` | `587` | SMTP port (usually 587 or 465). |
| `EmailSettings__SenderName` | `Gamerzilla Bot` | The display name on outgoing emails. |
| `EmailSettings__SenderEmail` | `bot@example.com` | The "From" address. |
| `EmailSettings__Username` | `user@example.com` | SMTP Username. |
| `EmailSettings__Password` | `secret` | SMTP Password. |
| `EmailSettings__ValidateSsl` | `true` | Enforce valid SSL certificates. |

---

## 🗄️ Database Setup

The application automatically creates the database schema on startup if it does not exist.

### Using PostgreSQL
To use Postgres instead of SQLite, update your environment variables:

```bash
ConnectionStrings__SqlType=postgresql
ConnectionStrings__TrophyConnection="Host=my-postgres-host;Database=gamerzilla;Username=postgres;Password=securepassword"
```

---

## 🌐 Sub-path Hosting (Virtual Folder)

You can host Gamerzilla under a virtual path (e.g., `example.com/trophy`) without rebuilding the Docker image.

1. Set the `BasePath` environment variable (e.g., `BasePath=/trophy`).
2. The frontend automatically detects this configuration at runtime via the config injection system.

---

## 🛠️ Development Setup

If you want to contribute code or run it without Docker:

### Prerequisites
* .NET SDK 8.0+
* Node.js & npm
* SQLite or PostgreSQL

### 1. Frontend
```bash
cd frontend
npm install
# In dev, the frontend runs on port 5173 and proxies API requests to port 5000
npm start 
```

### 2. Backend
```bash
cd backend
# Create a local .env file or set variables in your IDE
dotnet run
```
*The backend defaults to port 5000 in Development.*

---

## ⚠️ Troubleshooting

**I can't sign in (Login loop)**
If you are running on `http://localhost` or a LAN IP, ensure `RequireSslCookies=false`. Modern browsers drop "Secure" cookies if the connection is not HTTPS.

**Page Not Found when using BasePath**
Ensure you are accessing the URL with the path included (e.g., `/trophy`).