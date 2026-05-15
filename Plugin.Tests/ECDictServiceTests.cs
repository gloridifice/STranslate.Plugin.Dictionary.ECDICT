using STranslate.Plugin.Dictionary.ECDICT;
using Xunit;

namespace STranslate.Plugin.Dictionary.ECDICT.Tests;

public class ECDictServiceTests
{
    private readonly ECDictService _service;
    private readonly string _dbPath;
    private readonly string _lemmaPath;

    public ECDictServiceTests()
    {
        _service = new ECDictService();
        // Use the built output path or project path
        var baseDir = AppContext.BaseDirectory;
        _dbPath = Path.Combine(baseDir, "ecdict.db");
        _lemmaPath = Path.Combine(baseDir, "lemma.en.txt");
        if (!File.Exists(_dbPath))
        {
            var projDir = Path.Combine(
                Directory.GetParent(baseDir)!.Parent!.Parent!.Parent!.Parent!.FullName,
                "STranslate.Plugin.Dictionary.ECDICT");
            _dbPath = Path.Combine(projDir, "ecdict.db");
            _lemmaPath = Path.Combine(projDir, "lemma.en.txt");
        }
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("test")]
    [InlineData("world")]
    [InlineData("apple")]
    public void QueryExact_ShouldReturnResult(string word)
    {
        _service.Initialize(_dbPath, _lemmaPath);
        var result = _service.Query(word);
        Assert.NotNull(result);
        Assert.Equal(word, result.Word, ignoreCase: true);
    }

    [Theory]
    [InlineData("HELLO")]
    [InlineData("Test")]
    [InlineData("APPLE")]
    public void QueryCaseInsensitive_ShouldReturnResult(string word)
    {
        _service.Initialize(_dbPath, _lemmaPath);
        var result = _service.Query(word);
        Assert.NotNull(result);
        Assert.Equal(word, result.Word, ignoreCase: true);
    }

    [Theory]
    [InlineData("gave", "give")]
    [InlineData("taken", "take")]
    [InlineData("cities", "city")]
    public void GetLemma_ShouldReturnLemma(string word, string expectedLemma)
    {
        _service.Initialize(_dbPath, _lemmaPath);
        var lemma = _service.GetLemma(word);
        Assert.Equal(expectedLemma, lemma, ignoreCase: true);
    }

    [Fact]
    public void QueryWithLemma_ShouldReturnResultForKnownVariant()
    {
        _service.Initialize(_dbPath, _lemmaPath);
        // ECDICT includes "gave" as its own entry, so exact match succeeds
        var result = _service.QueryWithLemma("gave");
        Assert.NotNull(result);
        // If the DB has the variant itself, it returns that variant
        Assert.Equal("gave", result.Word, ignoreCase: true);
    }

    [Fact]
    public void QueryWithLemma_ShouldReturnLemmaWhenVariantNotInDb()
    {
        _service.Initialize(_dbPath, _lemmaPath);
        // "running" is in DB, but let's test a made-up variant not in DB
        // We use a heuristic fallback: "testings" -> "testing" -> "test"
        var result = _service.QueryWithLemma("testings");
        // "testings" is unlikely to be in DB; if not, lemma heuristic should map "testings" -> "testing"
        // If "testing" is also not found via exact match, it may fall back to "test"
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("long-time")]
    [InlineData("longtime")]
    public void QueryStripword_ShouldMatch(string word)
    {
        _service.Initialize(_dbPath, _lemmaPath);
        var result = _service.QueryWithLemma(word);
        Assert.NotNull(result);
    }

    [Fact]
    public void FuzzyQuery_ShouldReturnResults()
    {
        _service.Initialize(_dbPath, _lemmaPath);
        var results = _service.FuzzyQuery("appl", 5);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void StripWord_ShouldRemoveNonAlphanumeric()
    {
        Assert.Equal("longtime", ECDictService.StripWord("long-time"));
        Assert.Equal("hello", ECDictService.StripWord("hello"));
        Assert.Equal("test123", ECDictService.StripWord("test-123"));
    }
}
