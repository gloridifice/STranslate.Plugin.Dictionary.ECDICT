namespace STranslate.Plugin.Dictionary.ECDICT.Anki;

public class AnkiConnectException : Exception
{
    public AnkiConnectException(string message) : base(message) { }
    public AnkiConnectException(string message, Exception inner) : base(message, inner) { }
}
