using MySql.Data.MySqlClient;
using Dapper;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using CineLingoBot.Services;
using CineLingoBot;
using static CineLingoBot.Services.ReminderService;

const string token = "7498065892:AAHAt-D3LPMj1QnErU9RUmhTQcFYqvplNJo";

var bot = new TelegramBotClient(token);
using var cts = new CancellationTokenSource();

var authService = new AuthService();
var testService = new TestService();
var progressService = new ProgressService();
var keyboardFactory = new KeyboardFactory();

var awaitingLogin = new HashSet<long>();
var currentTests = new Dictionary<long, (int WordId, string CorrectTranslation)>();
var reminderService = new ReminderService(bot);
var awaitingReminderTime = new HashSet<long>();


bot.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    new ReceiverOptions(),
    cancellationToken: cts.Token
);

Console.WriteLine("Бот запущен. Нажмите Ctrl+C для остановки.");
await Task.Delay(-1, cts.Token);

async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
{
    if (update.CallbackQuery?.Data?.StartsWith("delete_reminder_") == true)
    {
        var timeStr = update.CallbackQuery.Data.Replace("delete_reminder_", "");
        if (TimeSpan.TryParse(timeStr, out var time))
        {
            if (reminderService.RemoveReminderTime(update.CallbackQuery.From.Id, time))
            {
                await client.AnswerCallbackQuery(update.CallbackQuery.Id, "🗑 Напоминание удалено", cancellationToken: ct);
                await client.SendMessage(update.CallbackQuery.Message.Chat.Id, $"❌ Удалено напоминание: {time:hh\\:mm}", cancellationToken: ct);
            }
        }
        return;
    }

    if (update.CallbackQuery?.Data?.StartsWith("reset_") == true)
    {
        await ProcessResetProgress(client, update.CallbackQuery, ct);
        return;
    }

    if (update.CallbackQuery is { } callback)
    {
        await ProcessTestAnswerAsync(client, callback, ct);
        return;
    }

    if (update.Message is not { Text: { } text, From: { Id: var telegramId } }) return;
    var chatId = update.Message.Chat.Id;

    if (reminderService._userStates.TryGetValue(telegramId, out var state) && state != ReminderService.ReminderState.None)
    {
        await reminderService.ProcessTimeInput(telegramId, chatId, text);
        return;
    }

    try
    {
        switch (text)
        {
            case "/start":
                await HandleStartCommand(client, chatId, telegramId, ct);
                break;
            case "Просмотр слов":
                await HandleViewWords(client, chatId, telegramId, ct);
                break;
            case "Тест: слова":
                await HandleTestCommand(client, chatId, telegramId, ct);
                break;
            case "Тест: пропуски":
                await HandleFillGapTest(client, chatId, telegramId, ct);
                break;
            case "Сбросить прогресс":
                await HandleResetProgress(client, chatId, telegramId, ct);
                break;
            case "Управление напоминаниями":
                await HandleRemindersCommand(client, chatId, telegramId, ct);
                break;
            case "Добавить время напоминания":
                await reminderService.StartSettingReminder(telegramId, chatId);
                break;
            case "Отключить все напоминания":
                reminderService.RemoveAllReminders(telegramId);
                await client.SendMessage(chatId, "🔕 Все напоминания отключены.", cancellationToken: ct);
                break;
            case "Включить уведомление":
                await reminderService.SubscribeAsync(telegramId, chatId);
                break;
            case "Выключить уведомление":
                reminderService.Unsubscribe(telegramId);
                await client.SendMessage(chatId, "🔕 Напоминание отключено.", cancellationToken: ct);
                break;
            case "Посмотреть напоминания":
                if (reminderService.TryGetReminderTimes(telegramId, out var reminderTimes))
                {
                    var message = "🔔 Установленные напоминания:\n" +
                                  string.Join("\n", reminderTimes.Select(t => $"• {t:hh\\:mm}"));
                    await client.SendMessage(chatId, message, cancellationToken: ct);
                }
                else
                {
                    await client.SendMessage(chatId, "📭 У вас нет активных напоминаний.", cancellationToken: ct);
                }
                break;

            case "Удалить конкретное напоминание":
                if (reminderService.TryGetReminderTimes(telegramId, out var timesToDelete))
                {
                    var buttons = timesToDelete
                        .Select(t => InlineKeyboardButton.WithCallbackData(t.ToString("hh\\:mm"), $"delete_reminder_{t}"))
                        .Select(b => new[] { b })
                        .ToArray();

                    await client.SendMessage(chatId,
                        "🗑 Выберите напоминание для удаления:",
                        replyMarkup: new InlineKeyboardMarkup(buttons),
                        cancellationToken: ct);
                }
                else
                {
                    await client.SendMessage(chatId, "📭 У вас нет напоминаний для удаления.", cancellationToken: ct);
                }
                break;
            case "Назад в меню":
                await client.SendMessage(chatId,
                    "Главное меню:",
                    replyMarkup: keyboardFactory.GetMainKeyboard(),
                    cancellationToken: ct);
                break;
            default:
                if (awaitingLogin.Contains(telegramId))
                    await HandleLogin(client, chatId, telegramId, text, ct);
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
        await client.SendMessage(chatId, "❌ Произошла ошибка. Попробуйте позже.", cancellationToken: ct);
    }
}

Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken ct)
{
    Console.WriteLine($"Ошибка: {exception.Message}");
    return Task.CompletedTask;
}

async Task HandleStartCommand(ITelegramBotClient client, long chatId, long telegramId, CancellationToken ct)
{
    await authService.ResetTelegramUser(telegramId);
    awaitingLogin.Add(telegramId);
    await client.SendMessage(chatId, "🔑 Введите ваш логин:", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: ct);
}

async Task HandleLogin(ITelegramBotClient client, long chatId, long telegramId, string login, CancellationToken ct)
{
    var userId = await authService.ValidateLogin(login);
    if (userId == null)
    {
        await client.SendMessage(chatId, "❌ Неверный логин", cancellationToken: ct);
    }
    else
    {
        await authService.BindTelegramUser(userId.Value, telegramId);
        awaitingLogin.Remove(telegramId);
        await client.SendMessage(chatId, "✅ Вы успешно вошли!", replyMarkup: keyboardFactory.GetMainKeyboard(), cancellationToken: ct);
    }
}

async Task HandleViewWords(ITelegramBotClient client, long chatId, long telegramId, CancellationToken ct)
{
    var userId = await authService.GetUserIdByTelegramId(telegramId);
    if (userId == null)
    {
        await client.SendMessage(chatId, "❌ Сначала выполните вход", cancellationToken: ct);
        return;
    }

    var words = await progressService.GetUserWords(userId.Value);
    var message = words.Any()
        ? "📚 Ваши слова:\n" + string.Join("\n", words.Select(w => $"{w.Word} — {w.Translation} ({w.Progress}%)"))
        : "📭 У вас пока нет слов для изучения.";

    await client.SendMessage(chatId, message, cancellationToken: ct);
}

async Task HandleTestCommand(ITelegramBotClient client, long chatId, long telegramId, CancellationToken ct)
{
    var userId = await authService.GetUserIdByTelegramId(telegramId);
    if (userId == null)
    {
        await client.SendMessage(chatId, "❌ Сначала выполните вход", cancellationToken: ct);
        return;
    }

    var wordData = await testService.GetNextTestWord(userId.Value);
    if (wordData is null)
    {
        await client.SendMessage(chatId, "📭 У вас нет слов для теста", cancellationToken: ct);
        return;
    }

    var word = wordData.Value; 

    var options = await testService.GetAnswerOptions(userId.Value, word.Id, word.Translation);
    currentTests[telegramId] = (word.Id, word.Translation);

    await client.SendMessage(chatId,
        $"🔤 Как переводится: *{word.Word}*?",
        replyMarkup: keyboardFactory.CreateInlineOptions(options),
        parseMode: ParseMode.Markdown,
        cancellationToken: ct);
}

async Task HandleFillGapTest(ITelegramBotClient client, long chatId, long telegramId, CancellationToken ct)
{
    var userId = await authService.GetUserIdByTelegramId(telegramId);
    if (userId == null)
    {
        await client.SendMessage(chatId, "❌ Сначала выполните вход", cancellationToken: ct);
        return;
    }

    var wordData = await testService.GetNextFillGapWord(userId.Value);
    if (wordData is null)
    {
        await client.SendMessage(chatId, "📭 У вас нет подходящих фраз для теста", cancellationToken: ct);
        return;
    }

    var word = wordData.Value;

    var (masked, missing) = testService.GetMaskedPhrase(word.Word);
    if (string.IsNullOrWhiteSpace(missing))
    {
        await client.SendMessage(chatId, "❌ Не удалось создать тест на пропуски. Попробуйте снова.", cancellationToken: ct);
        return;
    }

    var options = await testService.GetFillGapAnswerOptions(userId.Value, word.Id, missing);

    currentTests[telegramId] = (word.Id, missing);

    await client.SendMessage(chatId,
        $"🧩 Заполни пропуск:\n*{masked}*",
        replyMarkup: keyboardFactory.CreateInlineOptions(options),
        parseMode: ParseMode.Markdown,
        cancellationToken: ct);
}

async Task HandleResetProgress(ITelegramBotClient client, long chatId, long telegramId, CancellationToken ct)
{
    var userId = await authService.GetUserIdByTelegramId(telegramId);
    if (userId == null)
    {
        await client.SendMessage(chatId, "❌ Сначала выполните вход", cancellationToken: ct);
        return;
    }

    var words = await progressService.GetUserWords(userId.Value);
    if (words == null || !words.Any())
    {
        await client.SendMessage(chatId, "📭 У вас пока нет слов для сброса.", cancellationToken: ct);
        return;
    }

    var buttons = words
        .Select(w => InlineKeyboardButton.WithCallbackData($"{w.Word} — {w.Translation}", $"reset_{w.Id}"))
        .Select(b => new[] { b })
        .ToArray();

    await client.SendMessage(chatId,
        "🔁 Выберите слово/фразу для сброса прогресса:",
        replyMarkup: new InlineKeyboardMarkup(buttons),
        cancellationToken: ct);
}

async Task ProcessResetProgress(ITelegramBotClient client, CallbackQuery callback, CancellationToken ct)
{
    var telegramId = callback.From.Id;
    var chatId = callback.Message.Chat.Id;

    var userId = await authService.GetUserIdByTelegramId(telegramId);
    if (userId == null) return;

    if (!int.TryParse(callback.Data.Replace("reset_", ""), out var wordId)) return;

    await progressService.ResetProgress(userId.Value, wordId);
    await client.AnswerCallbackQuery(callback.Id, "🔄 Прогресс сброшен!", cancellationToken: ct);
    await client.SendMessage(chatId, "✅ Прогресс по слову был успешно сброшен.", cancellationToken: ct);
}

async Task ProcessTestAnswerAsync(ITelegramBotClient client, CallbackQuery callback, CancellationToken ct)
{
    var telegramId = callback.From.Id;
    var chatId = callback.Message.Chat.Id;
    var selected = callback.Data;

    if (!currentTests.TryGetValue(telegramId, out var testData)) return;

    var userId = await authService.GetUserIdByTelegramId(telegramId);
    if (userId == null) return;

    var isCorrect = selected.Equals(testData.CorrectTranslation, StringComparison.OrdinalIgnoreCase);
    await progressService.UpdateProgress(userId.Value, testData.WordId, isCorrect);

    await client.AnswerCallbackQuery(callback.Id,
        isCorrect ? "✅ Верно!" : $"❌ Неверно. Правильный ответ: {testData.CorrectTranslation}",
        showAlert: true,
        cancellationToken: ct);

    var currentProgress = await progressService.GetProgress(userId.Value, testData.WordId);
    if (currentProgress == 100)
    {
        await client.SendMessage(chatId,
            $"🎉 Поздравляем! Вы полностью выучили слово: *{testData.CorrectTranslation}*",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    currentTests.Remove(telegramId);
}

async Task HandleRemindersCommand(ITelegramBotClient client, long chatId, long telegramId, CancellationToken ct)
{
    var hasReminders = reminderService.HasReminders(telegramId);
    var message = hasReminders
        ? "⏰ Ваши текущие напоминания активны. Вы можете добавить новые или отключить все."
        : "⏰ У вас пока нет установленных напоминаний. Хотите добавить?";

    await client.SendMessage(chatId,
        message,
        replyMarkup: keyboardFactory.GetRemindersKeyboard(),
        cancellationToken: ct);
}

public class KeyboardFactory
{
    public ReplyKeyboardMarkup GetMainKeyboard() => new ReplyKeyboardMarkup(new[]
    {
    new[] { new KeyboardButton("Просмотр слов") },
    new[] { new KeyboardButton("Тест: слова") },
    new[] { new KeyboardButton("Тест: пропуски") },
    new[] { new KeyboardButton("Сбросить прогресс") },
    new[] { new KeyboardButton("Управление напоминаниями") },
    new[] { new KeyboardButton("/start") }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = false
    };

    public ReplyKeyboardMarkup GetRemindersKeyboard() => new ReplyKeyboardMarkup(new[]
    {
    new[] { new KeyboardButton("Добавить время напоминания") },
    new[] { new KeyboardButton("Посмотреть напоминания") },
    new[] { new KeyboardButton("Удалить конкретное напоминание") },
    new[] { new KeyboardButton("Отключить все напоминания") },
    new[] { new KeyboardButton("Назад в меню") }
    })
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = false
    };

    public InlineKeyboardMarkup CreateInlineOptions(List<string> options)
    {
        var buttons = options
            .Where(opt => !string.IsNullOrWhiteSpace(opt)) 
            .Select(opt => new[]
            {
            InlineKeyboardButton.WithCallbackData(opt.Trim(), opt.Trim()) 
            })
            .ToArray();

        return new InlineKeyboardMarkup(buttons);
    }
}
