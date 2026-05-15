using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using STranslate.Plugin.Dictionary.ECDICT.Anki;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace STranslate.Plugin.Dictionary.ECDICT.ViewModel;

public partial class SettingsViewModel : ObservableObject, IDisposable
{
    private readonly IPluginContext _context;
    private readonly Settings _settings;

    // Dictionary settings
    [ObservableProperty] public partial string DictionaryPath { get; set; }
    [ObservableProperty] public partial string LemmaPath { get; set; }
    [ObservableProperty] public partial bool EnableLemma { get; set; }
    [ObservableProperty] public partial bool EnableFuzzyMatch { get; set; }
    [ObservableProperty] public partial int MaxFuzzyResults { get; set; }

    // Display options
    [ObservableProperty] public partial bool ShowPlurals { get; set; }
    [ObservableProperty] public partial bool ShowPastTense { get; set; }
    [ObservableProperty] public partial bool ShowPastParticiple { get; set; }
    [ObservableProperty] public partial bool ShowPresentParticiple { get; set; }
    [ObservableProperty] public partial bool ShowThirdPersonSingular { get; set; }
    [ObservableProperty] public partial bool ShowTags { get; set; }
    [ObservableProperty] public partial bool ShowSourceInfo { get; set; }

    // Anki / Vocabulary settings
    [ObservableProperty] public partial bool EnableAnkiSave { get; set; }
    [ObservableProperty] public partial string SaveToAnkiHotkey { get; set; }
    [ObservableProperty] public partial string AnkiConnectUrl { get; set; }
    [ObservableProperty] public partial string AnkiDeckName { get; set; }
    [ObservableProperty] public partial string AnkiModelName { get; set; }
    [ObservableProperty] public partial string AnkiFieldWord { get; set; }
    [ObservableProperty] public partial string AnkiFieldDefinition { get; set; }
    [ObservableProperty] public partial string AnkiFieldPhonetic { get; set; }
    [ObservableProperty] public partial string AnkiTags { get; set; }
    [ObservableProperty] public partial bool AnkiAllowDuplicate { get; set; }
    [ObservableProperty] public partial string AnkiDuplicateCheckField { get; set; }

    [ObservableProperty] public partial bool IsConnected { get; set; }
    [ObservableProperty] public partial bool IsBusy { get; set; }

    public ObservableCollection<string> Decks { get; } = new();
    public ObservableCollection<string> Models { get; } = new();
    public ObservableCollection<string> SourceFields { get; } = new();
    public ObservableCollection<string> TargetFields { get; } = new();

    private AnkiConnectClient? _client;

    public SettingsViewModel(IPluginContext context, Settings settings)
    {
        _context = context;
        _settings = settings;

        // Dictionary
        DictionaryPath = settings.DictionaryPath;
        LemmaPath = settings.LemmaPath;
        EnableLemma = settings.EnableLemma;
        EnableFuzzyMatch = settings.EnableFuzzyMatch;
        MaxFuzzyResults = settings.MaxFuzzyResults;

        // Display
        ShowPlurals = settings.ShowPlurals;
        ShowPastTense = settings.ShowPastTense;
        ShowPastParticiple = settings.ShowPastParticiple;
        ShowPresentParticiple = settings.ShowPresentParticiple;
        ShowThirdPersonSingular = settings.ShowThirdPersonSingular;
        ShowTags = settings.ShowTags;
        ShowSourceInfo = settings.ShowSourceInfo;

        // Anki
        EnableAnkiSave = settings.EnableAnkiSave;
        SaveToAnkiHotkey = settings.SaveToAnkiHotkey;
        AnkiConnectUrl = settings.AnkiConnectUrl;
        AnkiDeckName = settings.AnkiDeckName;
        AnkiModelName = settings.AnkiModelName;
        AnkiFieldWord = settings.AnkiFieldWord;
        AnkiFieldDefinition = settings.AnkiFieldDefinition;
        AnkiFieldPhonetic = settings.AnkiFieldPhonetic;
        AnkiTags = settings.AnkiTags;
        AnkiAllowDuplicate = settings.AnkiAllowDuplicate;
        AnkiDuplicateCheckField = settings.AnkiDuplicateCheckField;

        PropertyChanged += OnPropertyChanged;
    }

    public void Dispose() => PropertyChanged -= OnPropertyChanged;

    private AnkiConnectClient Client => _client ??= new AnkiConnectClient(AnkiConnectUrl, _context.HttpService);

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            // Dictionary
            case nameof(DictionaryPath):
                _settings.DictionaryPath = DictionaryPath;
                break;
            case nameof(LemmaPath):
                _settings.LemmaPath = LemmaPath;
                break;
            case nameof(EnableLemma):
                _settings.EnableLemma = EnableLemma;
                break;
            case nameof(EnableFuzzyMatch):
                _settings.EnableFuzzyMatch = EnableFuzzyMatch;
                break;
            case nameof(MaxFuzzyResults):
                _settings.MaxFuzzyResults = MaxFuzzyResults;
                break;
            // Display
            case nameof(ShowPlurals):
                _settings.ShowPlurals = ShowPlurals;
                break;
            case nameof(ShowPastTense):
                _settings.ShowPastTense = ShowPastTense;
                break;
            case nameof(ShowPastParticiple):
                _settings.ShowPastParticiple = ShowPastParticiple;
                break;
            case nameof(ShowPresentParticiple):
                _settings.ShowPresentParticiple = ShowPresentParticiple;
                break;
            case nameof(ShowThirdPersonSingular):
                _settings.ShowThirdPersonSingular = ShowThirdPersonSingular;
                break;
            case nameof(ShowTags):
                _settings.ShowTags = ShowTags;
                break;
            case nameof(ShowSourceInfo):
                _settings.ShowSourceInfo = ShowSourceInfo;
                break;
            // Anki
            case nameof(EnableAnkiSave):
                _settings.EnableAnkiSave = EnableAnkiSave;
                break;
            case nameof(SaveToAnkiHotkey):
                _settings.SaveToAnkiHotkey = SaveToAnkiHotkey;
                break;
            case nameof(AnkiConnectUrl):
                _settings.AnkiConnectUrl = AnkiConnectUrl;
                _client = null;
                IsConnected = false;
                break;
            case nameof(AnkiDeckName):
                _settings.AnkiDeckName = AnkiDeckName;
                break;
            case nameof(AnkiModelName):
                _settings.AnkiModelName = AnkiModelName;
                break;
            case nameof(AnkiFieldWord):
                _settings.AnkiFieldWord = AnkiFieldWord;
                break;
            case nameof(AnkiFieldDefinition):
                _settings.AnkiFieldDefinition = AnkiFieldDefinition;
                break;
            case nameof(AnkiFieldPhonetic):
                _settings.AnkiFieldPhonetic = AnkiFieldPhonetic;
                break;
            case nameof(AnkiTags):
                _settings.AnkiTags = AnkiTags;
                break;
            case nameof(AnkiAllowDuplicate):
                _settings.AnkiAllowDuplicate = AnkiAllowDuplicate;
                break;
            case nameof(AnkiDuplicateCheckField):
                _settings.AnkiDuplicateCheckField = AnkiDuplicateCheckField;
                break;
            default:
                return;
        }

        _context.SaveSettingStorage<Settings>();
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        IsBusy = true;
        try
        {
            IsConnected = await Client.CheckConnectionAsync();
            if (IsConnected)
            {
                _context.Snackbar.ShowSuccess("AnkiConnect 连接成功");
                await RefreshDecksAsync();
                await RefreshModelsAsync();
            }
            else
            {
                _context.Snackbar.ShowError("AnkiConnect 连接失败");
            }
        }
        catch (Exception ex)
        {
            _context.Snackbar.ShowError("AnkiConnect 连接失败");
            _context.Logger.LogError(ex, "AnkiConnect test connection failed");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshDecksAsync()
    {
        try
        {
            var decks = await Client.InvokeAsync<string[]>("deckNames");
            Decks.Clear();
            foreach (var deck in decks)
            {
                Decks.Add(deck);
            }

            if (!string.IsNullOrEmpty(AnkiDeckName) && !Decks.Contains(AnkiDeckName))
            {
                AnkiDeckName = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _context.Logger.LogError(ex, "Failed to load deck names");
        }
    }

    [RelayCommand]
    private async Task RefreshModelsAsync()
    {
        try
        {
            var models = await Client.InvokeAsync<string[]>("modelNames");
            Models.Clear();
            foreach (var model in models)
            {
                Models.Add(model);
            }

            if (!string.IsNullOrEmpty(AnkiModelName) && !Models.Contains(AnkiModelName))
            {
                AnkiModelName = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _context.Logger.LogError(ex, "Failed to load model names");
        }
    }

    partial void OnAnkiModelNameChanged(string value)
    {
        _ = LoadFieldsAsync(value);
    }

    private async Task LoadFieldsAsync(string modelName)
    {
        if (string.IsNullOrEmpty(modelName) || _client == null)
            return;

        try
        {
            var fields = await Client.InvokeAsync<string[]>("modelFieldNames", new { modelName });
            SourceFields.Clear();
            TargetFields.Clear();
            foreach (var field in fields)
            {
                SourceFields.Add(field);
                TargetFields.Add(field);
            }

            if (fields.Length > 0)
            {
                if (!SourceFields.Contains(AnkiFieldWord))
                    AnkiFieldWord = fields[0];
                if (!TargetFields.Contains(AnkiFieldDefinition))
                    AnkiFieldDefinition = fields.Length > 1 ? fields[1] : fields[0];
                if (!SourceFields.Contains(AnkiDuplicateCheckField))
                    AnkiDuplicateCheckField = fields[0];
            }
        }
        catch (Exception ex)
        {
            _context.Logger.LogError(ex, "Failed to load field names for model {ModelName}", modelName);
        }
    }
}
