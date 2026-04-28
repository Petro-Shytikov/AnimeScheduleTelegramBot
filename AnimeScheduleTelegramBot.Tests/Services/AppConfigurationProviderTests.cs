using System.ComponentModel.DataAnnotations;
using AnimeScheduleTelegramBot.WebService.Services;
using Microsoft.Extensions.Configuration;

namespace AnimeScheduleTelegramBot.Tests.Services;

[NotInParallel]
public class AppConfigurationProviderTests
{
	private const string ValidTelegramBotToken = "123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefgh";
	private const string ValidTelegramPublicWebhookUrl = "https://example.com/webhook";
	private const string ValidTelegramWebhookSecretToken = "secret-token-123";
	private const string ValidKitsuBaseUrl = "https://kitsu.io/api/edge";

	[Before(Test)]
	public void Setup()
	{
		ClearEnvironmentVariables();
	}

	[After(Test)]
	public void Teardown()
	{
		ClearEnvironmentVariables();
	}

	[Test]
	public async Task Create_WithValidEnvironmentVariablesAndConfig_ReturnsValidAppConfiguration()
	{
		SetRequiredEnvironmentVariables();

		var configuration = CreateConfiguration(GetValidConfigurationValues());
		var provider = new AppConfigurationProvider(configuration);

		var result = provider.Create();

		await Assert.That(result).IsNotNull();
		await Assert.That(result.TelegramBotToken).IsEqualTo(ValidTelegramBotToken);
		await Assert.That(result.TelegramPublicWebhookUrl).IsEqualTo(ValidTelegramPublicWebhookUrl);
		await Assert.That(result.TelegramWebhookSecretToken).IsEqualTo(ValidTelegramWebhookSecretToken);
		await Assert.That(result.KitsuBaseUrl).IsEqualTo(ValidKitsuBaseUrl);
		await Assert.That(result.KitsuMaxRetries).IsEqualTo(3);
		await Assert.That(result.AnimeCacheLifetime).IsEqualTo(TimeSpan.FromHours(24));
	}

	[Test]
	[NotInParallel]
	public async Task Create_WithMissingTelegramBotTokenEnvironmentVariable_ThrowsNullReferenceException()
	{
		Environment.SetEnvironmentVariable("TELEGRAM_BOT_TOKEN", null);
		Environment.SetEnvironmentVariable("TELEGRAM_PUBLIC_WEBHOOK_URL", ValidTelegramPublicWebhookUrl);
		Environment.SetEnvironmentVariable("TELEGRAM_WEBHOOK_SECRET_TOKEN", ValidTelegramWebhookSecretToken);

		var configuration = CreateConfiguration(GetValidConfigurationValues());
		var provider = new AppConfigurationProvider(configuration);

		await Assert.ThrowsAsync<NullReferenceException>(async () => provider.Create());
	}

	[Test]
	public async Task Create_WithMissingAnimeCacheLifetimeConfigurationValue_ThrowsValidationException()
	{
		SetRequiredEnvironmentVariables();

		var configuration = CreateConfiguration(new Dictionary<string, string?>
		{
			{ "BotSettings:RetryTelegramWebhookInitializerDelay", "00:00:30" },
			{ "KitsuSettings:BaseUrl", ValidKitsuBaseUrl },
			{ "KitsuSettings:MaxRetries", "3" },
			{ "KitsuSettings:RetryDelay", "00:00:30" },
			{ "KitsuSettings:MinRequestInterval", "00:00:01" }
		});

		var provider = new AppConfigurationProvider(configuration);

		// When a value is missing, IConfiguration returns the default (TimeSpan.Zero for TimeSpan)
		// which then fails validation
		await Assert.ThrowsAsync<ValidationException>(async () => provider.Create());
	}

	[Test]
	public async Task Create_WithInvalidAnimeCacheLifetimeValidation_ThrowsValidationException()
	{
		SetRequiredEnvironmentVariables();

		var configuration = CreateConfiguration(new Dictionary<string, string?>
		{
			{ "BotSettings:RetryTelegramWebhookInitializerDelay", "00:00:30" },
			{ "KitsuSettings:BaseUrl", ValidKitsuBaseUrl },
			{ "KitsuSettings:MaxRetries", "3" },
			{ "KitsuSettings:RetryDelay", "00:00:30" },
			{ "KitsuSettings:MinRequestInterval", "00:00:01" },
			{ "AnimeSettings:CacheLifetime", "00:00:00" }
		});

		var provider = new AppConfigurationProvider(configuration);

		await Assert.ThrowsAsync<ValidationException>(async () => provider.Create());
	}

	[Test]
	[Arguments("BotSettings:RetryTelegramWebhookInitializerDelay", "00:00:00")]
	[Arguments("BotSettings:RetryTelegramWebhookInitializerDelay", "-00:00:01")]
	[Arguments("KitsuSettings:RetryDelay", "00:00:00")]
	[Arguments("KitsuSettings:RetryDelay", "-00:00:01")]
	[Arguments("KitsuSettings:MinRequestInterval", "00:00:00")]
	[Arguments("KitsuSettings:MinRequestInterval", "-00:00:01")]
	[Arguments("AnimeSettings:CacheLifetime", "00:00:00")]
	[Arguments("AnimeSettings:CacheLifetime", "-00:00:01")]
	public async Task Create_WithZeroOrNegativeTimeSpanConfiguration_ThrowsValidationException(string key, string invalidValue)
	{
		SetRequiredEnvironmentVariables();

		var values = GetValidConfigurationValues();
		values[key] = invalidValue;

		var configuration = CreateConfiguration(values);
		var provider = new AppConfigurationProvider(configuration);

		await Assert.ThrowsAsync<ValidationException>(async () => provider.Create());
	}

	[Test]
	[Arguments("KitsuSettings:MaxRetries", "not-a-number")]
	[Arguments("BotSettings:RetryTelegramWebhookInitializerDelay", "not-a-timespan")]
	[Arguments("KitsuSettings:RetryDelay", "still-not-a-timespan")]
	[Arguments("KitsuSettings:MinRequestInterval", "invalid")]
	[Arguments("AnimeSettings:CacheLifetime", "abc")]
	public async Task Create_WithInvalidConfigurationFormat_ThrowsInvalidOperationException(string key, string invalidValue)
	{
		SetRequiredEnvironmentVariables();

		var values = GetValidConfigurationValues();
		values[key] = invalidValue;

		var configuration = CreateConfiguration(values);
		var provider = new AppConfigurationProvider(configuration);

		await Assert.ThrowsAsync<InvalidOperationException>(async () => provider.Create());
	}

	[Test]
	[Arguments("KitsuSettings:MaxRetries", "")]
	[Arguments("KitsuSettings:MaxRetries", " ")]
	[Arguments("BotSettings:RetryTelegramWebhookInitializerDelay", "")]
	[Arguments("BotSettings:RetryTelegramWebhookInitializerDelay", " ")]
	[Arguments("KitsuSettings:RetryDelay", "")]
	[Arguments("KitsuSettings:RetryDelay", " ")]
	[Arguments("KitsuSettings:MinRequestInterval", "")]
	[Arguments("KitsuSettings:MinRequestInterval", " ")]
	[Arguments("AnimeSettings:CacheLifetime", "")]
	[Arguments("AnimeSettings:CacheLifetime", " ")]
	public async Task Create_WithEmptyOrBlankConfigurationValue_ThrowsInvalidOperationException(string key, string invalidValue)
	{
		SetRequiredEnvironmentVariables();

		var values = GetValidConfigurationValues();
		values[key] = invalidValue;

		var configuration = CreateConfiguration(values);
		var provider = new AppConfigurationProvider(configuration);

		await Assert.ThrowsAsync<InvalidOperationException>(async () => provider.Create());
	}

	[Test]
	[Arguments("KitsuSettings:BaseUrl", "")]
	[Arguments("KitsuSettings:BaseUrl", " ")]
	[Arguments("KitsuSettings:BaseUrl", "not-a-url")]
	public async Task Create_WithEmptyBlankOrInvalidKitsuBaseUrl_ThrowsValidationException(string key, string invalidValue)
	{
		SetRequiredEnvironmentVariables();

		var values = GetValidConfigurationValues();
		values[key] = invalidValue;

		var configuration = CreateConfiguration(values);
		var provider = new AppConfigurationProvider(configuration);

		await Assert.ThrowsAsync<ValidationException>(async () => provider.Create());
	}

	[Test]
	[Arguments("TELEGRAM_BOT_TOKEN")]
	[Arguments("TELEGRAM_PUBLIC_WEBHOOK_URL")]
	[Arguments("TELEGRAM_WEBHOOK_SECRET_TOKEN")]
	public async Task Create_WithEmptyEnvironmentVariable_ThrowsValidationException(string envKey)
	{
		SetRequiredEnvironmentVariables();
		Environment.SetEnvironmentVariable(envKey, string.Empty);

		var configuration = CreateConfiguration(GetValidConfigurationValues());
		var provider = new AppConfigurationProvider(configuration);

		await Assert.ThrowsAsync<ValidationException>(async () => provider.Create());
	}

	[Test]
	[Arguments("TELEGRAM_BOT_TOKEN")]
	[Arguments("TELEGRAM_PUBLIC_WEBHOOK_URL")]
	[Arguments("TELEGRAM_WEBHOOK_SECRET_TOKEN")]
	public async Task Create_WithBlankEnvironmentVariable_ThrowsValidationException(string envKey)
	{
		SetRequiredEnvironmentVariables();
		Environment.SetEnvironmentVariable(envKey, " ");

		var configuration = CreateConfiguration(GetValidConfigurationValues());
		var provider = new AppConfigurationProvider(configuration);

		await Assert.ThrowsAsync<ValidationException>(async () => provider.Create());
	}

	[Test]
	[Arguments(0)]
	[Arguments(-1)]
	public async Task Create_WithInvalidKitsuMaxRetriesValidation_ThrowsValidationException(int invalidMaxRetries)
	{
		SetRequiredEnvironmentVariables();

		var configuration = CreateConfiguration(new Dictionary<string, string?>
		{
			{ "BotSettings:RetryTelegramWebhookInitializerDelay", "00:00:30" },
			{ "KitsuSettings:BaseUrl", ValidKitsuBaseUrl },
			{ "KitsuSettings:MaxRetries", invalidMaxRetries.ToString() },
			{ "KitsuSettings:RetryDelay", "00:00:30" },
			{ "KitsuSettings:MinRequestInterval", "00:00:01" },
			{ "AnimeSettings:CacheLifetime", "1.00:00:00" }
		});

		var provider = new AppConfigurationProvider(configuration);

		await Assert.ThrowsAsync<ValidationException>(async () => provider.Create());
	}

	[Test]
	public async Task Create_WithAllRequiredValuesPopulated_SuccessfullyCreatesConfiguration()
	{
		SetRequiredEnvironmentVariables();

		var configuration = CreateConfiguration(new Dictionary<string, string?>
		{
			{ "BotSettings:RetryTelegramWebhookInitializerDelay", "00:00:45" },
			{ "KitsuSettings:BaseUrl", ValidKitsuBaseUrl },
			{ "KitsuSettings:MaxRetries", "5" },
			{ "KitsuSettings:RetryDelay", "00:01:00" },
			{ "KitsuSettings:MinRequestInterval", "00:00:02" },
			{ "AnimeSettings:CacheLifetime", "2.12:30:45" }
		});

		var provider = new AppConfigurationProvider(configuration);

		var result = provider.Create();

		await Assert.That(result.RetryTelegramWebhookInitializerDelay).IsEqualTo(TimeSpan.FromSeconds(45));
		await Assert.That(result.KitsuMaxRetries).IsEqualTo(5);
		await Assert.That(result.KitsuRetryDelay).IsEqualTo(TimeSpan.FromMinutes(1));
		await Assert.That(result.KitsuMinRequestInterval).IsEqualTo(TimeSpan.FromSeconds(2));
		await Assert.That(result.AnimeCacheLifetime).IsEqualTo(TimeSpan.Parse("2.12:30:45"));
	}

	private static void SetRequiredEnvironmentVariables()
	{
		Environment.SetEnvironmentVariable("TELEGRAM_BOT_TOKEN", ValidTelegramBotToken);
		Environment.SetEnvironmentVariable("TELEGRAM_PUBLIC_WEBHOOK_URL", ValidTelegramPublicWebhookUrl);
		Environment.SetEnvironmentVariable("TELEGRAM_WEBHOOK_SECRET_TOKEN", ValidTelegramWebhookSecretToken);
	}

	private static void ClearEnvironmentVariables()
	{
		Environment.SetEnvironmentVariable("TELEGRAM_BOT_TOKEN", null);
		Environment.SetEnvironmentVariable("TELEGRAM_PUBLIC_WEBHOOK_URL", null);
		Environment.SetEnvironmentVariable("TELEGRAM_WEBHOOK_SECRET_TOKEN", null);
	}

	private static Dictionary<string, string?> GetValidConfigurationValues()
	{
		return new Dictionary<string, string?>
		{
			{ "BotSettings:RetryTelegramWebhookInitializerDelay", "00:00:30" },
			{ "KitsuSettings:BaseUrl", ValidKitsuBaseUrl },
			{ "KitsuSettings:MaxRetries", "3" },
			{ "KitsuSettings:RetryDelay", "00:00:30" },
			{ "KitsuSettings:MinRequestInterval", "00:00:01" },
			{ "AnimeSettings:CacheLifetime", "1.00:00:00" }
		};
	}

	private static IConfiguration CreateConfiguration(Dictionary<string, string?> values)
	{
		var configBuilder = new ConfigurationBuilder();
		configBuilder.AddInMemoryCollection(values);
		return configBuilder.Build();
	}
}
