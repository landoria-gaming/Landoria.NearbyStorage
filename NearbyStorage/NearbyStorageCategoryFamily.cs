using System.Collections.Generic;

namespace Landoria.NearbyStorage
{
    // Groups nearby storage categories into broad naming families.
    internal static class NearbyStorageCategoryFamily
    {
        private static readonly string[] French =
        {
            "Équipement", "Ressources", "Cuisine et boissons",
            "Combat et activités", "Objets spéciaux"
        };
        private static readonly string[] English =
        {
            "Equipment", "Resources", "Food and Drinks",
            "Combat and Activities", "Special Items"
        };
        private static readonly int[] ByCategory =
        {
            1, 4, 2, 2, 0, 0, 0, 0, 0, 3, 3, 4, 4,
            2, 2, 2, 2, 1, 1, 1, 3, 1, 1, 3, 0
        };

        // Returns the family assigned to a category.
        internal static int For(int category)
        {
            return ByCategory[category];
        }

        // Returns the French and English names for a category's family.
        internal static IEnumerable<string> Names(int category)
        {
            int family = For(category);
            yield return French[family];
            yield return English[family];
        }
    }
}
