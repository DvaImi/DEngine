using System;
using DEngine.Localization;

public static class VariantHelper
{
    public static string GetVariant(Language language)
    {
        return language switch
        {
            Language.Unspecified => "<None>",
            Language.Afrikaans => "af",
            Language.Albanian => "sq",
            Language.Arabic => "ar",
            Language.Basque => "eu",
            Language.Belarusian => "be",
            Language.Bulgarian => "bg",
            Language.Catalan => "ca",
            Language.ChineseSimplified => "zh-cn",
            Language.ChineseTraditional => "zh-tw",
            Language.Croatian => "hr",
            Language.Czech => "cs",
            Language.Danish => "da",
            Language.Dutch => "nl",
            Language.English => "en",
            Language.Estonian => "et",
            Language.Faroese => "fo",
            Language.Finnish => "fi",
            Language.French => "fr",
            Language.Georgian => "ka",
            Language.German => "de",
            Language.Greek => "el",
            Language.Hebrew => "he",
            Language.Hungarian => "hu",
            Language.Icelandic => "is",
            Language.Indonesian => "id",
            Language.Italian => "it",
            Language.Japanese => "ja",
            Language.Korean => "ko",
            Language.Latvian => "lv",
            Language.Lithuanian => "lt",
            Language.Macedonian => "mk",
            Language.Malayalam => "ml",
            Language.Norwegian => "no",
            Language.Persian => "fa",
            Language.Polish => "pl",
            Language.PortugueseBrazil => "pt-br",
            Language.PortuguesePortugal => "pt-pt",
            Language.Romanian => "ro",
            Language.Russian => "ru",
            Language.SerboCroatian => "sh",
            Language.SerbianCyrillic => "sr",
            Language.SerbianLatin => "sr-latn",
            Language.Slovak => "sk",
            Language.Slovenian => "sl",
            Language.Spanish => "es",
            Language.Swedish => "sv",
            Language.Thai => "th",
            Language.Turkish => "tr",
            Language.Ukrainian => "uk",
            Language.Vietnamese => "vi",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };
    }

    public static string[] GetVariantArray()
    {
        var languages = Enum.GetValues(typeof(Language));
        string[] variants = new string[languages.Length];
        foreach (Language lang in languages)
        {
            variants[(int)lang] = GetVariant(lang);
        }

        return variants;
    }
}