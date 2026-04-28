using AnimeScheduleTelegramBot.WebService.Models;

namespace AnimeScheduleTelegramBot.WebService.Services;

public sealed class AnimeProvider(
	CacheService<KitsuAnime, (int Year, string Season)> cacheService,
	CacheService<AnimeWeekEpisodeInfo, AnimeWeekScheduleContext> weekScheduleCacheService) : IAnimeProvider
{
	public async Task<IReadOnlyList<AnimeInfo>> GetCurrentSeasonOngoingsAsync(CancellationToken cancellationToken)
	{
		var loadedOngoings = await cacheService.GetAsync(GetSeason(DateTime.UtcNow), forceRefresh: false, cancellationToken);
		return MapToAnimeInfoList(loadedOngoings);
	}

	public async Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetCurrentWeekScheduleAsync(CancellationToken cancellationToken)
	{
		var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);
		var weekRange = GetCurrentWeekRange(utcToday);
		var (year, season) = GetSeason(utcToday.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
		var context = new AnimeWeekScheduleContext(
			Year: year,
			Season: season,
			WeekStart: weekRange.Start,
			WeekEnd: weekRange.End);

		return await weekScheduleCacheService.GetAsync(context, forceRefresh: false, cancellationToken);
	}

	public Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetYesterdayScheduleAsync(CancellationToken cancellationToken)
	{
		var targetDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
		return GetScheduleForDateAsync(targetDate, cancellationToken);
	}

	public Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetTodayScheduleAsync(CancellationToken cancellationToken)
	{
		var targetDate = DateOnly.FromDateTime(DateTime.UtcNow);
		return GetScheduleForDateAsync(targetDate, cancellationToken);
	}

	public Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetTomorrowScheduleAsync(CancellationToken cancellationToken)
	{
		var targetDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
		return GetScheduleForDateAsync(targetDate, cancellationToken);
	}

	internal static (int Year, string Season) GetCurrentSeason() =>
		GetSeason(DateTime.UtcNow);

	private async Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetScheduleForDateAsync(DateOnly targetDate, CancellationToken cancellationToken)
	{
		var weekRange = GetCurrentWeekRange(targetDate);
		var (year, season) = GetSeason(targetDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
		var context = new AnimeWeekScheduleContext(
			Year: year,
			Season: season,
			WeekStart: weekRange.Start,
			WeekEnd: weekRange.End);

		var weekSchedule = await weekScheduleCacheService.GetAsync(context, forceRefresh: false, cancellationToken);
		return weekSchedule.Where(item => item.AirDate == targetDate).ToList().AsReadOnly();
	}

	private static (int Year, string Season) GetSeason(DateTime utcDate)
	{
		var season = utcDate.Month switch
		{
			>= 3 and <= 5 => "spring",
			>= 6 and <= 8 => "summer",
			>= 9 and <= 11 => "fall",
			_ => "winter"
		};

		return (utcDate.Year, season);
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