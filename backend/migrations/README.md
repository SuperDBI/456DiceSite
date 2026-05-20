# Migrations

This folder contains EF Core migrations for the backend project. To create a new migration, use the following command:

```bash
dotnet ef migrations add MigrationName --project backend --startup-project backend
```

To apply migrations to the database, use:

```bash
dotnet ef database update --project backend --startup-project backend
```