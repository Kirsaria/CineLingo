using MySql.Data.MySqlClient;
using Dapper;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using System.Security.Cryptography;
using System.Text;

const string token = "7498065892:AAHAt-D3LPMj1QnErU9RUmhTQcFYqvplNJo";
const string dbConnection = "Server=localhost;Database=cineLingoDictionary;User Id=root;Password=1111;";

var bot = new TelegramBotClient(token);

using var cts = new CancellationTokenSource();

var awaitingLogin = new HashSet<long>();
var awaitingPassword = new Dictionary<long, string>();

bot.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    new ReceiverOptions
    {
        AllowedUpdates = { }, // получать все типы апдейтов
    },
    cancellationToken: cts.Token
);

Console.WriteLine("Бот запущен. Нажмите Ctrl+C для остановки.");
await Task.Delay(-1, cts.Token);

// Основной обработчик апдейтов
async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
{
    if (update.Message is not { Text: { } text, From: { Id: var telegramId } messageFrom })
        return;

    var chatId = update.Message.Chat.Id;

    try
    {
        if (text == "/start")
        {
            await client.SendMessage(chatId, "🔑 Введите ваш логин:", cancellationToken: ct);
            awaitingLogin.Add(chatId);
        }
        else if (awaitingLogin.Contains(chatId))
        {
            awaitingPassword[chatId] = text; 
            awaitingLogin.Remove(chatId);
            await client.SendMessage(chatId, "🔒 Теперь введите ваш пароль:", cancellationToken: ct);
        }
        else if (awaitingPassword.ContainsKey(chatId))
        {
            var login = awaitingPassword[chatId];
            var password = text;
            var hashedPassword = HashPassword(password);

            using var db = new MySqlConnection(dbConnection);
            await db.OpenAsync(ct);

            var userId = await db.QueryFirstOrDefaultAsync<int?>(
                "SELECT id FROM users WHERE username = @login AND passwordHash = @password",
                new { login, password = hashedPassword }
            );

            if (userId == null)
            {
                await client.SendMessage(chatId, "❌ Неверный логин или пароль", cancellationToken: ct);
            }
            else
            {
                await db.ExecuteAsync(
                    "INSERT INTO user_telegram (user_id, telegram_id) VALUES (@userId, @telegramId) " +
                    "ON DUPLICATE KEY UPDATE telegram_id = @telegramId",
                    new { userId, telegramId }
                );

                await client.SendMessage(chatId, "✅ Вы успешно вошли! Используйте /words для просмотра слов.", cancellationToken: ct);
            }

            awaitingPassword.Remove(chatId);
        }
        else if (text == "/words")
        {
            using var db = new MySqlConnection(dbConnection);
            await db.OpenAsync(ct);

            var userId = await db.QueryFirstOrDefaultAsync<int?>(
                "SELECT user_id FROM user_telegram WHERE telegram_id = @telegramId",
                new { telegramId }
            );

            if (userId == null)
            {
                await client.SendMessage(chatId, "❌ Сначала выполните вход (/start)", cancellationToken: ct);
            }
            else
            {
                await SendUserWords(client, chatId, userId.Value, ct);
            }
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

async Task SendUserWords(ITelegramBotClient client, long chatId, int userId, CancellationToken ct)
{
    using var db = new MySqlConnection(dbConnection);
    await db.OpenAsync(ct);

    var words = await db.QueryAsync<(string WordOrPhrase, string Translation)>(
        "SELECT WordOrPhrase, translation FROM DictionaryItem WHERE userId = @userId",
        new { userId }
    );

    var response = words.Any()
        ? "📚 Ваши слова:\n" + string.Join("\n", words.Select(w => $"{w.WordOrPhrase} - {w.Translation}"))
        : "📭 У вас пока нет слов для изучения.";

    await client.SendMessage(chatId, response, cancellationToken: ct);
}

string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
}
