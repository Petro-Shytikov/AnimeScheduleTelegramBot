using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using AnimeScheduleTelegramBot.WebService.Filters;
using AnimeScheduleTelegramBot.WebService.Helpers;
using AnimeScheduleTelegramBot.WebService.Services;

namespace AnimeScheduleTelegramBot.WebService.Controllers;

[ApiController]
[Route("telegram")]
public sealed class TelegramController : ControllerBase
{
	private readonly ITelegramBotClient _botClient;
	private readonly IAppConfiguration _configuration;
	private readonly IAnimeProvider _animeProvider;
	private readonly ILogger<TelegramController> _logger;

	public TelegramController(
		ITelegramBotClient botClient,
		IAppConfiguration configuration,
		IAnimeProvider animeProvider,
		ILogger<TelegramController> logger)
	{
		_botClient = botClient;
		_configuration = configuration;
		_animeProvider = animeProvider;
		_logger = logger;
	}

	[ServiceFilter(typeof(ValidateTelegramSecretFilter))]
	[HttpPost("webhook")]
	public async Task<IActionResult> Webhook([FromBody] Update update, CancellationToken cancellationToken)
	{
		return await (TelegramBotHelper.TryHandle(update) switch
		{
			TelegramBotCommandType.Info => SendInfoMessageAsync(update, cancellationToken),
			TelegramBotCommandType.Ongoings => SendOngoingsMessageAsync(update, cancellationToken),
			TelegramBotCommandType.Week => SendWeekMessageAsync(update, cancellationToken),
			TelegramBotCommandType.Yesterday => SendYesterdayMessageAsync(update, cancellationToken),
			TelegramBotCommandType.Today => SendTodayMessageAsync(update, cancellationToken),
			TelegramBotCommandType.Tomorrow => SendTomorrowMessageAsync(update, cancellationToken),
			TelegramBotCommandType.None => SendMessageAsync(update, cancellationToken),
			_ => SendMessageAsync(update, cancellationToken)
		});
	}

	private async Task<IActionResult> SendInfoMessageAsync(Update update, CancellationToken cancellationToken)
	{
		var chatId = update.Message!.Chat!.Id;
		var infoText = TelegramBotHelper.BuildInfoReply();
		await _botClient.SendMessage(chatId, infoText, cancellationToken: cancellationToken);
		return Ok();
	}

	private async Task<IActionResult> SendOngoingsMessageAsync(Update update, CancellationToken cancellationToken)
	{
		var chatId = update.Message!.Chat!.Id;
		var ongoings = await _animeProvider.GetCurrentSeasonOngoingsAsync(cancellationToken);
		var text = TelegramBotHelper.BuildOngoingsReply(ongoings);
		await _botClient.SendMessage(chatId, text, cancellationToken: cancellationToken);
		return Ok();
	}

	private async Task<IActionResult> SendWeekMessageAsync(Update update, CancellationToken cancellationToken)
	{
		var chatId = update.Message!.Chat!.Id;
		var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);
		var weekRange = CommonHelper.GetCurrentWeekRange(utcToday);
		var weekStartDay = weekRange.Start;
		var weekEndDay = weekRange.End;
		var weekSchedule = await _animeProvider.GetCurrentWeekScheduleAsync(weekStartDay, weekEndDay, cancellationToken);
		var messages = TelegramBotHelper.BuildWeekDayReplies(weekSchedule, weekStartDay);

		foreach (var message in messages)
		{
			await _botClient.SendMessage(chatId, message, cancellationToken: cancellationToken);
		}

		return Ok();
	}

	private async Task<IActionResult> SendYesterdayMessageAsync(Update update, CancellationToken cancellationToken)
	{
		var chatId = update.Message!.Chat!.Id;
		var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
		var schedule = await _animeProvider.GetScheduleForDateAsync(date, cancellationToken);
		var text = TelegramBotHelper.BuildSingleDayReply(date, schedule);
		await _botClient.SendMessage(chatId, text, cancellationToken: cancellationToken);
		return Ok();
	}

	private async Task<IActionResult> SendTodayMessageAsync(Update update, CancellationToken cancellationToken)
	{
		var chatId = update.Message!.Chat!.Id;
		var date = DateOnly.FromDateTime(DateTime.UtcNow);
		var schedule = await _animeProvider.GetScheduleForDateAsync(date, cancellationToken);
		var text = TelegramBotHelper.BuildSingleDayReply(date, schedule);
		await _botClient.SendMessage(chatId, text, cancellationToken: cancellationToken);
		return Ok();
	}

	private async Task<IActionResult> SendTomorrowMessageAsync(Update update, CancellationToken cancellationToken)
	{
		var chatId = update.Message!.Chat!.Id;
		var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
		var schedule = await _animeProvider.GetScheduleForDateAsync(date, cancellationToken);
		var text = TelegramBotHelper.BuildSingleDayReply(date, schedule);
		await _botClient.SendMessage(chatId, text, cancellationToken: cancellationToken);
		return Ok();
	}

	private async Task<IActionResult> SendMessageAsync(Update update, CancellationToken cancellationToken)
	{
		var chatId = update.Message!.Chat!.Id;
		var text = $"Unknown command: {update.Message!.Text!.Split(' ', 2)[0].ToLowerInvariant()}";
		await _botClient.SendMessage(chatId, text, cancellationToken: cancellationToken);
		return Ok();
	}
}

