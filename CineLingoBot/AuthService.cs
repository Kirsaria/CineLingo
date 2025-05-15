using Dapper;
using MySql.Data.MySqlClient;

namespace CineLingoBot.Services;

internal class AuthService
{
    private readonly string _connectionString = "Server=localhost;Database=cineLingoDictionary;User Id=root;Password=1111;";

    public async Task<int?> ValidateLogin(string login)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();
        return await db.QueryFirstOrDefaultAsync<int?>(
            "SELECT id FROM users WHERE username = @login",
            new { login });
    }

    public async Task BindTelegramUser(int userId, long telegramId)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();
        await db.ExecuteAsync(@"
            INSERT INTO user_telegram (user_id, telegram_id)
            VALUES (@userId, @telegramId)
            ON DUPLICATE KEY UPDATE telegram_id = @telegramId",
            new { userId, telegramId });
    }

    public async Task<int?> GetUserIdByTelegramId(long telegramId)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();
        return await db.QueryFirstOrDefaultAsync<int?>(
            "SELECT user_id FROM user_telegram WHERE telegram_id = @telegramId",
            new { telegramId });
    }

    public async Task ResetTelegramUser(long telegramId)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();
        await db.ExecuteAsync(
            "DELETE FROM user_telegram WHERE telegram_id = @telegramId",
            new { telegramId });
    }
}