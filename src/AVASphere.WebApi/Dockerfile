FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore src/AVASphere.WebApi/AVASphere.WebApi.csproj
RUN dotnet publish src/AVASphere.WebApi/AVASphere.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Variables de entorno configurables (se pueden override)
ENV ASPNETCORE_ENVIRONMENT=Development

# Database Configuration
ENV ASPNETCORE_DbSettings__ConnectionString="Host=localhost;Port=5432;Database=default;Username=default;Password=default"
ENV ASPNETCORE_DbSettings__DatabaseName="default"

# JWT Configuration
ENV ASPNETCORE_JwtSettings__Key="default_jwt_key"
ENV ASPNETCORE_JwtSettings__Issuer="default_issuer"
ENV ASPNETCORE_JwtSettings__Audience="default_audience"
ENV ASPNETCORE_JwtSettings__ExpirationInMinutes="60"

# MinIO Configuration
ENV ASPNETCORE_MinIO__Endpoint="localhost"
ENV ASPNETCORE_MinIO__AccessKey="default_access"
ENV ASPNETCORE_MinIO__SecretKey="default_secret"
ENV ASPNETCORE_MinIO__BucketName="default_bucket"
ENV ASPNETCORE_MinIO__UseSSL="false"

# Direct Environment Variables
ENV POSTGRES_CONNECTION_STRING="Host=localhost;Port=5432;Database=default;Username=default;Password=default"
ENV JWT_KEY="default_jwt_key"
ENV JWT_ISSUER="default_issuer"
ENV JWT_AUDIENCE="default_audience"
ENV JWT_EXPIRATION_MINUTES="60"

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AVASphere.WebApi.dll"]
