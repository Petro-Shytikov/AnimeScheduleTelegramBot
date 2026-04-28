using AnimeScheduleTelegramBot.WebService.Models;

namespace AnimeScheduleTelegramBot.WebService.Services;

public sealed class AnimeProvider(
	CacheService<KitsuAnime, (int Year, string Season)> cacheService,
	CacheService<AnimeWeekEpisodeInfo, AnimeWeekScheduleContext> weekScheduleCacheService) : IAnimeProvider
{
	public async Task<IReadOnlyList<AnimeInfo>> GetCurrentSeasonOngoingsAsync(CancellationToken cancellationToken)
	{
		var loadedOngoings = await cacheService.GetAsync(GetCurrentSeason(), forceRefresh: false, cancellationToken);
		return MapToAnimeInfoList(loadedOngoings);
	}

	public async Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetCurrentWeekScheduleAsync(CancellationToken cancellationToken)
	{
		var weekRange = GetCurrentWeekRange(DateOnly.FromDateTime(DateTime.UtcNow));
		var (year, season) = GetCurrentSeason();
		var context = new AnimeWeekScheduleContext(
			Year: year,
			Season: season,
			WeekStart: weekRange.Start,
			WeekEnd: weekRange.End);

		return await weekScheduleCacheService.GetAsync(context, forceRefresh: false, cancellationToken);
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

	private static (DateOnly Start, DateOnly End) GetCurrentWeekRange(DateOnly utcDate)
	{
		var dayOffset = ((int)utcDate.DayOfWeek + 6) % 7;
		var weekStart = utcDate.AddDays(-dayOffset);
		var weekEnd = weekStart.AddDays(6);
		return (weekStart, weekEnd);
	}

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