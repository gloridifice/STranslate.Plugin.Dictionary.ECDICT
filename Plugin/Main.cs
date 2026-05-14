using Microsoft.Extensions.Logging;
using STranslate.Plugin.Vocabulary.Anki.View;
using STranslate.Plugin.Vocabulary.Anki.ViewModel;
using System.Windows.Controls;

namespace STranslate.Plugin.Vocabulary.Anki;

public class Main : IVocabularyPlugin
{
    private Control? _settingUi;
    private SettingsViewModel? _viewModel;
    private Settings Settings { get; set; } = null!;
    private IPluginContext Context { get; set; } = null!;

    public Control GetSettingUI()
    {
        _viewModel ??= new SettingsViewModel(Context, Settings);
        _settingUi ??= new SettingsView { DataContext = _viewModel };
        return _settingUi;
    }

    public void Init(IPluginContext context)
    {
        Context = context;
        Settings = context.LoadSettingStorage<Settings>();
    }

    public void Dispose() => _viewModel?.Dispose();

    public async Task<VocabularyResult> SaveAsync(string text, CancellationToken cancellationToken = default)
    {
        var result = new VocabularyResult();
        var startTime = DateTime.Now;
        try
        {
            var client = new AnkiConnectClient(Settings.AnkiConnectUrl, Context.HttpService);

            if (!Settings.AllowDuplicate)
            {
                var exists = await CheckDuplicateAsync(client, text, cancellationToken);
                if (exists)
                {
                    result.Fail("该词已存在于 Anki 中");
                    return result;
                }
            }

            var note = BuildNote(text, string.Empty);
            var noteId = await client.InvokeAsync<long>("addNote", new { note }, cancellationToken);
            result.IsSuccess = noteId > 0;
        }
        catch (AnkiConnectException ex)
        {
            result.Fail($"Anki 错误: {ex.Message}");
            Context.Logger.LogError(ex, "Anki addNote failed for text: {Text}", text);
        }
        catch (Exception ex)
        {
            result.Fail($"保存至 Anki 失败: {ex.Message}");
            Context.Logger.LogError(ex, "Save to Anki failed for text: {Text}", text);
        }
        finally
        {
            result.Duration = DateTime.Now - startTime;
        }

        return result;
    }

    public async Task<VocabularyResult> SaveWithNoteAsync(string word, string note, CancellationToken cancellationToken = default)
    {
        var result = new VocabularyResult();
        var startTime = DateTime.Now;
        try
        {
            var client = new AnkiConnectClient(Settings.AnkiConnectUrl, Context.HttpService);

            if (!Settings.AllowDuplicate)
            {
                var exists = await CheckDuplicateAsync(client, word, cancellationToken);
                if (exists)
                {
                    result.Fail("该词已存在于 Anki 中");
                    return result;
                }
            }

            var noteObj = BuildNote(word, note);
            var noteId = await client.InvokeAsync<long>("addNote", new { note = noteObj }, cancellationToken);
            result.IsSuccess = noteId > 0;
        }
        catch (AnkiConnectException ex)
        {
            result.Fail($"Anki 错误: {ex.Message}");
            Context.Logger.LogError(ex, "Anki addNote failed for word: {Word}", word);
        }
        catch (Exception ex)
        {
            result.Fail($"保存至 Anki 失败: {ex.Message}");
            Context.Logger.LogError(ex, "Save to Anki failed for word: {Word}", word);
        }
        finally
        {
            result.Duration = DateTime.Now - startTime;
        }

        return result;
    }

    private async Task<bool> CheckDuplicateAsync(AnkiConnectClient client, string text, CancellationToken cancellationToken)
    {
        try
        {
            var query = $"\"{Settings.DuplicateCheckField}:{EscapeQuery(text)}\"";
            var notes = await client.InvokeAsync<long[]>("findNotes", new { query }, cancellationToken);
            return notes.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    private object BuildNote(string sourceText, string targetText)
    {
        var fields = new Dictionary<string, string>
        {
            { Settings.FieldMappingSource, sourceText },
            { Settings.FieldMappingTarget, targetText }
        };

        var tags = string.IsNullOrWhiteSpace(Settings.Tags)
            ? Array.Empty<string>()
            : Settings.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new
        {
            deckName = Settings.DeckName,
            modelName = Settings.ModelName,
            fields,
            tags
        };
    }

    private static string EscapeQuery(string text)
    {
        return text.Replace("\"", "\\\"");
    }
}
