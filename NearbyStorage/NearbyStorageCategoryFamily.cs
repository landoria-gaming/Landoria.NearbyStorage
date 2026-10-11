using System.Collections.Generic;

namespace Landoria.NearbyStorage
{
    // Groups nearby storage categories into broad naming families.
    internal static class NearbyStorageCategoryFamily
    {
        private static readonly int[] ByCategory =
        {
            1, 4, 2, 2, 0, 0, 0, 0, 0, 3, 3, 4, 4,
            2, 2, 2, 2, 1, 1, 1, 3, 1, 1, 3, 0
        };
        private static readonly string[] FamilyKeys =
        {
            "FamilyEquipment", "FamilyResources", "FamilyFoodAndDrinks",
            "FamilyCombatAndActivities", "FamilySpecialItems"
        };

        internal static int Count => FamilyKeys.Length;

        // Returns a family label in the active game language.
        internal static string Label(int family)
        {
            return ModText.CategoryText(FamilyKeys[family], ModText.SelectedLanguage);
        }

        // Returns a family label in a requested game language.
        internal static string Label(int family, string language)
        {
            return ModText.CategoryText(FamilyKeys[family], language);
        }

        // Returns hidden search aliases for a family in one language.
        internal static string Aliases(int family, string language)
        {
            return ModText.Aliases(FamilyKeys[family], language);
        }

        // Returns the family assigned to a category.
        internal static int For(int category)
        {
            return ByCategory[category];
        }

        // Returns the selected, English, and French names for a category's family.
        internal static IEnumerable<string> Names(int category)
        {
            int family = For(category);
            string key = FamilyKeys[family];
            yield return ModText.CategoryText(key, ModText.SelectedLanguage);
            yield return ModText.CategoryText(key, "English");
            yield return ModText.CategoryText(key, "French");
        }
    }
}
