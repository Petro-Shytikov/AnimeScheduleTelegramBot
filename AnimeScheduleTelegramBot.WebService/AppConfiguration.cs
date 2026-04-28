using System.ComponentModel.DataAnnotations;

public sealed class AppConfiguration : IAppConfiguration, IValidatableObject
{
	public AppConfiguration(
		string telegramBotToken,
		string telegramPublicWebhookUrl,
		string telegramWebhookSecretToken,
		TimeSpan retryTelegramWebhookInitializerDelay,
		string kitsuBaseUrl,
		int kitsuMaxRetries,
		TimeSpan kitsuRetryDelay,
		TimeSpan kitsuMinRequestInterval,
		TimeSpan animeCacheLifetime)
    {
        TelegramBotToken = telegramBotToken;
        TelegramPublicWebhookUrl = telegramPublicWebhookUrl;
        TelegramWebhookSecretToken = telegramWebhookSecretToken;
		RetryTelegramWebhookInitializerDelay = retryTelegramWebhookInitializerDelay;
		KitsuBaseUrl = kitsuBaseUrl;
		KitsuMaxRetries = kitsuMaxRetries;
		KitsuRetryDelay = kitsuRetryDelay;
		KitsuMinRequestInterval = kitsuMinRequestInterval;
		AnimeCacheLifetime = animeCacheLifetime;
    }

	[Required]
	public string TelegramBotToken { get;}

    [Required]
	public string TelegramPublicWebhookUrl { get; }

	[Required]
	public string TelegramWebhookSecretToken { get; }

	[Required]
	public TimeSpan RetryTelegramWebhookInitializerDelay { get; }

	[Required]
	public string KitsuBaseUrl { get; }

	[Required]
	public int KitsuMaxRetries { get; }

	[Required]
	public TimeSpan KitsuRetryDelay { get; }

	[Required]
	public TimeSpan KitsuMinRequestInterval { get; }

	[Required]
	public TimeSpan AnimeCacheLifetime { get; }


	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (string.IsNullOrWhiteSpace(TelegramBotToken))
		{
			yield return CreateValidationResult(
				$"{nameof(TelegramBotToken)} must not be empty or whitespace.",
				nameof(TelegramBotToken));
		}

		if (string.IsNullOrWhiteSpace(TelegramPublicWebhookUrl))
		{
			yield return CreateValidationResult(
				$"{nameof(TelegramPublicWebhookUrl)} must not be empty or whitespace.",
				nameof(TelegramPublicWebhookUrl));
		}

		if (string.IsNullOrWhiteSpace(TelegramWebhookSecretToken))
		{
			yield return CreateValidationResult(
				$"{nameof(TelegramWebhookSecretToken)} must not be empty or whitespace.",
				nameof(TelegramWebhookSecretToken));
		}

		if (RetryTelegramWebhookInitializerDelay <= TimeSpan.Zero)
		{
			yield return CreateValidationResult(
				$"{nameof(RetryTelegramWebhookInitializerDelay)} must be greater than zero.",
				nameof(RetryTelegramWebhookInitializerDelay));
		}

		if (string.IsNullOrWhiteSpace(KitsuBaseUrl))
		{
			yield return CreateValidationResult(
				$"{nameof(KitsuBaseUrl)} must not be empty or whitespace.",
				nameof(KitsuBaseUrl));
		}
		else if (!Uri.TryCreate(KitsuBaseUrl, UriKind.Absolute, out _))
		{
			yield return CreateValidationResult(
				$"{nameof(KitsuBaseUrl)} must be an absolute URI.",
				nameof(KitsuBaseUrl));
		}

		if (KitsuMaxRetries <= 0)
		{
			yield return CreateValidationResult(
				$"{nameof(KitsuMaxRetries)} must be greater than zero.",
				nameof(KitsuMaxRetries));
		}

		if (KitsuRetryDelay <= TimeSpan.Zero)
		{
			yield return CreateValidationResult(
				$"{nameof(KitsuRetryDelay)} must be greater than zero.",
				nameof(KitsuRetryDelay));
		}

		if (KitsuMinRequestInterval <= TimeSpan.Zero)
		{
			yield return CreateValidationResult(
				$"{nameof(KitsuMinRequestInterval)} must be greater than zero.",
				nameof(KitsuMinRequestInterval));
		}

		if (AnimeCacheLifetime <= TimeSpan.Zero)
		{
			yield return CreateValidationResult(
				$"{nameof(AnimeCacheLifetime)} must be greater than zero.",
				nameof(AnimeCacheLifetime));
		}
	}

	private static ValidationResult CreateValidationResult(string errorMessage, string memberName) =>
		new(errorMessage, [memberName]);
}
