using AnimeScheduleTelegramBot.WebService.Services.Kitsu;

namespace AnimeScheduleTelegramBot.WebService.Services;

public sealed class AnimeProvider(IKitsuHttpProvider kitsuHttpProvider) : IAnimeProvider
{
	public Task<IReadOnlyList<Models.AnimeInfo>> GetCurrentSeasonOngoingsAsync(CancellationToken cancellationToken)
	{
		var (year, season) = GetCurrentSeason();
		return kitsuHttpProvider.GetCurrentSeasonOngoingsAsync(year, season, cancellationToken);
	}

	private static (int Year, string Season) GetCurrentSeason()
	{
		var now = DateTime.UtcNow;
		var season = now.Month switch
		{
			>= 3 and <= 5 => "spring",
			>= 6 and <= 8 => "summer",
			>= 9 and <= 11 => "fall",
			_ => "winter"
		};

		return (now.Year, season);
	}
}