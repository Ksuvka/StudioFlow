FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "StudioFlow.csproj"

# Ограничиваем память при публикации
RUN dotnet publish "StudioFlow.csproj" -c Release -o /app/publish \
    /p:UseSharedCompilation=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_GCHeapHardLimit=400000000
ENV DOTNET_GCConserveMemory=9

EXPOSE 8080

ENTRYPOINT ["dotnet", "StudioFlow.dll"]