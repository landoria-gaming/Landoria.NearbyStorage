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
        private static readonly HashSet<string> PreparedFoods =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> FeastItems =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> FireCookingInputs =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> FireCookingOutputs =
            new HashSet<string>(StringComparer.Ordinal);
        private static ObjectDB _cachedDb;
        private static ZNetScene _cachedScene;
        private static int _recipeCount;
        private static int _prefabCount;
        internal static readonly string[] French = { "Matières premières", "Précieux", "Aliments cuisinés",
            "Boissons", "Armes", "Boucliers", "Armures", "Accessoires",
            "Outils", "Munitions", "Poissons", "Trophées", "Divers", "Matériaux",
            "Viande crue", "Aliments crus", "Festins", "Viande cuite" };
        internal static readonly string[] English = { "Raw Materials", "Valuables", "Cooked Food",
            "Drinks", "Weapons", "Shields", "Armor", "Accessories", "Tools",
            "Ammunition", "Fish", "Trophies", "Miscellaneous", "Materials",
            "Raw Meat", "Raw Food", "Feasts", "Cooked Meat" };
        internal static readonly int[] DisplayOrder = { -1, 0, 13, 15, 2, 14, 17, 10, 16,
            3, 4, 6, 5, 9, 8, 7, 11, 12, 1 };

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
                if (FeastItems.Contains(item.m_dropPrefab.name)) { return 16; }
                if (FireCookingInputs.Contains(item.m_dropPrefab.name)) { return 14; }
                if (FireCookingOutputs.Contains(item.m_dropPrefab.name)) { return 17; }
            }
            switch (item.m_shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.Material:
                    RefreshMaterialSources();
                    return item.m_dropPrefab != null &&
                        CraftedMaterials.Contains(item.m_dropPrefab.name) ? 13 : 0;
                case ItemDrop.ItemData.ItemType.Consumable:
                    if (item.m_shared.m_isDrink) { return 3; }
                    RefreshMaterialSources();
                    return item.m_dropPrefab != null &&
                        PreparedFoods.Contains(item.m_dropPrefab.name) ? 2 : 15;
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
            PreparedFoods.Clear();
            FeastItems.Clear();
            FireCookingInputs.Clear();
            FireCookingOutputs.Clear();
            AddRecipeOutputs(db);
            AddConversionOutputs(scene);
            AddCookingStationItems(scene);
            AddFeastItems(scene);
        }

        // Adds material items produced by crafting recipes.
        private static void AddRecipeOutputs(ObjectDB db)
        {
            if (db == null) { return; }
            foreach (Recipe recipe in db.m_recipes)
            {
                ItemDrop output = recipe?.m_item;
                if (output == null) { continue; }
                if (output.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Material)
                {
                    CraftedMaterials.Add(output.gameObject.name);
                }
                else if (output.m_itemData.m_shared.m_itemType ==
                    ItemDrop.ItemData.ItemType.Consumable)
                {
                    PreparedFoods.Add(output.gameObject.name);
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

        // Finds food outputs and inputs cooked over a fire.
        private static void AddCookingStationItems(ZNetScene scene)
        {
            if (scene == null) { return; }
            foreach (GameObject prefab in scene.m_prefabs)
            {
                CookingStation station = prefab == null ? null :
                    prefab.GetComponent<CookingStation>();
                if (station == null) { continue; }
                foreach (CookingStation.ItemConversion conversion in station.m_conversion)
                {
                    ItemDrop input = conversion?.m_from;
                    ItemDrop output = conversion?.m_to;
                    if (station.m_requireFire && input != null)
                    {
                        FireCookingInputs.Add(input.gameObject.name);
                        if (output != null)
                        {
                            FireCookingOutputs.Add(output.gameObject.name);
                        }
                    }
                    if (output != null && output.m_itemData.m_shared.m_itemType ==
                        ItemDrop.ItemData.ItemType.Consumable)
                    {
                        PreparedFoods.Add(output.gameObject.name);
                    }
                }
            }
        }

        // Groups feast food and its matching placement item.
        private static void AddFeastItems(ZNetScene scene)
        {
            if (scene == null) { return; }
            foreach (GameObject prefab in scene.m_prefabs)
            {
                Feast feast = prefab == null ? null : prefab.GetComponent<Feast>();
                if (feast == null) { continue; }
                ItemDrop food = feast.m_foodItem ?? prefab.GetComponent<ItemDrop>();
                if (food == null) { continue; }
                FeastItems.Add(food.gameObject.name);
                Piece piece = prefab.GetComponent<Piece>();
                if (piece == null) { continue; }
                foreach (Piece.Requirement requirement in piece.m_resources)
                {
                    ItemDrop material = requirement?.m_resItem;
                    if (material != null && material.m_itemData.m_shared.m_name ==
                        food.m_itemData.m_shared.m_name)
                    {
                        FeastItems.Add(material.gameObject.name);
                    }
                }
            }
        }
    }
}
