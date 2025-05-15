using Dapper;
using MySql.Data.MySqlClient;

namespace CineLingoBot.Services;

public class ProgressService
{
    private readonly string _connectionString = "Server=localhost;Database=cineLingoDictionary;User Id=root;Password=1111;";

    public async Task<List<(int Id, string Word, string Translation, int Progress)>> GetUserWords(int userId)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();

        var results = await db.QueryAsync<(int Id, string Word, string Translation, int Progress)>(
            @"SELECT d.Id as Id, d.WordOrPhrase as Word, d.translation as Translation, 
                      IFNULL(up.progress_percent, 0) AS Progress
              FROM DictionaryItem d
              LEFT JOIN UserProgress up ON d.id = up.dictionary_item AND up.user_id = @userId
              WHERE d.userId = @userId",
            new { userId });

        return results.ToList();
    }

    public async Task UpdateProgress(int userId, int wordId, bool isCorrect)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();

        await db.ExecuteAsync(@"
            INSERT INTO UserProgress (user_id, dictionary_item, correct_answer, wrong_answer)
            VALUES (@userId, @wordId, @correct, @wrong)
            ON DUPLICATE KEY UPDATE
                correct_answer = correct_answer + @correct,
                wrong_answer = wrong_answer + @wrong,
                progress_percent = least(progress_percent + @progress, 100)",
            new
            {
                userId,
                wordId,
                correct = isCorrect ? 1 : 0,
                wrong = isCorrect ? 0 : 1,
                progress = isCorrect ? 20 : 0
            });
    }

    public async Task<int?> GetProgress(int userId, int wordId)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();

        return await db.QueryFirstOrDefaultAsync<int?>(
            "SELECT progress_percent FROM UserProgress WHERE user_id = @userId AND dictionary_item = @wordId",
            new { userId, wordId });
    }

    public async Task ResetProgress(int userId, int wordId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "UPDATE UserProgress SET progress_percent = 0, correct_answer = 0, wrong_answer = 0 WHERE user_id = @userId AND dictionary_item = @wordId",
            new { userId, wordId });
    }
}
