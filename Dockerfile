# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos del proyecto y restaurar dependencias
COPY *.csproj .
RUN dotnet restore

# Copiar todo el código y compilar
COPY . .
RUN dotnet publish -c Release -o /app

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Exponer el puerto 8080
EXPOSE 8080

# Configurar la URL
ENV ASPNETCORE_URLS=http://*:8080

# Comando de inicio
ENTRYPOINT ["dotnet", "LaLigaTrackerBackend.dll"]