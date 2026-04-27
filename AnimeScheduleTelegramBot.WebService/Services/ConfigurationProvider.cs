using System.ComponentModel.DataAnnotations;

namespace AnimeScheduleTelegramBot.WebService.Services;

public sealed class AppConfigurationProvider : IAppConfigurationProvider
{
	private readonly IConfiguration _configuration;

	public AppConfigurationProvider(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public IAppConfiguration Create()
	{
		var appConfiguration = new AppConfiguration(
			telegramBotToken: GetRequiredEnvironmentVariable("TELEGRAM_BOT_TOKEN"),
			telegramPublicWebhookUrl: GetRequiredEnvironmentVariable("TELEGRAM_PUBLIC_WEBHOOK_URL"),
			telegramWebhookSecretToken: GetRequiredEnvironmentVariable("TELEGRAM_WEBHOOK_SECRET_TOKEN"),
			retryTelegramWebhookInitializerDelay: GetRequiredConfigurationValue<TimeSpan>(_configuration, "BotSettings:RetryTelegramWebhookInitializerDelay"),
			kitsuBaseUrl: GetRequiredConfigurationValue<string>(_configuration, "KitsuSettings:BaseUrl"),
			kitsuMaxRetries: GetRequiredConfigurationValue<int>(_configuration, "KitsuSettings:MaxRetries"),
			kitsuRetryDelay: GetRequiredConfigurationValue<TimeSpan>(_configuration, "KitsuSettings:RetryDelay"),
			kitsuMinRequestInterval: GetRequiredConfigurationValue<TimeSpan>(_configuration, "KitsuSettings:MinRequestInterval")
		);

		Validate(appConfiguration);

		return appConfiguration;
	}

	private static T GetRequiredConfigurationValue<T>(IConfiguration configuration, string key) =>
		configuration.GetValue<T>(key) ?? throw new NullReferenceException($"Configuration value {key} is required.");

	private static string GetRequiredEnvironmentVariable(string key) =>
		Environment.GetEnvironmentVariable(key) ?? throw new NullReferenceException($"Environment variable {key} is required.");

	private static void Validate(IAppConfiguration appConfiguration) =>
		Validator.ValidateObject(appConfiguration, new ValidationContext(appConfiguration), validateAllProperties: true);
}
