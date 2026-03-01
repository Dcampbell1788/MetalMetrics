# SQL Server LocalDB Setup

## Overview

MetalMetrics uses SQL Server as its database. For local development, it connects to a **LocalDB** instance — a lightweight SQL Server that ships with Visual Studio.

## Prerequisites

- Visual Studio 2022 (any edition) with the **Data storage and processing** workload, or
- SQL Server Express LocalDB installed standalone

## Setup Steps

### 1. Verify LocalDB is installed

```bash
sqllocaldb info
```

You should see one or more instances listed (e.g., `MSSQLLocalDB`).

### 2. Create and start the dev instance

The project uses a custom instance named `Dcc-Dev`:

```bash
sqllocaldb create Dcc-Dev
sqllocaldb start Dcc-Dev
```

### 3. Verify the instance is running

```bash
sqllocaldb info Dcc-Dev
```

Look for `State: Running` in the output.

### 4. Run EF Core migrations

From the solution root:

```bash
dotnet ef database update --project MetalMetrics.Infrastructure --startup-project MetalMetrics.Web
```

This creates the database and applies all migrations.

### 5. Run the app

Seed data runs automatically on startup in the **Development** environment. The first launch populates demo tenants, users, jobs, and employees.

## Configuration

Connection strings are already set in the project files:

**appsettings.Development.json** (used in Development):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\Dcc-Dev;Database=MetalMetrics_Dev;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

**appsettings.json** (production/default):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\Dcc-Dev;Database=MetalMetrics;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

No User Secrets needed for the database — Trusted Connection uses Windows auth.

## Verification

Connect to the database using SQL Server Object Explorer in Visual Studio:

1. View → SQL Server Object Explorer
2. Expand **SQL Server → (localdb)\Dcc-Dev → Databases**
3. You should see `MetalMetrics_Dev` with tables like `Jobs`, `Employees`, `Tenants`, etc.

Or query from the command line:

```bash
sqlcmd -S "(localdb)\Dcc-Dev" -d MetalMetrics_Dev -Q "SELECT COUNT(*) FROM Jobs"
```

## Troubleshooting

### "SQL Server instance not found"
```bash
sqllocaldb create Dcc-Dev
sqllocaldb start Dcc-Dev
```

### "Cannot open database ... Login failed"
The database hasn't been created yet. Run migrations:
```bash
dotnet ef database update --project MetalMetrics.Infrastructure --startup-project MetalMetrics.Web
```

### "A connection was successfully established with the server, but then an error occurred... certificate"
Add `TrustServerCertificate=True` to the connection string if you hit SSL/TLS certificate errors in local dev. The project connection strings already use `Trusted_Connection=True`.

### "The EF Core tools are not installed"
```bash
dotnet tool install --global dotnet-ef
```

### Seed data not appearing
Seed data only runs when `ASPNETCORE_ENVIRONMENT=Development`. Verify the environment is set in `launchSettings.json` or your IDE run configuration.
