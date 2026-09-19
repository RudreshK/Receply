# Build context is the repository root (see azure.yaml) - Api pulls in Domain/Application/
# Infrastructure via project references, so the whole src/ tree needs to be present to restore.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Receply.Domain/Receply.Domain.csproj src/Receply.Domain/
COPY src/Receply.Application/Receply.Application.csproj src/Receply.Application/
COPY src/Receply.Infrastructure/Receply.Infrastructure.csproj src/Receply.Infrastructure/
COPY src/Receply.Api/Receply.Api.csproj src/Receply.Api/
RUN dotnet restore src/Receply.Api/Receply.Api.csproj

COPY src/ src/
RUN dotnet publish src/Receply.Api/Receply.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Receply.Api.dll"]
