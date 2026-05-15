namespace STranslate.Plugin.Dictionary.ECDICT;

public class WordEntry
{
    public string Word { get; set; } = string.Empty;
    public string? Phonetic { get; set; }
    public string? Definition { get; set; }
    public string? Translation { get; set; }
    public string? Pos { get; set; }
    public string? Collins { get; set; }
    public bool Oxford { get; set; }
    public string? Tag { get; set; }
    public string? Bnc { get; set; }
    public string? Frq { get; set; }
    public string? Exchange { get; set; }
    public string? Detail { get; set; }
    public string? Audio { get; set; }
}
