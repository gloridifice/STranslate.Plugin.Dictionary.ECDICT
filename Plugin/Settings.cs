namespace STranslate.Plugin.Dictionary.ECDICT;

public class Settings
{
    public string DictionaryPath { get; set; } = "ecdict.db";
    public string LemmaPath { get; set; } = "lemma.en.txt";
    public bool EnableLemma { get; set; } = true;
    public bool EnableFuzzyMatch { get; set; } = true;
    public int MaxFuzzyResults { get; set; } = 5;

    // Display options
    public bool ShowPlurals { get; set; } = true;
    public bool ShowPastTense { get; set; } = true;
    public bool ShowPastParticiple { get; set; } = true;
    public bool ShowPresentParticiple { get; set; } = true;
    public bool ShowThirdPersonSingular { get; set; } = true;
    public bool ShowTags { get; set; } = true;
    public bool ShowSourceInfo { get; set; } = true;

    // Anki / Vocabulary settings
    public bool EnableAnkiSave { get; set; } = false;
    public string SaveToAnkiHotkey { get; set; } = "Ctrl+Shift+S";
    public string AnkiConnectUrl { get; set; } = "http://127.0.0.1:8765";
    public string AnkiDeckName { get; set; } = string.Empty;
    public string AnkiModelName { get; set; } = "Basic";
    public string AnkiFieldWord { get; set; } = "Front";
    public string AnkiFieldDefinition { get; set; } = "Back";
    public string AnkiFieldPhonetic { get; set; } = string.Empty;
    public string AnkiTags { get; set; } = "ecdict";
    public bool AnkiAllowDuplicate { get; set; } = false;
    public string AnkiDuplicateCheckField { get; set; } = "Front";
}
