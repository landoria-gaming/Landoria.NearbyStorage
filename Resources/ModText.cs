using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace Landoria.NearbyStorage
{
    // Reads embedded translations for the selected Valheim language.
    internal static class ModText
    {
        internal const string ModName = "Nearby Storage";
        private static readonly Dictionary<string, string> LanguageCodes =
            new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Swedish", "sv" }, { "French", "fr" }, { "Italian", "it" },
            { "German", "de" }, { "Spanish", "es" }, { "Russian", "ru" },
            { "Romanian", "ro" }, { "Bulgarian", "bg" }, { "Macedonian", "mk" },
            { "Finnish", "fi" }, { "Danish", "da" }, { "Norwegian", "no" },
            { "Icelandic", "is" }, { "Turkish", "tr" }, { "Lithuanian", "lt" },
            { "Czech", "cs" }, { "Hungarian", "hu" }, { "Slovak", "sk" },
            { "Polish", "pl" }, { "Dutch", "nl" },
            { "Portuguese_European", "pt-PT" },
            { "Portuguese_Brazilian", "pt-BR" },
            { "Chinese", "zh-CN" }, { "Chinese_Trad", "zh-TW" },
            { "Japanese", "ja" }, { "Korean", "ko" }, { "Hindi", "hi" },
            { "Thai", "th" }, { "Croatian", "hr" }, { "Georgian", "ka" },
            { "Greek", "el" }, { "Serbian", "sr" }, { "Ukrainian", "uk" },
            { "Latvian", "lv" }
        };
        private static readonly Dictionary<string, ResourceManager> Managers =
            new Dictionary<string, ResourceManager>(StringComparer.Ordinal);
        private static readonly string[] CategoryKeys =
        {
            "CategoryRawMaterials", "CategoryValuables", "CategoryCookedFood",
            "CategoryPotions", "CategoryWeapons", "CategoryShields", "CategoryArmor",
            "CategoryAccessories", "CategoryTools", "CategoryArrows", "CategoryFishing",
            "CategoryTrophies", "CategoryMiscellaneous", "CategoryRawMeat",
            "CategoryRawFood", "CategoryFeasts", "CategoryCookedMeat", "CategoryWood",
            "CategoryHidesAndFurs", "CategoryStone", "CategoryFireworks",
            "CategorySeeds", "CategoryOresAndMetals", "CategoryBlobBombs",
            "CategoryClothing"
        };
        internal static int CategoryCount => CategoryKeys.Length;
        internal static string SelectedLanguage =>
            Localization.instance?.GetSelectedLanguage() ?? "English";

        // Returns the culture used to sort labels in the selected language.
        internal static CompareInfo Comparison()
        {
            string code = LanguageCodes.TryGetValue(SelectedLanguage, out string value) ?
                value : "en-US";
            return CultureInfo.GetCultureInfo(code).CompareInfo;
        }

        // Returns a translated interface label with English as the fallback.
        internal static string Get(string key)
        {
            return Lookup("UI", key, SelectedLanguage);
        }

        // Returns a translated error with English as the fallback.
        internal static string Error(string key)
        {
            return Lookup("Errors", key, SelectedLanguage);
        }

        // Returns a category label in a named Valheim language.
        internal static string Category(int index, string language)
        {
            return CategoryText(CategoryKeys[index], language);
        }

        // Returns a category or family label in a named Valheim language.
        internal static string CategoryText(string key, string language)
        {
            return Lookup("Categories", key, language);
        }

        // Loads an embedded language value, falling back to English.
        private static string Lookup(string group, string key, string language)
        {
            string name = "Landoria.NearbyStorage.Resources." + group;
            string english = Manager(name).GetString(key, CultureInfo.InvariantCulture);
            if (!LanguageCodes.TryGetValue(language, out string code))
            {
                return english ?? key;
            }
            string translated = Manager(name + "." + code)
                .GetString(key, CultureInfo.InvariantCulture);
            return translated ?? english ?? key;
        }

        // Reuses a resource manager for an embedded resource set.
        private static ResourceManager Manager(string name)
        {
            if (!Managers.TryGetValue(name, out ResourceManager manager))
            {
                manager = new ResourceManager(name, typeof(ModText).Assembly);
                Managers[name] = manager;
            }
            return manager;
        }
    }
}
