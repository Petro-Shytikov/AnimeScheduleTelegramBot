using AnimeScheduleTelegramBot.WebService.Models;

namespace AnimeScheduleTelegramBot.WebService.Services;

public sealed class AnimeProvider(
	CacheService<KitsuAnime, (int Year, string Season)> cacheService) : IAnimeProvider
{
	public async Task<IReadOnlyList<AnimeInfo>> GetCurrentSeasonOngoingsAsync(CancellationToken cancellationToken)
	{
		var loadedOngoings = await cacheService.GetAsync(GetCurrentSeason(), forceRefresh: false, cancellationToken);
		return MapToAnimeInfoList(loadedOngoings);
	}

	internal static (int Year, string Season) GetCurrentSeason()
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

	private static IReadOnlyList<AnimeInfo> MapToAnimeInfoList(IReadOnlyList<KitsuAnime> animes) =>
		animes.Select(MapToAnimeInfo).ToList().AsReadOnly();

	private static AnimeInfo MapToAnimeInfo(KitsuAnime anime) =>
		new(
			Id: anime.Id,
			CanonicalTitle: anime.Attributes.CanonicalTitle,
			EnglishTitle: anime.Attributes.Titles.En,
			Status: anime.Attributes.Status,
			Subtype: anime.Attributes.Subtype,
			StartDate: anime.Attributes.StartDate,
			EpisodeCount: anime.Attributes.EpisodeCount
		);
}