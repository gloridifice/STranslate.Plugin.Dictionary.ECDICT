using Microsoft.Extensions.Logging;
using STranslate.Plugin.Dictionary.ECDICT.Anki;
using Xunit;

namespace STranslate.Plugin.Dictionary.ECDICT.Tests;

public class SettingsAnkiTests
{
    [Fact]
    public void Settings_DefaultAnkiValues_ShouldBeExpected()
    {
        var settings = new Settings();

        Assert.False(settings.EnableAnkiSave);
        Assert.Equal("Ctrl+Shift+S", settings.SaveToAnkiHotkey);
        Assert.Equal("http://127.0.0.1:8765", settings.AnkiConnectUrl);
        Assert.Equal(string.Empty, settings.AnkiDeckName);
        Assert.Equal("Basic", settings.AnkiModelName);
        Assert.Equal("Front", settings.AnkiFieldWord);
        Assert.Equal("Back", settings.AnkiFieldDefinition);
        Assert.Equal(string.Empty, settings.AnkiFieldPhonetic);
        Assert.Equal("ecdict", settings.AnkiTags);
        Assert.False(settings.AnkiAllowDuplicate);
        Assert.Equal("Front", settings.AnkiDuplicateCheckField);
    }
}

public class VocabularyInterfaceTests
{
    [Fact]
    public void Main_Implements_IVocabularyPlugin()
    {
        var main = new Main();
        Assert.IsAssignableFrom<IVocabularyPlugin>(main);
    }

    [Fact]
    public async Task SaveAsync_WhenAnkiDisabled_ShouldFail()
    {
        var main = new Main();
        // SaveAsync requires Init first; but when EnableAnkiSave=false it fails early without using Context
        var result = await main.SaveAsync("hello");

        Assert.False(result.IsSuccess);
        Assert.Contains("未启用", result.ErrorMessage);
    }
}

public class AnkiConnectClientTests
{
    [Fact]
    public void AnkiConnectException_ShouldPreserveMessage()
    {
        var ex = new AnkiConnectException("test error");
        Assert.Equal("test error", ex.Message);
    }

    [Fact]
    public void AnkiConnectException_WithInner_ShouldPreserveInner()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new AnkiConnectException("outer", inner);
        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
