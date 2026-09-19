# Build context is the repository root (see azure.yaml) - Workers pulls in Domain/Application/
# Infrastructure via project references, so the whole src/ tree needs to be present to restore.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Receply.Domain/Receply.Domain.csproj src/Receply.Domain/
COPY src/Receply.Application/Receply.Application.csproj src/Receply.Application/
COPY src/Receply.Infrastructure/Receply.Infrastructure.csproj src/Receply.Infrastructure/
COPY src/Receply.Workers/Receply.Workers.csproj src/Receply.Workers/
RUN dotnet restore src/Receply.Workers/Receply.Workers.csproj

COPY src/ src/
RUN dotnet publish src/Receply.Workers/Receply.Workers.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# ASP.NET Core here only backs the /healthz endpoint App Service pings to confirm startup -
# the real work is the Quartz-hosted background service (see Program.cs).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Receply.Workers.dll"]
