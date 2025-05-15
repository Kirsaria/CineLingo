using Dapper;
using MySql.Data.MySqlClient;

namespace CineLingoBot.Services;

public class TestService
{
    private readonly string _connectionString = "Server=localhost;Database=cineLingoDictionary;User Id=root;Password=1111;";

    public async Task<(int Id, string Word, string Translation)?> GetNextTestWord(int userId)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();

        return await db.QueryFirstOrDefaultAsync<(int, string, string)?>(@"
            SELECT di.id, di.WordOrPhrase, di.translation 
            FROM DictionaryItem di
            LEFT JOIN UserProgress up ON di.id = up.dictionary_item AND up.user_id = @userId
            WHERE di.userId = @userId
              AND (LENGTH(di.WordOrPhrase) - LENGTH(REPLACE(di.WordOrPhrase, ' ', '')) + 1) <= 3
              AND (up.progress_percent IS NULL OR up.progress_percent < 100)
            ORDER BY RAND()
            LIMIT 1",
            new { userId });
    }

    public async Task<List<string>> GetAnswerOptions(int userId, int wordId, string correctTranslation)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();

        var wrongOptions = await db.QueryAsync<string>(@"
            SELECT translation 
            FROM DictionaryItem 
            WHERE userId = @userId AND id != @wordId 
              AND (LENGTH(WordOrPhrase) - LENGTH(REPLACE(WordOrPhrase, ' ', '')) + 1) <= 3 
            ORDER BY RAND() 
            LIMIT 3",
            new { userId, wordId });

        return wrongOptions.Append(correctTranslation).OrderBy(_ => Guid.NewGuid()).ToList();
    }

    public async Task<List<string>> GetFillGapAnswerOptions(int userId, int wordId, string correctWord)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();

        var wrongOptions = await db.QueryAsync<string>(@"
        SELECT WordOrPhrase 
        FROM DictionaryItem 
        WHERE userId = @userId AND id != @wordId
          AND (LENGTH(WordOrPhrase) - LENGTH(REPLACE(WordOrPhrase, ' ', '')) < 3)
        ORDER BY RAND()
        LIMIT 3",
            new { userId, wordId });

        return wrongOptions
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Append(correctWord)
            .OrderBy(_ => Guid.NewGuid())
            .ToList();
    }

    public async Task<(int Id, string Word, string Translation)?> GetNextFillGapWord(int userId)
    {
        using var db = new MySqlConnection(_connectionString);
        await db.OpenAsync();

        return await db.QueryFirstOrDefaultAsync<(int, string, string)?>(@"
        SELECT di.id, di.WordOrPhrase, di.translation 
        FROM DictionaryItem di
        LEFT JOIN UserProgress up ON di.id = up.dictionary_item AND up.user_id = @userId
        WHERE di.userId = @userId
          AND (LENGTH(di.WordOrPhrase) - LENGTH(REPLACE(di.WordOrPhrase, ' ', '')) + 1) > 2
          AND (up.progress_percent IS NULL OR up.progress_percent < 100)
        ORDER BY RAND()
        LIMIT 1",
            new { userId });
    }


    public (string MaskedPhrase, string MissingWord) GetMaskedPhrase(string phrase)
    {
        var words = phrase.Split(' ');
        if (words.Length < 2) return (phrase, "");

        var rand = new Random();
        var index = rand.Next(words.Length);
        var missingWord = words[index];
        words[index] = "___";

        return (string.Join(" ", words), missingWord);
    }

}