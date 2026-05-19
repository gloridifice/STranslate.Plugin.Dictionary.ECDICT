using CommunityToolkit.Mvvm.ComponentModel;
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


        PropertyChanged += OnPropertyChanged;
    }

    public void Dispose() => PropertyChanged -= OnPropertyChanged;

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
            default:
                return;
        }

        _context.SaveSettingStorage<Settings>();
    }

}
