public interface IAppConfiguration
{
	string TelegramBotToken { get; }
	string TelegramPublicWebhookUrl { get; }
	string TelegramWebhookSecretToken { get; }
	TimeSpan RetryTelegramWebhookInitializerDelay { get; }
	string KitsuBaseUrl { get; }
	int KitsuMaxRetries { get; }
	TimeSpan KitsuRetryDelay { get; }
	TimeSpan KitsuMinRequestInterval { get; }
	TimeSpan AnimeCacheLifetime { get; }
}
