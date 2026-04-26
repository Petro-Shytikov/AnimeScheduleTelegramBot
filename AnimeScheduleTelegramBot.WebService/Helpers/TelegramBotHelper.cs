using System.Text;
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
			_ => TelegramBotCommandType.Unknown
		};
	}

	public static string BuildInfoReply() =>
	new StringBuilder()
		.AppendLine($"Service Version: {CommonHelper.ServiceVersion}")
		.ToString();
}