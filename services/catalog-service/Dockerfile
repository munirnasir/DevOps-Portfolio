# syntax=docker/dockerfile:1

# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Pos.Catalog.Api.csproj ./
RUN dotnet restore Pos.Catalog.Api.csproj

COPY . ./
RUN dotnet publish Pos.Catalog.Api.csproj -c Release -o /app --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Run as the non-root user that ships with the aspnet image.
USER $APP_UID

COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Pos.Catalog.Api.dll"]
