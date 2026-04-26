using AnimeScheduleTelegramBot.WebService.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddAppConfiguration();
builder.Services.AddSingleton<ValidateTelegramSecretFilter>();
builder.Services.AddTelegramBotClient();
builder.Services.AddTelegramWebhookInitialization();

var app = builder.Build();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
