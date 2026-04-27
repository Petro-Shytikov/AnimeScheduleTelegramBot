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
}