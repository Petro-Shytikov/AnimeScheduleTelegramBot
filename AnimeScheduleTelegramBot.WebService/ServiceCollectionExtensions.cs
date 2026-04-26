using Telegram.Bot;
using AnimeScheduleTelegramBot.WebService.Services;

internal static class ServiceCollectionExtensions
{
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
