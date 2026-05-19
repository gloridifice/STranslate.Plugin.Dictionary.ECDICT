using Microsoft.Extensions.Logging;
using STranslate.Plugin.Dictionary.ECDICT.View;
using STranslate.Plugin.Dictionary.ECDICT.ViewModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace STranslate.Plugin.Dictionary.ECDICT;

public partial class Main : DictionaryPluginBase
{
    private Control? _settingUi;
    private SettingsViewModel? _viewModel;
    private Settings Settings { get; set; } = null!;
    private IPluginContext Context { get; set; } = null!;
    private ECDictService DictService { get; } = new();


    public override Control GetSettingUI()
    {
        _viewModel ??= new SettingsViewModel(Context, Settings);
        _settingUi ??= new SettingsView { DataContext = _viewModel };
        return _settingUi;
    }

    public override void Init(IPluginContext context)
    {
        Context = context;
        Settings = context.LoadSettingStorage<Settings>();

        try
        {
            var pluginDir = context.MetaData.PluginDirectory;
            var dbPath = Path.IsPathRooted(Settings.DictionaryPath)
                ? Settings.DictionaryPath
                : Path.Combine(pluginDir, Settings.DictionaryPath);
            var lemmaPath = Path.IsPathRooted(Settings.LemmaPath)
                ? Settings.LemmaPath
                : Path.Combine(pluginDir, Settings.LemmaPath);

            DictService.Initialize(dbPath, Settings.EnableLemma ? lemmaPath : null);
            Context.Logger.LogInformation("ECDICT initialized with database: {Path}", dbPath);
        }
        catch (Exception ex)
        {
            Context.Logger.LogError(ex, "Failed to initialize ECDICT database");
        }

    }

    public override void Dispose()
    {

        _viewModel?.Dispose();
        DictService.Dispose();
    }

    public override async Task TranslateAsync(string content, DictionaryResult result, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        try
        {
            result.Text = string.Empty;
            result.ResultType = DictionaryResultType.None;
            result.Symbols.Clear();
            result.DictMeans.Clear();
            result.Plurals.Clear();
            result.PastTense.Clear();
            result.PastParticiple.Clear();
            result.PresentParticiple.Clear();
            result.ThirdPersonSingular.Clear();
            result.Comparative.Clear();
            result.Superlative.Clear();
            result.Tags.Clear();
            result.Sentences.Clear();

            var word = content.Trim();
            if (string.IsNullOrWhiteSpace(word))
            {
                result.ResultType = DictionaryResultType.NoResult;
                return;
            }

            // Only handle English source text effectively; for non-English pass through
            if (!IsEnglishWord(word))
            {
                result.ResultType = DictionaryResultType.NoResult;
                return;
            }

            WordEntry? entry = null;

            if (Settings.EnableLemma)
            {
                entry = DictService.QueryWithLemma(word);
            }
            else
            {
                entry = DictService.Query(word);
            }

            if (entry == null && Settings.EnableFuzzyMatch)
            {
                var fuzzy = DictService.FuzzyQuery(word, Settings.MaxFuzzyResults);
                entry = fuzzy.FirstOrDefault();
            }

            if (entry == null)
            {
                result.ResultType = DictionaryResultType.NoResult;
                return;
            }

            PopulateResult(result, entry);
            result.ResultType = DictionaryResultType.Success;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            result.ResultType = DictionaryResultType.Error;
            result.Text = $"ECDICT query error: {ex.Message}";
            Context.Logger.LogError(ex, "ECDICT translation failed for: {Content}", content);
        }
        finally
        {
            result.Duration = DateTime.Now - startTime;
            result.IsProcessing = false;
        }

        await Task.CompletedTask;
    }



    // ===================== Result population =====================

    private void PopulateResult(DictionaryResult result, WordEntry entry)
    {
        result.Text = entry.Word;

        // Phonetic
        if (!string.IsNullOrWhiteSpace(entry.Phonetic))
        {
            result.Symbols.Add(new Symbol
            {
                Label = "UK/US",
                Phonetic = entry.Phonetic
            });
        }

        // Translation / Definition -> DictMeans
        var meaningText = !string.IsNullOrWhiteSpace(entry.Translation)
            ? entry.Translation
            : entry.Definition;

        if (!string.IsNullOrWhiteSpace(meaningText))
        {
            var lines = meaningText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            DictMean? currentMean = null;
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                // Try to extract part of speech prefix like "n.", "v.", "adj."
                var match = PosRegex().Match(trimmed);
                if (match.Success)
                {
                    currentMean = new DictMean
                    {
                        PartOfSpeech = match.Value.TrimEnd('.', ' ')
                    };
                    result.DictMeans.Add(currentMean);
                    var rest = trimmed[match.Length..].Trim();
                    if (!string.IsNullOrEmpty(rest))
                    {
                        currentMean.Means.Add(rest);
                    }
                }
                else if (currentMean != null)
                {
                    currentMean.Means.Add(trimmed);
                }
                else
                {
                    currentMean = new DictMean { PartOfSpeech = string.Empty };
                    currentMean.Means.Add(trimmed);
                    result.DictMeans.Add(currentMean);
                }
            }
        }

        // Exchange (word forms)
        if (!string.IsNullOrWhiteSpace(entry.Exchange))
        {
            var parts = entry.Exchange.Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var kv = part.Split(':', 2);
                if (kv.Length != 2) continue;
                var type = kv[0].Trim();
                var value = kv[1].Trim();
                switch (type)
                {
                    case "s":
                        if (Settings.ShowPlurals) result.Plurals.Add(value);
                        break;
                    case "p":
                        if (Settings.ShowPastTense) result.PastTense.Add(value);
                        break;
                    case "d":
                        if (Settings.ShowPastParticiple) result.PastParticiple.Add(value);
                        break;
                    case "i":
                        if (Settings.ShowPresentParticiple) result.PresentParticiple.Add(value);
                        break;
                    case "3":
                        if (Settings.ShowThirdPersonSingular) result.ThirdPersonSingular.Add(value);
                        break;
                    case "r":
                        result.Comparative.Add(value);
                        break;
                    case "t":
                        result.Superlative.Add(value);
                        break;
                }
            }
        }

        // Tags
        if (Settings.ShowTags && !string.IsNullOrWhiteSpace(entry.Tag))
        {
            var tags = entry.Tag.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tags)
            {
                result.Tags.Add(tag);
            }
        }

        // Additional info as a sentence (source info)
        if (Settings.ShowSourceInfo)
        {
            var extra = new List<string>();
            if (!string.IsNullOrWhiteSpace(entry.Collins))
                extra.Add($"Collins: {entry.Collins} stars");
            if (entry.Oxford)
                extra.Add("Oxford 3000");
            if (!string.IsNullOrWhiteSpace(entry.Bnc))
                extra.Add($"BNC: {entry.Bnc}");
            if (!string.IsNullOrWhiteSpace(entry.Frq))
                extra.Add($"FRQ: {entry.Frq}");
            if (!string.IsNullOrWhiteSpace(entry.Pos))
                extra.Add($"POS: {entry.Pos}");

            if (extra.Count > 0)
            {
                result.Sentences.Add(string.Join(" | ", extra));
            }
        }
    }

    private static bool IsEnglishWord(string text)
    {
        // Simple heuristic: mostly ASCII letters, possibly with hyphens/apostrophes
        return text.Trim().Length > 0 && text.All(c => char.IsLetter(c) || c == '-' || c == '\'' || c == ' ');
    }

    [GeneratedRegex(@"^\s*[a-zA-Z]+\.\s*")]
    private static partial Regex PosRegex();
}
