using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Maps Valheim item types to the nearby storage panel's fixed categories.
    internal static class NearbyStorageCategory
    {
        private static readonly HashSet<string> PreparedFoods =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> FeastItems =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> FireCookingInputs =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> FireCookingOutputs =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> WoodItems = new HashSet<string>(StringComparer.Ordinal)
        {
            "Wood", "RoundLog", "FineWood", "ElderBark", "HardAntler",
            "Root", "WrithanRoots"
        };
        private static readonly HashSet<string> StoneItems = new HashSet<string>(StringComparer.Ordinal)
        {
            "Stone", "Flint", "StoneRock"
        };
        private static readonly HashSet<string> SeedItems = new HashSet<string>(StringComparer.Ordinal)
        {
            "CarrotSeeds", "TurnipSeeds", "OnionSeeds", "OatSeeds", "KaleSeeds",
            "PoteitrSeeds", "BeechSeeds", "BirchSeeds", "FirCone", "FirConeFrost",
            "PineCone", "Acorn", "AncientSeed", "Barley", "Flax",
            "VineGreenSeeds", "VineberrySeeds"
        };
        private static readonly HashSet<string> OreAndMetalItems =
            new HashSet<string>(StringComparer.Ordinal)
        {
            "CopperOre", "CopperScrap", "Copper", "TinOre", "Tin",
            "BronzeScrap", "Bronze", "IronOre", "IronScrap", "Iron",
            "SilverOre", "Silver", "BlackMetalScrap", "BlackMetal",
            "FlametalOre", "FlametalOreNew", "Flametal", "FlametalNew",
            "GoldOre", "Gold", "Chain", "BronzeNails", "IronNails", "Coal"
        };
        private static readonly HashSet<string> HidesAndFurs =
            new HashSet<string>(StringComparer.Ordinal)
        {
            "DeerHide", "WolfPelt", "LoxPelt", "SerpentScale", "TrollHide",
            "ScaleHide", "WolfHairBundle", "LeatherScraps", "BjornHide"
        };
        private static readonly HashSet<string> RawFoodMaterials =
            new HashSet<string>(StringComparer.Ordinal)
        {
            "Turnip", "BreadDough", "BarleyFlour", "OatFlour"
        };
        private static ObjectDB _cachedDb;
        private static ZNetScene _cachedScene;
        private static int _recipeCount;
        private static int _prefabCount;
        // Sorts category labels in the selected language, with All first and Miscellaneous last.
        internal static int[] DisplayOrder()
        {
            int[] order = new int[ModText.CategoryCount + 1];
            order[0] = -1;
            int next = 1;
            for (int index = 0; index < ModText.CategoryCount; index++)
            {
                if (index != 12) { order[next++] = index; }
            }
            order[next] = 12;
            CompareInfo comparison = CultureInfo.GetCultureInfo(ModText.IsFrench ? "fr-FR" : "en-US").CompareInfo;
            Array.Sort(order, 1, ModText.CategoryCount - 1, Comparer<int>.Create((left, right) =>
                comparison.Compare(Label(left), Label(right),
                    CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace)));
            return order;
        }

        // Returns the label for the active Valheim language.
        internal static string Label(int index)
        {
            if (index == -1)
            {
                return ModText.CategoryText("CategoryAll", ModText.IsFrench);
            }
            return ModText.Category(index, ModText.IsFrench);
        }

        // Assigns every item type to one category.
        internal static int For(ItemDrop.ItemData item)
        {
            if (item.m_dropPrefab != null &&
                RawFoodMaterials.Contains(item.m_dropPrefab.name))
            {
                return 14;
            }
            if (item.m_dropPrefab != null && item.m_dropPrefab.name == "Feaster")
            {
                return 2;
            }
            if (item.m_dropPrefab != null && item.m_dropPrefab.name == "HelmetDverger")
            {
                return 7;
            }
            if (item.m_dropPrefab != null &&
                (item.m_dropPrefab.name == "LinenThread" ||
                item.m_dropPrefab.name == "JuteBlue" ||
                item.m_dropPrefab.name == "JuteRed"))
            {
                return 24;
            }
            if (item.m_dropPrefab != null && item.m_dropPrefab.name == "SurtlingCore")
            {
                return 1;
            }
            if (item.m_dropPrefab != null &&
                (item.m_dropPrefab.name.StartsWith("MeadBase", StringComparison.Ordinal) ||
                item.m_dropPrefab.name == "BarleyWineBase"))
            {
                return 3;
            }
            if (item.m_dropPrefab != null && WoodItems.Contains(item.m_dropPrefab.name))
            {
                return 17;
            }
            if (item.m_dropPrefab != null && StoneItems.Contains(item.m_dropPrefab.name))
            {
                return 19;
            }
            if (item.m_dropPrefab != null && SeedItems.Contains(item.m_dropPrefab.name))
            {
                return 21;
            }
            if (item.m_dropPrefab != null && OreAndMetalItems.Contains(item.m_dropPrefab.name))
            {
                return 22;
            }
            if (item.m_dropPrefab != null && HidesAndFurs.Contains(item.m_dropPrefab.name))
            {
                return 18;
            }
            if (item.m_dropPrefab != null &&
                (item.m_dropPrefab.name.StartsWith("FishingBait", StringComparison.Ordinal) ||
                item.m_dropPrefab.name == "FishingRod"))
            {
                return 10;
            }
            if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Fish) { return 10; }
            if (item.m_dropPrefab != null &&
                (item.m_dropPrefab.name.StartsWith("FireworksRocket_", StringComparison.Ordinal) ||
                item.m_dropPrefab.name == "Sparkler"))
            {
                return 20;
            }
            if (item.m_dropPrefab != null &&
                (item.m_dropPrefab.name.StartsWith("Bomb", StringComparison.Ordinal) ||
                item.m_dropPrefab.name == "BlobVial"))
            {
                return 23;
            }
            if (item.m_dropPrefab != null &&
                (item.m_dropPrefab.name.StartsWith("Arrow", StringComparison.Ordinal) ||
                item.m_dropPrefab.name.StartsWith("Bolt", StringComparison.Ordinal) ||
                item.m_dropPrefab.name.StartsWith("TurretBolt", StringComparison.Ordinal) ||
                item.m_dropPrefab.name.StartsWith("Snowball", StringComparison.Ordinal)))
            {
                return 9;
            }
            if (item.m_shared.m_value > 0)
            {
                return 1;
            }
            if (item.m_dropPrefab != null)
            {
                RefreshCategorySources();
                if (FeastItems.Contains(item.m_dropPrefab.name)) { return 15; }
                if (FireCookingInputs.Contains(item.m_dropPrefab.name)) { return 13; }
                if (FireCookingOutputs.Contains(item.m_dropPrefab.name)) { return 16; }
                if (item.m_dropPrefab.name == "ScytheHandle" ||
                    item.m_dropPrefab.name == "SharpeningStone") { return 8; }
            }
            if (item.m_shared.m_skillType == Skills.SkillType.Pickaxes)
            {
                return 8;
            }
            switch (item.m_shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.Material:
                    return 0;
                case ItemDrop.ItemData.ItemType.Consumable:
                    if (item.m_shared.m_isDrink) { return 3; }
                    RefreshCategorySources();
                    return item.m_dropPrefab != null &&
                        PreparedFoods.Contains(item.m_dropPrefab.name) ? 2 : 14;
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                    return item.m_shared.m_skillType == Skills.SkillType.Axes ? 8 : 4;
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                case ItemDrop.ItemData.ItemType.Bow: return 4;
                case ItemDrop.ItemData.ItemType.Shield: return 5;
                case ItemDrop.ItemData.ItemType.Helmet:
                case ItemDrop.ItemData.ItemType.Chest:
                case ItemDrop.ItemData.ItemType.Legs:
                case ItemDrop.ItemData.ItemType.Hands:
                case ItemDrop.ItemData.ItemType.Shoulder:
                    return Mathf.Approximately(item.GetArmor(), 1f) ? 24 : 6;
                case ItemDrop.ItemData.ItemType.Utility:
                case ItemDrop.ItemData.ItemType.Trinket: return 7;
                case ItemDrop.ItemData.ItemType.Tool:
                case ItemDrop.ItemData.ItemType.Torch: return 8;
                case ItemDrop.ItemData.ItemType.Ammo:
                case ItemDrop.ItemData.ItemType.AmmoNonEquipable: return 9;
                case ItemDrop.ItemData.ItemType.Trophy: return 11;
                default: return 12;
            }
        }

        // Rebuilds food and feast lists when game content changes.
        private static void RefreshCategorySources()
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
            PreparedFoods.Clear();
            FeastItems.Clear();
            FireCookingInputs.Clear();
            FireCookingOutputs.Clear();
            AddRecipeOutputs(db);
            AddCookingStationItems(scene);
            AddFeastItems(scene);
        }

        // Adds consumable items produced by crafting recipes.
        private static void AddRecipeOutputs(ObjectDB db)
        {
            if (db == null) { return; }
            foreach (Recipe recipe in db.m_recipes)
            {
                ItemDrop output = recipe?.m_item;
                if (output == null) { continue; }
                if (output.m_itemData.m_shared.m_itemType ==
                    ItemDrop.ItemData.ItemType.Consumable)
                {
                    PreparedFoods.Add(output.gameObject.name);
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
