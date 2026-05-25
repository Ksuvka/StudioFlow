FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем все файлы репозитория
COPY . .

# Восстанавливаем зависимости (используем .csproj в корне)
RUN dotnet restore "StudioFlow.csproj"

# Публикуем проект
RUN dotnet publish "StudioFlow.csproj" -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Настройка порта для Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StudioFlow.dll"]