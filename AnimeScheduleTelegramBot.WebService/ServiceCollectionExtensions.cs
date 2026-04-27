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
		services.AddSingleton<RateLimiter>(serviceProvider =>
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
		});

		services.AddTransient<RateLimitedHandler>();
		services.AddTransient<RetryPolicyHandler>();

		services.AddHttpClient<IKitsuHttpProvider, KitsuHttpProvider>((serviceProvider, client) =>
		{
			var appConfiguration = serviceProvider.GetRequiredService<IAppConfiguration>();

			client.BaseAddress = HttpHelper.CrateBaseUri(appConfiguration.KitsuBaseUrl);
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));
		})
		.AddHttpMessageHandler<RateLimitedHandler>()
		.AddHttpMessageHandler<RetryPolicyHandler>();

		services.AddTransient<IAnimeProvider, AnimeProvider>();

		return services;
	}


	public static IServiceCollection AddAppConfiguration(this IServiceCollection services)
	{
		services.AddSingleton<IAppConfigurationProvider, AppConfigurationProvider>();
		services.AddSingleton<IAppConfiguration>(serviceProvider =>
			serviceProvider.GetRequiredService<IAppConfigurationProvider>().Create());

		return services;
	}

	public static IServiceCollection AddTelegramBotClient(this IServiceCollection services)
	{
		services.AddSingleton<ITelegramBotClient>(serviceProvider =>
		{
			var appConfiguration = serviceProvider.GetRequiredService<IAppConfiguration>();
			return new TelegramBotClient(appConfiguration.TelegramBotToken);
		});

		return services;
	}

	public static IServiceCollection AddTelegramWebhookInitialization(this IServiceCollection services)
	{
		services.AddHostedService<TelegramWebhookInitializerBackgroundService>();

		return services;
	}
}
