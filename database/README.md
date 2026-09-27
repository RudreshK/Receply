# Receply database schema (reference + local publish)

This folder is a plain SQL reference for the schema that `Receply.Infrastructure`'s EF Core
migrations produce — not a separate source of truth. The migrations under
[`src/Receply.Infrastructure/Persistence/Migrations`](../src/Receply.Infrastructure/Persistence/Migrations)
are still what actually runs in every environment (`Program.cs` calls `db.Database.MigrateAsync()`
on startup). `schema.sql` here is a generated, idempotent snapshot of that same schema in raw SQL,
so you can browse table/index/foreign-key definitions without reading C# Fluent API configuration
classes.

## Files

- **schema.sql** — full `CREATE TABLE` / `CREATE INDEX` / `FOREIGN KEY` script, generated from all
  migrations up to the latest. Wrapped in `IF NOT EXISTS` guards (via `--idempotent`), so re-running
  it against a database that already has some or all of the schema is safe — it only applies what's
  missing.
- **publish-local.ps1** — applies `schema.sql` to a local PostgreSQL instance via `psql`.

## Regenerating schema.sql after a new migration

Whenever you add an EF Core migration, regenerate this file so it stays in sync:

```bash
dotnet tool run dotnet-ef migrations script --idempotent \
  --project src/Receply.Infrastructure \
  --startup-project src/Receply.Api \
  --output database/schema.sql
```

(`dotnet-ef` is a local tool — restore it once per clone with `dotnet tool restore`.)

## Publishing to a local database

Requires `psql` (the PostgreSQL command-line client) on your PATH, and a local Postgres server
with an empty `receply` database already created.

```powershell
.\database\publish-local.ps1
```

It prompts for the password and defaults to `localhost:5432`, database `receply`, user `receply` —
override with `-HostName`, `-Port`, `-Database`, `-Username` if yours differ.

This is an alternative to `dotnet ef database update` (which does the same thing programmatically
and is what actually runs in every deployed environment) — use whichever is more convenient;
they produce the same schema.
