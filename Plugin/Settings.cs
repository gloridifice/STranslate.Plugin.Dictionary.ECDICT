namespace STranslate.Plugin.Vocabulary.Anki;

public class Settings
{
    public string AnkiConnectUrl { get; set; } = "http://127.0.0.1:8765";
    public string DeckName { get; set; } = string.Empty;
    public string ModelName { get; set; } = "Basic";
    public string FieldMappingSource { get; set; } = "Front";
    public string FieldMappingTarget { get; set; } = "Back";
    public string Tags { get; set; } = "stranslate";
    public bool AllowDuplicate { get; set; } = true;
    public string DuplicateCheckField { get; set; } = "Front";
}
