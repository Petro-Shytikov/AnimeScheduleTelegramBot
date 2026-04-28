using AnimeScheduleTelegramBot.WebService.Models;
using AnimeScheduleTelegramBot.WebService.Services.Kitsu;

namespace AnimeScheduleTelegramBot.WebService.Services;

public sealed class KitsuCurrentSeasonAnimeSourceProvider(
	IKitsuHttpProvider kitsuHttpProvider,
	ILogger<KitsuCurrentSeasonAnimeSourceProvider> logger) : ICacheSourceProvider<KitsuAnime, (int Year, string Season)>
{
	public string GetCacheKey((int Year, string Season) context) =>
		$"anime:current-season-ongoings:{context.Year}:{context.Season}";

	public async Task<IReadOnlyList<KitsuAnime>> LoadAsync((int Year, string Season) context, CancellationToken cancellationToken)
	{
		logger.LogInformation("Loading anime source for {Season} {Year}.", context.Season, context.Year);

		var ongoings = await kitsuHttpProvider.GetCurrentSeasonOngoingsAsync(context.Year, context.Season, cancellationToken);
		return ongoings.ToList().AsReadOnly();
	}

}