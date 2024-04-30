using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Reflection;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .Build();

        var languages = GetLanguages(host.Services);

        var globalLocalization = new GlobalLocalization(languages);
        var json = SerializeToJson(globalLocalization);

        File.WriteAllText("localization.json", json);

        Console.WriteLine("Localization JSON file generated successfully.");

        //host.Run();

        //host.Dispose();
    }

    private static Dictionary<Type, IStringLocalizer> GetLanguages(IServiceProvider serviceProvider)
    {
        var languages = new Dictionary<Type, IStringLocalizer>();
        var languageTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Language)));

        foreach (var languageType in languageTypes)
        {
            var language = Activator.CreateInstance(languageType) as Language;

            var localizerType = typeof(IStringLocalizer<>).MakeGenericType(languageType);
            var localizer = new StringLocalizer(language);

            languages.Add(languageType, localizer);
        }

        return languages;
    }

    private static string SerializeToJson(GlobalLocalization globalLocalization)
    {
        var dict = new Dictionary<string, Dictionary<string, string>>();
        string appName = Assembly.GetExecutingAssembly().GetName().Name;
        foreach (var languageType in globalLocalization.Languages.Keys)
        {
            var language = globalLocalization.Languages[languageType];
            dict[$"{appName}.{languageType.Name}"] = language.GetAllStrings().ToDictionary(kv => kv.Name, kv => kv.Value);
        }

        var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        return json;
    }
}

public class GlobalLocalization
{
    public Dictionary<Type, IStringLocalizer> Languages { get; }

    public GlobalLocalization(Dictionary<Type, IStringLocalizer> languages)
    {
        Languages = languages;
    }
}

public class Language : Dictionary<string, string>{}

public class EnglishLanguage : Language
{
    public EnglishLanguage()
    {
        Add("direct use of language", "English value 1");
        Add("some value here for localisation", "English value 2");
        Add("localisation 1", "English value 3");
        Add("localisation 2", "English value 4");
        Add("localisation 3", "English value 5");
        Add("localisation 4", "English value 6");
    }
}

public class SinhalaLanguage : Language
{
    public SinhalaLanguage()
    {
        Add("alternative use of language", "Sinhala value 1");
        Add("Example of use by property \"[] {x@]}", "Sinhala value 2");
    }
}

//public class ChineseLanguage : Language
//{
//    public ChineseLanguage()
//    {
//        Add("Chinese use of language", "Chinese value 1");
//        Add("Example of use by", "Chinese value 2");
//    }
//}

//public class SpanishLanguage : Language
//{
//    public SpanishLanguage()
//    {
//        Add("Spanish use of language", "Spanish value 1");
//        Add("Spanish Example of use by", "Spanish value 2");
//    }
//}

public class StringLocalizer : IStringLocalizer
{
    private readonly Dictionary<string, string> _localizations;

    public StringLocalizer(Dictionary<string, string> localizations)
    {
        _localizations = localizations;
    }

    public LocalizedString this[string name] => new LocalizedString(name, _localizations.ContainsKey(name) ? _localizations[name] : name);

    public LocalizedString this[string name, params object[] arguments] => throw new NotImplementedException();

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return _localizations.Select(kv => new LocalizedString(kv.Key, kv.Value));
    }
}
