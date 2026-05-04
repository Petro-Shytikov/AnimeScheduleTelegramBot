# AnimeScheduleTelegramBot

ASP.NET web service for a Telegram bot that returns anime information.

Current implemented commands:
- `/info` - returns service version.
- `/ongoings` - returns current season ongoing anime titles.
- `/week` - returns current week episode schedule as separate messages per weekday (UTC), skipping days without episodes.
- `/yesterday` - returns yesterday episode schedule (UTC).
- `/today` - returns today episode schedule (UTC).
- `/tomorrow` - returns tomorrow episode schedule (UTC).

## Tech Stack

- .NET `net10.0`
- ASP.NET Core Web API
- Telegram.Bot
- HttpClientFactory + delegating handlers
- Polly retry policy
- `IMemoryCache` for in-process anime caching

## HTTP Pipeline

Kitsu HTTP client is configured via `IHttpClientFactory` with:

- `RateLimitedHandler`: client-side request throttling using `RateLimiter`.
- `RetryPolicyHandler`: Polly retries for transient failures and `429` responses with exponential backoff.

The typed client for `IKitsuHttpProvider` sets:

- `BaseAddress` from `KitsuSettings:BaseUrl`.
- `Accept: application/vnd.api+json`.

## Schedule Behavior

- `/week` sends one message per weekday in Monday-Sunday order.
- Weekday messages are sent only for days that actually contain episodes.
- `/yesterday`, `/today`, and `/tomorrow` each send a single day-specific message.
- If no episodes are found for a requested period/day, the bot sends an explicit no-data message.

## Configuration

### Environment Variables (required)

- `TELEGRAM_BOT_TOKEN`
- `TELEGRAM_PUBLIC_WEBHOOK_URL`
- `TELEGRAM_WEBHOOK_SECRET_TOKEN`

### appsettings.json

`BotSettings`:
- `RetryTelegramWebhookInitializerDelay` (TimeSpan)

`KitsuSettings`:
- `BaseUrl` (absolute URL)
- `MaxRetries` (int > 0)
- `RetryDelay` (TimeSpan > 0)
- `MinRequestInterval` (TimeSpan > 0)

`AnimeSettings`:
- `CacheLifetime` (TimeSpan > 0, default `1.00:00:00` — 24 hours)

`appsettings.Development.json` can override any of the above for local/mock development.

## API Endpoints

- `GET /` - simple health-like text response (`Hello World!`).
- `GET /health` - ASP.NET health checks endpoint.
- `POST /telegram/webhook` - Telegram webhook endpoint with secret-token validation.

## Run Locally

1. Set required environment variables.
2. Update `AnimeScheduleTelegramBot.WebService/appsettings.json` if needed.
3. Run:

```bash
dotnet run --project AnimeScheduleTelegramBot.WebService/AnimeScheduleTelegramBot.WebService.csproj
```

## Build

```bash
dotnet build AnimeScheduleTelegramBot.WebService/AnimeScheduleTelegramBot.WebService.csproj
```

## Unit Tests

Run all tests:

```bash
dotnet test
```

Run tests exactly as CI does:

```bash
dotnet restore
dotnet test --solution AnimeScheduleTelegramBot.slnx --no-restore --configuration Release
```

Test project location:
- `AnimeScheduleTelegramBot.Tests`


## Docker

Build image:

```bash
docker build -t anime-schedule-telegram-bot .
```

Run container:

```bash
docker run --rm -p 8080:8080 \
	-e TELEGRAM_BOT_TOKEN="<your_bot_token>" \
	-e TELEGRAM_PUBLIC_WEBHOOK_URL="<your_public_webhook_base_url>" \
	-e TELEGRAM_WEBHOOK_SECRET_TOKEN="<your_webhook_secret_token>" \
	anime-schedule-telegram-bot
```

## Deploy To Azure Container Registry (ACR)

Workflow file:
- `.github/workflows/deploy-acr.yml`

The workflow is triggered on pushed tags:
- `v*`
- `deploy`

Deployment runs only when both tags exist on the same commit:
- a semantic version tag, for example `v1.2.3`
- the `deploy` tag

Required GitHub repository secrets:
- `ACR_LOGIN_SERVER` (example: `myregistry.azurecr.io`)
- `ACR_USERNAME`
- `ACR_PASSWORD`
- `ACR_REPOSITORY` (example: `anime-schedule-telegram-bot`)

Published image tags:
- `<version with v>` (example: `v1.2.3`)
- `<version without v>` (example: `1.2.3`)
- `latest`
