using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace STranslate.Plugin.Vocabulary.Anki.ViewModel;

public partial class SettingsViewModel : ObservableObject, IDisposable
{
    private readonly IPluginContext _context;
    private readonly Settings _settings;

    [ObservableProperty] public partial string AnkiConnectUrl { get; set; }
    [ObservableProperty] public partial string DeckName { get; set; }
    [ObservableProperty] public partial string ModelName { get; set; }
    [ObservableProperty] public partial string FieldMappingSource { get; set; }
    [ObservableProperty] public partial string FieldMappingTarget { get; set; }
    [ObservableProperty] public partial string Tags { get; set; }
    [ObservableProperty] public partial bool AllowDuplicate { get; set; }
    [ObservableProperty] public partial string DuplicateCheckField { get; set; }

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

        AnkiConnectUrl = settings.AnkiConnectUrl;
        DeckName = settings.DeckName;
        ModelName = settings.ModelName;
        FieldMappingSource = settings.FieldMappingSource;
        FieldMappingTarget = settings.FieldMappingTarget;
        Tags = settings.Tags;
        AllowDuplicate = settings.AllowDuplicate;
        DuplicateCheckField = settings.DuplicateCheckField;

        PropertyChanged += OnPropertyChanged;
    }

    public void Dispose() => PropertyChanged -= OnPropertyChanged;

    private AnkiConnectClient Client => _client ??= new AnkiConnectClient(AnkiConnectUrl, _context.HttpService);

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(AnkiConnectUrl):
                _settings.AnkiConnectUrl = AnkiConnectUrl;
                _client = null;
                IsConnected = false;
                break;
            case nameof(DeckName):
                _settings.DeckName = DeckName;
                break;
            case nameof(ModelName):
                _settings.ModelName = ModelName;
                break;
            case nameof(FieldMappingSource):
                _settings.FieldMappingSource = FieldMappingSource;
                break;
            case nameof(FieldMappingTarget):
                _settings.FieldMappingTarget = FieldMappingTarget;
                break;
            case nameof(Tags):
                _settings.Tags = Tags;
                break;
            case nameof(AllowDuplicate):
                _settings.AllowDuplicate = AllowDuplicate;
                break;
            case nameof(DuplicateCheckField):
                _settings.DuplicateCheckField = DuplicateCheckField;
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

            if (!string.IsNullOrEmpty(DeckName) && !Decks.Contains(DeckName))
            {
                DeckName = string.Empty;
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

            if (!string.IsNullOrEmpty(ModelName) && !Models.Contains(ModelName))
            {
                ModelName = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _context.Logger.LogError(ex, "Failed to load model names");
        }
    }

    partial void OnModelNameChanged(string value)
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
                if (!SourceFields.Contains(FieldMappingSource))
                    FieldMappingSource = fields[0];
                if (!TargetFields.Contains(FieldMappingTarget))
                    FieldMappingTarget = fields.Length > 1 ? fields[1] : fields[0];
            }
        }
        catch (Exception ex)
        {
            _context.Logger.LogError(ex, "Failed to load field names for model {ModelName}", modelName);
        }
    }
}
