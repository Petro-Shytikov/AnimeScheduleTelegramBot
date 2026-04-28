using AnimeScheduleTelegramBot.WebService.Helpers;
using AnimeScheduleTelegramBot.WebService.Models;
using Telegram.Bot.Types;

namespace AnimeScheduleTelegramBot.Tests.Helpers;

public class TelegramBotHelperTests
{
	// --- TryHandle ---

	[Test]
	public async Task TryHandle_WithNullMessage_ReturnsNone()
	{
		var update = new Update();

		var result = TelegramBotHelper.TryHandle(update);

		await Assert.That(result).IsEqualTo(TelegramBotCommandType.None);
	}

	[Test]
	[Arguments("")]
	[Arguments(" ")]
	[Arguments(null)]
	public async Task TryHandle_WithNullOrWhitespaceMessageText_ReturnsNone(string? text)
	{
		var update = new Update { Message = new Message { Text = text } };

		var result = TelegramBotHelper.TryHandle(update);

		await Assert.That(result).IsEqualTo(TelegramBotCommandType.None);
	}

	[Test]
	[Arguments("hello")]
	[Arguments("not a command")]
	[Arguments("info")]
	public async Task TryHandle_WithTextNotStartingWithSlash_ReturnsNone(string text)
	{
		var update = new Update { Message = new Message { Text = text } };

		var result = TelegramBotHelper.TryHandle(update);

		await Assert.That(result).IsEqualTo(TelegramBotCommandType.None);
	}

	[Test]
	[Arguments("/info", TelegramBotCommandType.Info)]
	[Arguments("/ongoings", TelegramBotCommandType.Ongoings)]
	[Arguments("/week", TelegramBotCommandType.Week)]
	[Arguments("/yesterday", TelegramBotCommandType.Yesterday)]
	[Arguments("/today", TelegramBotCommandType.Today)]
	[Arguments("/tomorrow", TelegramBotCommandType.Tomorrow)]
	[Arguments("/unknown", TelegramBotCommandType.Unknown)]
	[Arguments("/random", TelegramBotCommandType.Unknown)]
	public async Task TryHandle_WithCommand_ReturnsExpectedCommandType(string command, TelegramBotCommandType expected)
	{
		var update = new Update { Message = new Message { Text = command } };

		var result = TelegramBotHelper.TryHandle(update);

		await Assert.That(result).IsEqualTo(expected);
	}

	[Test]
	[Arguments("/INFO")]
	[Arguments("/Info")]
	[Arguments("/iNfO")]
	public async Task TryHandle_WithCommandInMixedCase_ReturnsCorrectType(string command)
	{
		var update = new Update { Message = new Message { Text = command } };

		var result = TelegramBotHelper.TryHandle(update);

		await Assert.That(result).IsEqualTo(TelegramBotCommandType.Info);
	}

	[Test]
	public async Task TryHandle_WithCommandAndTrailingArguments_ParsesCommandCorrectly()
	{
		var update = new Update { Message = new Message { Text = "/info some argument" } };

		var result = TelegramBotHelper.TryHandle(update);

		await Assert.That(result).IsEqualTo(TelegramBotCommandType.Info);
	}

	// --- BuildInfoReply ---

	[Test]
	public async Task BuildInfoReply_ReturnsStringContainingServiceVersionLabel()
	{
		var result = TelegramBotHelper.BuildInfoReply();

		await Assert.That(result).Contains("Service Version:");
	}

	// --- BuildOngoingsReply ---

	[Test]
	public async Task BuildOngoingsReply_WithEmptyList_ReturnsNoOngoingsMessage()
	{
		var result = TelegramBotHelper.BuildOngoingsReply([]);

		await Assert.That(result).IsEqualTo("No ongoing anime found for the current season.");
	}

	[Test]
	public async Task BuildOngoingsReply_WithAnimeHavingEnglishTitle_UsesEnglishTitle()
	{
		var anime = new AnimeInfo("1", "Canonical Title", "English Title", null, null, null, null);

		var result = TelegramBotHelper.BuildOngoingsReply([anime]);

		await Assert.That(result).Contains("English Title");
		await Assert.That(result).DoesNotContain("Canonical Title");
	}

	[Test]
	public async Task BuildOngoingsReply_WithAnimeWithoutEnglishTitle_UsesCanonicalTitle()
	{
		var anime = new AnimeInfo("1", "Canonical Title", null, null, null, null, null);

		var result = TelegramBotHelper.BuildOngoingsReply([anime]);

		await Assert.That(result).Contains("Canonical Title");
	}

	[Test]
	public async Task BuildOngoingsReply_WithMultipleAnime_ReturnsNumberedList()
	{
		var animes = new[]
		{
			new AnimeInfo("1", "Anime One", "Anime One EN", null, null, null, null),
			new AnimeInfo("2", "Anime Two", "Anime Two EN", null, null, null, null),
		};

		var result = TelegramBotHelper.BuildOngoingsReply(animes);

		await Assert.That(result).Contains("1. Anime One EN");
		await Assert.That(result).Contains("2. Anime Two EN");
	}

	// --- BuildWeekReply ---

	[Test]
	public async Task BuildWeekReply_WithEmptyList_ReturnsNoScheduleMessage()
	{
		var result = TelegramBotHelper.BuildWeekReply([]);

		await Assert.That(result).IsEqualTo("No episodes scheduled for the current week.");
	}

	[Test]
	public async Task BuildWeekReply_WithEpisodes_ContainsScheduleHeader()
	{
		var episode = new AnimeWeekEpisodeInfo("1", "Anime A", 1, null, new DateOnly(2025, 1, 6));

		var result = TelegramBotHelper.BuildWeekReply([episode]);

		await Assert.That(result).Contains("Current week episodes (UTC):");
		await Assert.That(result).Contains("Anime A");
	}

	[Test]
	public async Task BuildWeekReply_WithNullEpisodeNumber_ShowsQuestionMark()
	{
		var episode = new AnimeWeekEpisodeInfo("1", "Anime A", null, null, new DateOnly(2025, 1, 6));

		var result = TelegramBotHelper.BuildWeekReply([episode]);

		await Assert.That(result).Contains("Ep ?");
	}

	[Test]
	public async Task BuildWeekReply_WithEpisodeTitle_IncludesEpisodeTitle()
	{
		var episode = new AnimeWeekEpisodeInfo("1", "Anime A", 1, "The Beginning", new DateOnly(2025, 1, 6));

		var result = TelegramBotHelper.BuildWeekReply([episode]);

		await Assert.That(result).Contains("- The Beginning");
	}

	[Test]
	public async Task BuildWeekReply_WithNullEpisodeTitle_DoesNotAppendDash()
	{
		var episode = new AnimeWeekEpisodeInfo("1", "Anime A", 1, null, new DateOnly(2025, 1, 6));

		var result = TelegramBotHelper.BuildWeekReply([episode]);

		// Line should end after episode number info, no trailing " - "
		await Assert.That(result).Contains("(Ep 1)");
		await Assert.That(result).DoesNotContain("(Ep 1) -");
	}

	[Test]
	public async Task BuildWeekReply_WithEpisodesOnDifferentDates_OrdersByDateAscending()
	{
		var monday = new DateOnly(2025, 1, 6);
		var tuesday = new DateOnly(2025, 1, 7);
		var episodes = new[]
		{
			new AnimeWeekEpisodeInfo("2", "Anime B", 1, null, tuesday),
			new AnimeWeekEpisodeInfo("1", "Anime A", 1, null, monday),
		};

		var result = TelegramBotHelper.BuildWeekReply(episodes);

		var mondayIndex = result.IndexOf("Monday", StringComparison.Ordinal);
		var tuesdayIndex = result.IndexOf("Tuesday", StringComparison.Ordinal);
		await Assert.That(mondayIndex).IsLessThan(tuesdayIndex);
	}

	// --- BuildSingleDayReply ---

	[Test]
	public async Task BuildSingleDayReply_WithEmptyList_ReturnsNoDayScheduleMessage()
	{
		var date = new DateOnly(2025, 1, 6); // Monday

		var result = TelegramBotHelper.BuildSingleDayReply(date, []);

		await Assert.That(result).IsEqualTo("No episodes scheduled for Monday (UTC).");
	}

	[Test]
	public async Task BuildSingleDayReply_WithEpisodes_ContainsDayHeader()
	{
		var date = new DateOnly(2025, 1, 6); // Monday
		var episode = new AnimeWeekEpisodeInfo("1", "Anime A", 1, null, date);

		var result = TelegramBotHelper.BuildSingleDayReply(date, [episode]);

		await Assert.That(result).Contains("Monday:");
		await Assert.That(result).Contains("Anime A");
	}

	[Test]
	public async Task BuildSingleDayReply_WithNullEpisodeNumber_ShowsQuestionMark()
	{
		var date = new DateOnly(2025, 1, 6);
		var episode = new AnimeWeekEpisodeInfo("1", "Anime A", null, null, date);

		var result = TelegramBotHelper.BuildSingleDayReply(date, [episode]);

		await Assert.That(result).Contains("- ?");
	}

	[Test]
	public async Task BuildSingleDayReply_WithEpisodeTitle_IncludesEpisodeTitle()
	{
		var date = new DateOnly(2025, 1, 6);
		var episode = new AnimeWeekEpisodeInfo("1", "Anime A", 1, "The Beginning", date);

		var result = TelegramBotHelper.BuildSingleDayReply(date, [episode]);

		await Assert.That(result).Contains("The Beginning");
	}

	[Test]
	public async Task BuildSingleDayReply_WithNullEpisodeTitle_DoesNotAppendTitle()
	{
		var date = new DateOnly(2025, 1, 6);
		var episode = new AnimeWeekEpisodeInfo("1", "Anime A", 1, null, date);

		var result = TelegramBotHelper.BuildSingleDayReply(date, [episode]);

		await Assert.That(result).Contains("Anime A - 1");
		// trailing whitespace should not be present after episode number
		await Assert.That(result).DoesNotContain("Anime A - 1 ");
	}

	[Test]
	public async Task BuildSingleDayReply_WithMultipleEpisodes_ReturnsNumberedListOrderedByTitle()
	{
		var date = new DateOnly(2025, 1, 6);
		var episodes = new[]
		{
			new AnimeWeekEpisodeInfo("2", "Anime B", 2, null, date),
			new AnimeWeekEpisodeInfo("1", "Anime A", 1, null, date),
		};

		var result = TelegramBotHelper.BuildSingleDayReply(date, episodes);

		// Ordered by AnimeTitle then EpisodeNumber
		await Assert.That(result).Contains("1. Anime A - 1");
		await Assert.That(result).Contains("2. Anime B - 2");
	}

	// --- BuildWeekDayReplies ---

	[Test]
	public async Task BuildWeekDayReplies_WithEmptyList_ReturnsSingleNoScheduleMessage()
	{
		var result = TelegramBotHelper.BuildWeekDayReplies([]);

		await Assert.That(result.Count).IsEqualTo(1);
		await Assert.That(result[0]).IsEqualTo("No episodes scheduled for the current week.");
	}

	[Test]
	public async Task BuildWeekDayReplies_WithEpisodeInCurrentWeek_ReturnsReplyContainingAnimeTitle()
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		var episode = new AnimeWeekEpisodeInfo("1", "Anime A", 1, null, today);

		var result = TelegramBotHelper.BuildWeekDayReplies([episode]);

		await Assert.That(result.Count).IsEqualTo(1);
		await Assert.That(result[0]).Contains("Anime A");
	}

	[Test]
	public async Task BuildWeekDayReplies_WithEpisodesOnTwoDaysInCurrentWeek_ReturnsTwoReplies()
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		var dayOffset = ((int)today.DayOfWeek + 6) % 7;
		var weekStart = today.AddDays(-dayOffset); // Monday of current ISO week

		var episodes = new[]
		{
			new AnimeWeekEpisodeInfo("1", "Anime A", 1, null, weekStart),
			new AnimeWeekEpisodeInfo("2", "Anime B", 1, null, weekStart.AddDays(1)),
		};

		var result = TelegramBotHelper.BuildWeekDayReplies(episodes);

		await Assert.That(result.Count).IsEqualTo(2);
	}

	[Test]
	public async Task BuildWeekDayReplies_WithEpisodeOutsideCurrentWeek_ReturnsEmptyList()
	{
		// A date far in the past, not in the current week
		var pastDate = new DateOnly(2020, 1, 1);
		var episode = new AnimeWeekEpisodeInfo("1", "Anime A", 1, null, pastDate);

		var result = TelegramBotHelper.BuildWeekDayReplies([episode]);

		await Assert.That(result.Count).IsEqualTo(0);
	}

	[Test]
	public async Task BuildWeekDayReplies_WithMultipleEpisodesOnSameDay_ReturnsSingleReplyForThatDay()
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		var episodes = new[]
		{
			new AnimeWeekEpisodeInfo("1", "Anime A", 1, null, today),
			new AnimeWeekEpisodeInfo("2", "Anime B", 2, null, today),
		};

		var result = TelegramBotHelper.BuildWeekDayReplies(episodes);

		await Assert.That(result.Count).IsEqualTo(1);
		await Assert.That(result[0]).Contains("Anime A");
		await Assert.That(result[0]).Contains("Anime B");
	}
}
