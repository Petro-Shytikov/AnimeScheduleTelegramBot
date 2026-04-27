using Telegram.Bot;
using AnimeScheduleTelegramBot.WebService.HttpClientHandlers;
using AnimeScheduleTelegramBot.WebService.Helpers;
using AnimeScheduleTelegramBot.WebService.Services;
using AnimeScheduleTelegramBot.WebService.Services.Kitsu;
using System.Net.Http.Headers;
using System.Threading.RateLimiting;

internal static class ServiceCollectionExtensions
{
	public static IServiceCollection AddAnimeServices(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<RateLimiter>(CreateKitsuRateLimiter);

		services.AddTransient<RateLimitedHandler>();
		services.AddTransient<RetryPolicyHandler>();

		services.AddHttpClient<IKitsuHttpProvider, KitsuHttpProvider>(ConfigureKitsuHttpClient)
		.AddHttpMessageHandler<RateLimitedHandler>()
		.AddHttpMessageHandler<RetryPolicyHandler>();

		services.AddTransient<IAnimeProvider, AnimeProvider>();

		return services;
	}


	public static IServiceCollection AddAppConfiguration(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<IAppConfigurationProvider, AppConfigurationProvider>();
		services.AddSingleton<IAppConfiguration>(serviceProvider =>
			serviceProvider.GetRequiredService<IAppConfigurationProvider>().Create());

		return services;
	}

	public static IServiceCollection AddTelegramBotClient(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<ITelegramBotClient>(CreateTelegramBotClient);

		return services;
	}

	public static IServiceCollection AddTelegramWebhookInitialization(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddHostedService<TelegramWebhookInitializerBackgroundService>();

		return services;
	}

	private static RateLimiter CreateKitsuRateLimiter(IServiceProvider serviceProvider)
	{
		var appConfiguration = serviceProvider.GetRequiredService<IAppConfiguration>();

		return new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
		{
			PermitLimit = 1,
			Window = appConfiguration.KitsuMinRequestInterval,
			QueueLimit = 1,
			QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
			AutoReplenishment = true
		});
	}

	private static void ConfigureKitsuHttpClient(IServiceProvider serviceProvider, HttpClient client)
	{
		var appConfiguration = serviceProvider.GetRequiredService<IAppConfiguration>();

		client.BaseAddress = HttpHelper.CrateBaseUri(appConfiguration.KitsuBaseUrl);
		client.DefaultRequestHeaders.Accept.Clear();
		client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(HttpHelper.KitsuAcceptMediaType));
	}

	private static ITelegramBotClient CreateTelegramBotClient(IServiceProvider serviceProvider)
	{
		var appConfiguration = serviceProvider.GetRequiredService<IAppConfiguration>();
		return new TelegramBotClient(appConfiguration.TelegramBotToken);
	}
}
