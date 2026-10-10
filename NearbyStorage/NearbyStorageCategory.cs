using System;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Maps Valheim item types to the nearby storage panel's fixed categories.
    internal static class NearbyStorageCategory
    {
        private static readonly HashSet<string> CraftedMaterials =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> FireCookingInputs =
            new HashSet<string>(StringComparer.Ordinal);
        private static ObjectDB _cachedDb;
        private static ZNetScene _cachedScene;
        private static int _recipeCount;
        private static int _prefabCount;
        internal static readonly string[] French = { "Matières premières", "Précieux", "Nourriture",
            "Boissons", "Armes", "Boucliers", "Armures", "Accessoires",
            "Outils", "Munitions", "Poissons", "Trophées", "Divers", "Matériaux",
            "Viande crue" };
        internal static readonly string[] English = { "Raw Materials", "Valuables", "Food",
            "Drinks", "Weapons", "Shields", "Armor", "Accessories", "Tools",
            "Ammunition", "Fish", "Trophies", "Miscellaneous", "Materials",
            "Raw Meat" };

        // Returns the label for the active Valheim language.
        internal static string Label(int index)
        {
            if (index == -1)
            {
                return Localization.instance != null &&
                    Localization.instance.GetSelectedLanguage() == "French" ? "Tout" : "All";
            }
            return Localization.instance != null && Localization.instance.GetSelectedLanguage() == "French" ?
                French[index] : English[index];
        }

        // Assigns every item type to one category.
        internal static int For(ItemDrop.ItemData item)
        {
            if (item.m_shared.m_value > 0)
            {
                return 1;
            }
            if (item.m_dropPrefab != null)
            {
                RefreshMaterialSources();
                if (FireCookingInputs.Contains(item.m_dropPrefab.name)) { return 14; }
            }
            switch (item.m_shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.Material:
                    if (item.m_dropPrefab != null &&
                        item.m_dropPrefab.name.StartsWith("Feast", StringComparison.Ordinal) &&
                        item.m_dropPrefab.name.EndsWith("_Material", StringComparison.Ordinal))
                    {
                        return 2;
                    }
                    RefreshMaterialSources();
                    return item.m_dropPrefab != null &&
                        CraftedMaterials.Contains(item.m_dropPrefab.name) ? 13 : 0;
                case ItemDrop.ItemData.ItemType.Consumable:
                    return item.m_shared.m_isDrink ? 3 : 2;
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                case ItemDrop.ItemData.ItemType.Bow: return 4;
                case ItemDrop.ItemData.ItemType.Shield: return 5;
                case ItemDrop.ItemData.ItemType.Helmet:
                case ItemDrop.ItemData.ItemType.Chest:
                case ItemDrop.ItemData.ItemType.Legs:
                case ItemDrop.ItemData.ItemType.Hands:
                case ItemDrop.ItemData.ItemType.Shoulder: return 6;
                case ItemDrop.ItemData.ItemType.Utility:
                case ItemDrop.ItemData.ItemType.Trinket: return 7;
                case ItemDrop.ItemData.ItemType.Tool:
                case ItemDrop.ItemData.ItemType.Torch: return 8;
                case ItemDrop.ItemData.ItemType.Ammo:
                case ItemDrop.ItemData.ItemType.AmmoNonEquipable: return 9;
                case ItemDrop.ItemData.ItemType.Fish: return 10;
                case ItemDrop.ItemData.ItemType.Trophy: return 11;
                default: return 12;
            }
        }

        // Rebuilds material and fire cooking lists when game content changes.
        private static void RefreshMaterialSources()
        {
            ObjectDB db = ObjectDB.instance;
            ZNetScene scene = ZNetScene.instance;
            int recipes = db?.m_recipes?.Count ?? 0;
            int prefabs = scene?.m_prefabs?.Count ?? 0;
            if (db == _cachedDb && scene == _cachedScene && recipes == _recipeCount &&
                prefabs == _prefabCount) { return; }
            _cachedDb = db;
            _cachedScene = scene;
            _recipeCount = recipes;
            _prefabCount = prefabs;
            CraftedMaterials.Clear();
            FireCookingInputs.Clear();
            AddRecipeOutputs(db);
            AddConversionOutputs(scene);
            AddFireCookingInputs(scene);
        }

        // Adds material items produced by crafting recipes.
        private static void AddRecipeOutputs(ObjectDB db)
        {
            if (db == null) { return; }
            foreach (Recipe recipe in db.m_recipes)
            {
                ItemDrop output = recipe?.m_item;
                if (output != null && output.m_itemData.m_shared.m_itemType ==
                    ItemDrop.ItemData.ItemType.Material)
                {
                    CraftedMaterials.Add(output.gameObject.name);
                }
            }
        }

        // Adds material items produced by smelters and similar processors.
        private static void AddConversionOutputs(ZNetScene scene)
        {
            if (scene == null) { return; }
            foreach (GameObject prefab in scene.m_prefabs)
            {
                Smelter smelter = prefab == null ? null : prefab.GetComponent<Smelter>();
                if (smelter == null) { continue; }
                foreach (Smelter.ItemConversion conversion in smelter.m_conversion)
                {
                    ItemDrop output = conversion?.m_to;
                    if (output != null && output.m_itemData.m_shared.m_itemType ==
                        ItemDrop.ItemData.ItemType.Material)
                    {
                        CraftedMaterials.Add(output.gameObject.name);
                    }
                }
            }
        }

        // Finds inputs for stations that cook over a fire.
        private static void AddFireCookingInputs(ZNetScene scene)
        {
            if (scene == null) { return; }
            foreach (GameObject prefab in scene.m_prefabs)
            {
                CookingStation station = prefab == null ? null :
                    prefab.GetComponent<CookingStation>();
                if (station == null || !station.m_requireFire) { continue; }
                foreach (CookingStation.ItemConversion conversion in station.m_conversion)
                {
                    ItemDrop input = conversion?.m_from;
                    if (input != null) { FireCookingInputs.Add(input.gameObject.name); }
                }
            }
        }
    }
}
