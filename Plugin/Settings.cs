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

}
