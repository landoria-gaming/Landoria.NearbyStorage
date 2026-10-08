using System;
using System.Collections.Generic;

namespace Landoria.NearbyStorage
{
    // Finds item families from the crafting stations in Valheim's recipes.
    internal static class NearbyStorageRecipeFamily
    {
        private static readonly Dictionary<string, HashSet<string>> Stations =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        private static ObjectDB _database;
        private static int _recipeCount = -1;

        // Groups items of the same type crafted at a shared station.
        internal static bool SameStationFamily(ItemDrop.ItemData left, ItemDrop.ItemData right)
        {
            if (left?.m_shared == null || right?.m_shared == null ||
                left.m_shared.m_itemType != right.m_shared.m_itemType)
            {
                return false;
            }
            HashSet<string> leftStations = StationsFor(left);
            HashSet<string> rightStations = StationsFor(right);
            if (leftStations == null || rightStations == null) { return false; }
            foreach (string station in leftStations)
            {
                if (rightStations.Contains(station)) { return true; }
            }
            return false;
        }

        // Gets recipe stations by the item's internal Valheim name.
        private static HashSet<string> StationsFor(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null || ObjectDB.instance == null) { return null; }
            ObjectDB database = ObjectDB.instance;
            if (_database != database || _recipeCount != database.m_recipes.Count)
            {
                Refresh(database);
            }
            return Stations.TryGetValue(item.m_shared.m_name, out HashSet<string> stations) ?
                stations : null;
        }

        // Refreshes the lookup after the game or mods replace the recipe list.
        private static void Refresh(ObjectDB database)
        {
            Stations.Clear();
            foreach (Recipe recipe in database.m_recipes)
            {
                string name = recipe?.m_item?.m_itemData?.m_shared?.m_name;
                string station = recipe?.m_craftingStation?.m_name;
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(station)) { continue; }
                if (!Stations.TryGetValue(name, out HashSet<string> names))
                {
                    names = new HashSet<string>(StringComparer.Ordinal);
                    Stations.Add(name, names);
                }
                names.Add(station);
            }
            _database = database;
            _recipeCount = database.m_recipes.Count;
        }
    }
}
