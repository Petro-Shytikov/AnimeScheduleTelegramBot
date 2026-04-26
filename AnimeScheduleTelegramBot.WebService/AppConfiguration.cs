using System.ComponentModel.DataAnnotations;

public sealed class AppConfiguration : IAppConfiguration, IValidatableObject
{
	public AppConfiguration(
		string telegramBotToken,
		string telegramPublicWebhookUrl,
		string telegramWebhookSecretToken,
		TimeSpan retryTelegramWebhookInitializerDelay)
    {
        TelegramBotToken = telegramBotToken;
        TelegramPublicWebhookUrl = telegramPublicWebhookUrl;
        TelegramWebhookSecretToken = telegramWebhookSecretToken;
		RetryTelegramWebhookInitializerDelay = retryTelegramWebhookInitializerDelay;
    }

	[Required]
	public string TelegramBotToken { get;}

    [Required]
	public string TelegramPublicWebhookUrl { get; }

	[Required]
	public string TelegramWebhookSecretToken { get; }

	[Required]
	public TimeSpan RetryTelegramWebhookInitializerDelay { get; }


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
	}

	private static ValidationResult CreateValidationResult(string errorMessage, string memberName) =>
		new(errorMessage, [memberName]);
}
