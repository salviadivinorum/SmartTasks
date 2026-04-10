# 1. Build stage: Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["SmartTasks.Web/SmartTasks.Web.csproj", "SmartTasks.Web/"]
RUN dotnet restore "SmartTasks.Web/SmartTasks.Web.csproj"

# Copy the entire source code and publish the application
COPY . .
WORKDIR "/src/SmartTasks.Web"
RUN dotnet publish "SmartTasks.Web.csproj" -c Release -o /app/publish

# 2. Runtime stage
# we will use nginx because Blazor WASM is just static files
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY SmartTasks.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80