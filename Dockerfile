# ============================================================
#  APIGasoline - Imagen de la API (.NET 10)
#  Build:  docker build -t apigasoline-api .
#  Escucha en el puerto 8080 dentro del contenedor.
# ============================================================

# ---------- Etapa 1: compilar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj para aprovechar la caché del "restore"
COPY APIGasoline/APIGasoline.csproj                               APIGasoline/
COPY APIGasoline.Application/APIGasoline.Application.csproj       APIGasoline.Application/
COPY APIGasoline.Domain/APIGasoline.Domain.csproj                 APIGasoline.Domain/
COPY APIGasoline.Infrastructure/APIGasoline.Infrastructure.csproj APIGasoline.Infrastructure/
RUN dotnet restore APIGasoline/APIGasoline.csproj

# Ahora el código y publicar
COPY APIGasoline/                APIGasoline/
COPY APIGasoline.Application/    APIGasoline.Application/
COPY APIGasoline.Domain/         APIGasoline.Domain/
COPY APIGasoline.Infrastructure/ APIGasoline.Infrastructure/
RUN dotnet publish APIGasoline/APIGasoline.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Etapa 2: ejecutar ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    TZ=America/Mexico_City

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "APIGasoline.dll"]
