# Online Delivery API

.NET REST API for the Online Delivery application.

This API provides backend services for users, restaurants, menus, orders,
pricing, notifications, OTP-based authentication, PDF generation, and AI features.

## Tech Stack
- ASP.NET Core Web API
- PostgreSQL
- Entity Framework Core
- QuestPDF for server-side PDF generation
- SMS provider for Indian mobile OTP
- Azure hosting

## Local Development
```bash
dotnet restore
dotnet build
dotnet run
```

## API Base URL
```text
http://localhost:5000/api/v1
```
