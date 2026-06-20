# syntax=docker/dockerfile:1

# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Pos.Identity.Api.csproj ./
RUN dotnet restore Pos.Identity.Api.csproj

COPY . ./
RUN dotnet publish Pos.Identity.Api.csproj -c Release -o /app --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

USER $APP_UID

COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Pos.Identity.Api.dll"]
