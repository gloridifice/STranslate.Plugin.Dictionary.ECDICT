using Microsoft.Data.Sqlite;
using System.IO;
using System.Text.RegularExpressions;

namespace STranslate.Plugin.Dictionary.ECDICT;

public partial class ECDictService : IDisposable
{
    private SqliteConnection? _connection;
    private readonly Dictionary<string, string> _lemmaMap = new(StringComparer.OrdinalIgnoreCase);
    private bool _initialized;

    public void Initialize(string dbPath, string? lemmaPath = null)
    {
        if (_initialized) return;

        // Resolve relative path against plugin directory
        var fullDbPath = Path.IsPathRooted(dbPath) ? dbPath : Path.Combine(AppContext.BaseDirectory, dbPath);
        if (!File.Exists(fullDbPath))
        {
            // fallback: search in current directory
            fullDbPath = Path.Combine(Directory.GetCurrentDirectory(), dbPath);
        }

        if (!File.Exists(fullDbPath))
        {
            throw new FileNotFoundException($"ECDICT database not found: {dbPath}", fullDbPath);
        }

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullDbPath,
            Mode = SqliteOpenMode.ReadOnly,
            Cache = SqliteCacheMode.Shared
        }.ToString();

        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        // Ensure necessary indexes exist (if writable, but we are read-only; assume db is pre-built with indexes)
        if (!string.IsNullOrWhiteSpace(lemmaPath))
        {
            LoadLemma(lemmaPath);
        }

        _initialized = true;
    }

    private void LoadLemma(string lemmaPath)
    {
        var fullPath = Path.IsPathRooted(lemmaPath)
            ? lemmaPath
            : Path.Combine(AppContext.BaseDirectory, lemmaPath);

        if (!File.Exists(fullPath))
        {
            fullPath = Path.Combine(Directory.GetCurrentDirectory(), lemmaPath);
        }

        if (!File.Exists(fullPath)) return;

        foreach (var line in File.ReadLines(fullPath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";")) continue;

            // lemma.en.txt format: lemma/count -> variant1,variant2,variant3
            // e.g. "take/172773 -> took,taken,taking,takes"
            var arrowIndex = trimmed.IndexOf("->", StringComparison.Ordinal);
            if (arrowIndex < 0) continue;

            var left = trimmed[..arrowIndex].Trim();
            var right = trimmed[(arrowIndex + 2)..].Trim();

            // Extract lemma from left side (remove "/count" suffix)
            var slashIndex = left.IndexOf('/');
            var lemma = slashIndex >= 0 ? left[..slashIndex].Trim().ToLowerInvariant() : left.ToLowerInvariant();

            // Parse variants from right side
            var variants = right.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var variant in variants)
            {
                var key = variant.Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(key))
                {
                    _lemmaMap[key] = lemma;
                }
            }

            // Also map the lemma itself
            if (!string.IsNullOrEmpty(lemma))
            {
                _lemmaMap[lemma] = lemma;
            }
        }
    }

    public WordEntry? Query(string word)
    {
        EnsureInitialized();
        return QueryInternal(word, exact: true);
    }

    public WordEntry? QueryWithLemma(string word)
    {
        EnsureInitialized();

        // 1. Try exact query first
        var result = QueryInternal(word, exact: true);
        if (result != null) return result;

        // 2. Try lemma lookup
        var lemma = GetLemma(word);
        if (!string.IsNullOrEmpty(lemma) && !string.Equals(lemma, word, StringComparison.OrdinalIgnoreCase))
        {
            result = QueryInternal(lemma, exact: true);
            if (result != null) return result;
        }

        // 3. Try stripword fuzzy match
        result = QueryInternal(word, exact: false);
        return result;
    }

    public List<WordEntry> FuzzyQuery(string word, int limit = 5)
    {
        EnsureInitialized();
        var results = new List<WordEntry>();
        var sw = StripWord(word);

        using var cmd = _connection!.CreateCommand();
        cmd.CommandText =
            "SELECT word, phonetic, definition, translation, pos, collins, oxford, tag, bnc, frq, exchange, detail, audio " +
            "FROM stardict WHERE sw LIKE @sw ORDER BY frq ASC LIMIT @limit;";
        cmd.Parameters.AddWithValue("@sw", sw + "%");
        cmd.Parameters.AddWithValue("@limit", limit);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(ReadEntry(reader));
        }

        return results;
    }

    private WordEntry? QueryInternal(string word, bool exact)
    {
        using var cmd = _connection!.CreateCommand();
        if (exact)
        {
            cmd.CommandText =
                "SELECT word, phonetic, definition, translation, pos, collins, oxford, tag, bnc, frq, exchange, detail, audio " +
                "FROM stardict WHERE word = @word COLLATE NOCASE LIMIT 1;";
            cmd.Parameters.AddWithValue("@word", word);
        }
        else
        {
            var sw = StripWord(word);
            cmd.CommandText =
                "SELECT word, phonetic, definition, translation, pos, collins, oxford, tag, bnc, frq, exchange, detail, audio " +
                "FROM stardict WHERE sw = @sw COLLATE NOCASE LIMIT 1;";
            cmd.Parameters.AddWithValue("@sw", sw);
        }

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return ReadEntry(reader);
        }
        return null;
    }

    private WordEntry ReadEntry(SqliteDataReader reader)
    {
        return new WordEntry
        {
            Word = reader.GetString(0),
            Phonetic = reader.IsDBNull(1) ? null : reader.GetString(1),
            Definition = reader.IsDBNull(2) ? null : reader.GetString(2),
            Translation = reader.IsDBNull(3) ? null : reader.GetString(3),
            Pos = reader.IsDBNull(4) ? null : reader.GetString(4),
            Collins = reader.IsDBNull(5) ? null : reader.GetString(5),
            Oxford = reader.IsDBNull(6) ? false : reader.GetString(6) == "1" || reader.GetString(6).Equals("true", StringComparison.OrdinalIgnoreCase),
            Tag = reader.IsDBNull(7) ? null : reader.GetString(7),
            Bnc = reader.IsDBNull(8) ? null : reader.GetString(8),
            Frq = reader.IsDBNull(9) ? null : reader.GetString(9),
            Exchange = reader.IsDBNull(10) ? null : reader.GetString(10),
            Detail = reader.IsDBNull(11) ? null : reader.GetString(11),
            Audio = reader.IsDBNull(12) ? null : reader.GetString(12)
        };
    }

    public string? GetLemma(string word)
    {
        var key = word.ToLowerInvariant();
        if (_lemmaMap.TryGetValue(key, out var lemma))
            return lemma;

        // Simple heuristic fallback for common patterns
        if (key.EndsWith("ies") && key.Length > 3)
            return key[..^3] + "y"; // cities -> city
        if (key.EndsWith("ied") && key.Length > 3)
            return key[..^3] + "y"; // tried -> try
        if (key.EndsWith("ies") && key.Length > 3)
            return key[..^3] + "y";
        if (key.EndsWith("s") && !key.EndsWith("ss") && key.Length > 1)
            return key[..^1]; // apples -> apple
        if (key.EndsWith("ed") && key.Length > 2)
        {
            var baseWord = key[..^2];
            if (baseWord.Length > 0 && "aeiou".Contains(baseWord[^1]))
                return baseWord[..^1] + "e"; // loved -> love (simplistic)
            return baseWord;
        }
        if (key.EndsWith("ing") && key.Length > 3)
        {
            var baseWord = key[..^3];
            if (baseWord.Length > 0 && baseWord[^1] == baseWord[^2] && !"aeiou".Contains(baseWord[^1]))
                return baseWord[..^1]; // running -> run
            if (baseWord.EndsWith("y"))
                return baseWord; // trying -> try (incorrect but close)
            return baseWord;
        }

        return null;
    }

    public static string StripWord(string word)
    {
        // Remove all non-alphanumeric characters and lowercase
        return MyRegex().Replace(word.ToLowerInvariant(), "");
    }

    [GeneratedRegex("[^a-zA-Z0-9]+")]
    private static partial Regex MyRegex();

    private void EnsureInitialized()
    {
        if (!_initialized || _connection == null)
            throw new InvalidOperationException("ECDictService not initialized. Call Initialize() first.");
    }

    public void Dispose()
    {
        _connection?.Dispose();
        _connection = null;
        _initialized = false;
    }
}
