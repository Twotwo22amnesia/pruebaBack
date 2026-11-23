# Imagen multi-etapa: se compila con el SDK y se ejecuta solo con el runtime de ASP.NET,
# así la imagen final es más liviana y no incluye compiladores ni código fuente.

# ---------- build: restaura y copia el código ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj: mientras no cambien las dependencias, Docker reutiliza
# la capa del restore y las reconstrucciones son mucho más rápidas.
COPY MarcasAutos.sln ./
COPY src/MarcasAutos.Api/MarcasAutos.Api.csproj src/MarcasAutos.Api/
COPY tests/MarcasAutos.Api.Tests/MarcasAutos.Api.Tests.csproj tests/MarcasAutos.Api.Tests/
RUN dotnet restore MarcasAutos.sln

COPY src/ src/
COPY tests/ tests/
COPY coverage.runsettings ./

# ---------- test: ejecuta las pruebas con cobertura (servicio "tests" del compose) ----------
FROM build AS test
ENTRYPOINT ["dotnet", "test", "MarcasAutos.sln", "--no-restore", \
            "--settings", "coverage.runsettings", \
            "--collect", "XPlat Code Coverage", \
            "--results-directory", "/testresults"]

# ---------- publish ----------
FROM build AS publish
RUN dotnet publish src/MarcasAutos.Api/MarcasAutos.Api.csproj -c Release -o /app/publish --no-restore

# ---------- final: imagen de ejecución de la API ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Usuario sin privilegios que ya trae la imagen oficial.
USER $APP_UID

# Desde .NET 8 las imágenes oficiales escuchan en el puerto 8080 por defecto.
EXPOSE 8080
ENTRYPOINT ["dotnet", "MarcasAutos.Api.dll"]
