# 1. Fáze: Sestavení (Build)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopírování souborů projektu a restaurování
COPY ["SmartTasks.Web/SmartTasks.Web.csproj", "SmartTasks.Web/"]
RUN dotnet restore "SmartTasks.Web/SmartTasks.Web.csproj"

# Kopírování zbytku a publikace
COPY . .
WORKDIR "/src/SmartTasks.Web"
RUN dotnet publish "SmartTasks.Web.csproj" -c Release -o /app/publish

# 2. Fáze: Spuštění (Runtime)
# Použijeme nginx, protože Blazor WASM jsou jen statické soubory
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY SmartTasks.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80