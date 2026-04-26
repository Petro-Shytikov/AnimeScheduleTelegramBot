using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using AnimeScheduleTelegramBot.WebService.Filters;
using AnimeScheduleTelegramBot.WebService.Helpers;

namespace AnimeScheduleTelegramBot.WebService.Controllers;

[ApiController]
[Route("telegram")]
public sealed class TelegramController : ControllerBase
{
	private readonly ITelegramBotClient _botClient;
	private readonly IAppConfiguration _configuration;
	private readonly ILogger<TelegramController> _logger;

	public TelegramController(
		ITelegramBotClient botClient,
		IAppConfiguration configuration,
		ILogger<TelegramController> logger)
	{
		_botClient = botClient;
		_configuration = configuration;
		_logger = logger;
	}

	[ServiceFilter(typeof(ValidateTelegramSecretFilter))]
	[HttpPost("webhook")]
	public async Task<IActionResult> Webhook([FromBody] Update update, CancellationToken cancellationToken)
	{
		return await (TelegramBotHelper.TryHandle(update) switch
		{
			TelegramBotCommandType.Info => SendInfoMessageAsync(update, cancellationToken),
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

	private async Task<IActionResult> SendMessageAsync(Update update, CancellationToken cancellationToken)
	{
		var chatId = update.Message!.Chat!.Id;
		var text = $"Unknown command: {update.Message!.Text!.Split(' ', 2)[0].ToLowerInvariant()}";
		await _botClient.SendMessage(chatId, text, cancellationToken: cancellationToken);
		return Ok();
	}
}

