FROM node:20 AS frontend-build
WORKDIR /src/frontend

# Copy package.json first to leverage Docker cache for dependencies
COPY frontend/package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the frontend source code
COPY frontend/ .

# Build the app
# (Assumes Vite builds to 'dist'. If using Create-React-App, change 'dist' to 'build' below)
RUN npm run build


# =========================
# STAGE 2: Build Backend (.NET 8)
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src/backend

# Copy csproj and restore dependencies
COPY backend/*.csproj ./
RUN dotnet restore

# Copy the rest of the backend source code
COPY backend/ .

# Build and Publish the application to /app/publish
RUN dotnet publish -c Release -o /app/publish


# =========================
# STAGE 3: Final Runtime Image
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080

# 1. Copy the compiled .NET application from the backend-build stage
COPY --from=backend-build /app/publish .

# 2. Copy the compiled React app from frontend-build stage into the wwwroot folder
COPY --from=frontend-build /src/frontend/dist ./wwwroot

# 3. Define the entry point
ENTRYPOINT ["dotnet", "backend.dll"]