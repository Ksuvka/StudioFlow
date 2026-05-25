FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем всё содержимое репозитория
COPY . .

# Переходим в папку, где лежит .csproj
WORKDIR /src/StudioFlow

# Восстанавливаем зависимости
RUN dotnet restore

# Публикуем проект
RUN dotnet publish -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StudioFlow.dll"]