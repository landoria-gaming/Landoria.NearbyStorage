using System.Globalization;
using System.Resources;

namespace Landoria.NearbyStorage
{
    // Reads embedded English and French text for the selected Valheim language.
    internal static class ModText
    {
        private static readonly ResourceManager English = new ResourceManager(
            "Landoria.NearbyStorage.Resources.UI", typeof(ModText).Assembly);
        private static readonly ResourceManager French = new ResourceManager(
            "Landoria.NearbyStorage.Resources.UI.fr", typeof(ModText).Assembly);
        private static readonly ResourceManager EnglishCategories = new ResourceManager(
            "Landoria.NearbyStorage.Resources.Categories", typeof(ModText).Assembly);
        private static readonly ResourceManager FrenchCategories = new ResourceManager(
            "Landoria.NearbyStorage.Resources.Categories.fr", typeof(ModText).Assembly);
        private static readonly ResourceManager EnglishErrors = new ResourceManager(
            "Landoria.NearbyStorage.Resources.Errors", typeof(ModText).Assembly);
        private static readonly ResourceManager FrenchErrors = new ResourceManager(
            "Landoria.NearbyStorage.Resources.Errors.fr", typeof(ModText).Assembly);
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
        internal static bool IsFrench => Localization.instance?.GetSelectedLanguage() == "French";
        internal static int CategoryCount => CategoryKeys.Length;

        // Returns the selected translation with English as the fallback.
        internal static string Get(string key)
        {
            string value = IsFrench ? French.GetString(key, CultureInfo.InvariantCulture) : null;
            return value ?? English.GetString(key, CultureInfo.InvariantCulture) ?? key;
        }

        // Returns an error message with English as the fallback.
        internal static string Error(string key)
        {
            string value = IsFrench ? FrenchErrors.GetString(key, CultureInfo.InvariantCulture) : null;
            return value ?? EnglishErrors.GetString(key, CultureInfo.InvariantCulture) ?? key;
        }

        // Returns the translated label of a numbered category.
        internal static string Category(int index, bool french)
        {
            return CategoryText(CategoryKeys[index], french);
        }

        // Returns a category or family label in the requested language.
        internal static string CategoryText(string key, bool french)
        {
            string value = french ? FrenchCategories.GetString(key, CultureInfo.InvariantCulture) : null;
            return value ?? EnglishCategories.GetString(key, CultureInfo.InvariantCulture) ?? key;
        }
    }
}
