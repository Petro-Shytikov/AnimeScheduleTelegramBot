using System.Text;
using AnimeScheduleTelegramBot.WebService.Models;
using Telegram.Bot.Types;

namespace AnimeScheduleTelegramBot.WebService.Helpers;

public static class TelegramBotHelper
{
	public static TelegramBotCommandType TryHandle(Update update)
	{
		var messageText = update.Message?.Text;
		if (string.IsNullOrWhiteSpace(messageText))
			return TelegramBotCommandType.None;

		if (!messageText.StartsWith('/'))
			return TelegramBotCommandType.None;

		var command = messageText.Split(' ', 2)[0].ToLowerInvariant();

		return command switch
		{
			"/info" => TelegramBotCommandType.Info,
			"/ongoings" => TelegramBotCommandType.Ongoings,
			"/week" => TelegramBotCommandType.Week,
			"/yesterday" => TelegramBotCommandType.Yesterday,
			"/today" => TelegramBotCommandType.Today,
			"/tomorrow" => TelegramBotCommandType.Tomorrow,
			_ => TelegramBotCommandType.Unknown
		};
	}

	public static string BuildInfoReply() =>
	new StringBuilder()
		.AppendLine($"Service Version: {CommonHelper.ServiceVersion}")
		.ToString();

	public static string BuildOngoingsReply(IReadOnlyList<AnimeInfo> ongoings)
	{
		if (ongoings.Count == 0)
			return "No ongoing anime found for the current season.";

		var sb = new StringBuilder();
		sb.AppendLine("Current season ongoings:");
		for (var i = 0; i < ongoings.Count; i++)
		{
			var title = ongoings[i].EnglishTitle ?? ongoings[i].CanonicalTitle;
			sb.AppendLine($"{i + 1}. {title}");
		}
		return sb.ToString();
	}

	public static string BuildWeekReply(IReadOnlyList<AnimeWeekEpisodeInfo> weekSchedule)
	{
		if (weekSchedule.Count == 0)
			return "No episodes scheduled for the current week.";

		var sb = new StringBuilder();
		sb.AppendLine("Current week episodes (UTC):");

		var orderedSchedule = weekSchedule
			.OrderBy(item => item.AirDate)
			.ThenBy(item => item.AnimeTitle)
			.ThenBy(item => item.EpisodeNumber ?? int.MaxValue)
			.ToList();

		var groupedByDate = orderedSchedule.GroupBy(item => item.AirDate);
		foreach (var dateGroup in groupedByDate)
		{
			sb.AppendLine();
			sb.AppendLine($"{dateGroup.Key:dddd, MMM dd}:");

			foreach (var episode in dateGroup)
			{
				var episodeNumberPart = episode.EpisodeNumber is null
					? "Ep ?"
					: $"Ep {episode.EpisodeNumber}";

				var episodeTitlePart = string.IsNullOrWhiteSpace(episode.EpisodeTitle)
					? string.Empty
					: $" - {episode.EpisodeTitle}";

				sb.AppendLine($"- {episode.AnimeTitle} ({episodeNumberPart}){episodeTitlePart}");
			}
		}

		return sb.ToString();
	}

	public static IReadOnlyList<string> BuildWeekDayReplies(IReadOnlyList<AnimeWeekEpisodeInfo> weekSchedule)
	{
		if (weekSchedule.Count == 0)
			return ["No episodes scheduled for the current week."];

		var currentUtcDate = DateOnly.FromDateTime(DateTime.UtcNow);
		var weekStart = GetCurrentWeekStart(currentUtcDate);

		var episodesByDate = weekSchedule
			.OrderBy(item => item.AnimeTitle)
			.ThenBy(item => item.EpisodeNumber ?? int.MaxValue)
			.GroupBy(item => item.AirDate)
			.ToDictionary(group => group.Key, group => group.ToList());

		var replies = new List<string>();

		for (var dayOffset = 0; dayOffset < 7; dayOffset++)
		{
			var dayDate = weekStart.AddDays(dayOffset);
			if (!episodesByDate.TryGetValue(dayDate, out var dayEpisodes))
				continue;

			if (dayEpisodes is null || dayEpisodes.Count == 0)
				continue;

			var sb = new StringBuilder();
			sb.AppendLine($"{dayDate.DayOfWeek}:");

			for (var i = 0; i < dayEpisodes.Count; i++)
			{
				var episode = dayEpisodes[i];
				var episodeNumber = episode.EpisodeNumber?.ToString() ?? "?";
				var episodeTitle = string.IsNullOrWhiteSpace(episode.EpisodeTitle)
					? string.Empty
					: $" {episode.EpisodeTitle}";

				sb.AppendLine($"{i + 1}. {episode.AnimeTitle} - {episodeNumber}{episodeTitle}");
			}

			replies.Add(sb.ToString());
		}

		return replies.AsReadOnly();
	}

	public static string BuildSingleDayReply(DateOnly date, IReadOnlyList<AnimeWeekEpisodeInfo> daySchedule)
	{
		if (daySchedule.Count == 0)
			return $"No episodes scheduled for {date:dddd} (UTC).";

		var sb = new StringBuilder();
		sb.AppendLine($"{date:dddd}:");

		var orderedDaySchedule = daySchedule
			.OrderBy(item => item.AnimeTitle)
			.ThenBy(item => item.EpisodeNumber ?? int.MaxValue)
			.ToList();

		for (var i = 0; i < orderedDaySchedule.Count; i++)
		{
			var episode = orderedDaySchedule[i];
			var episodeNumber = episode.EpisodeNumber?.ToString() ?? "?";
			var episodeTitle = string.IsNullOrWhiteSpace(episode.EpisodeTitle)
				? string.Empty
				: $" {episode.EpisodeTitle}";

			sb.AppendLine($"{i + 1}. {episode.AnimeTitle} - {episodeNumber}{episodeTitle}");
		}

		return sb.ToString();
	}

	private static DateOnly GetCurrentWeekStart(DateOnly utcDate)
	{
		var dayOffset = ((int)utcDate.DayOfWeek + 6) % 7;
		return utcDate.AddDays(-dayOffset);
	}
}