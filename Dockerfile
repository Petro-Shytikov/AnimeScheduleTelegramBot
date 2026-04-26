FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /AnimeScheduleTelegramBot.WebService

COPY ["AnimeScheduleTelegramBot.WebService/AnimeScheduleTelegramBot.WebService.csproj", "AnimeScheduleTelegramBot.WebService/"]
RUN dotnet restore "AnimeScheduleTelegramBot.WebService/AnimeScheduleTelegramBot.WebService.csproj"

COPY . .
RUN dotnet publish "AnimeScheduleTelegramBot.WebService/AnimeScheduleTelegramBot.WebService.csproj" -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AnimeScheduleTelegramBot.WebService.dll"]
