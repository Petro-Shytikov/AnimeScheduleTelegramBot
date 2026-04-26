public interface IAppConfiguration
{
	string TelegramBotToken { get; }
	string TelegramPublicWebhookUrl { get; }
	string TelegramWebhookSecretToken { get; }
	TimeSpan RetryTelegramWebhookInitializerDelay { get; }
}
