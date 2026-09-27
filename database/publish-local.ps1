# Applies database/schema.sql to a local PostgreSQL instance.
# Regenerate schema.sql first if you've added new EF Core migrations (see README.md).
param(
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [string]$Database = "receply",
    [string]$Username = "receply"
)

$env:PGPASSWORD = Read-Host -Prompt "Postgres password for $Username@$HostName" -AsSecureString |
    ForEach-Object { [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($_)) }

psql -h $HostName -p $Port -U $Username -d $Database -f "$PSScriptRoot\schema.sql"
