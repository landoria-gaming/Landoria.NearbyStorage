using System;
using System.Collections.Generic;

namespace Landoria.NearbyStorage
{
    // Maps Valheim item types to the stock browser's fixed categories.
    internal static class NearbyStorageCategory
    {
        private static readonly HashSet<string> Valuables = new HashSet<string>(StringComparer.Ordinal)
        {
            "Amber", "AmberPearl", "AncientCoin", "AncientGemstoneBlack",
            "AncientGemstoneGreen", "AncientGemstoneOrange", "AncientGemstonePurple",
            "Coins", "Ruby", "SilverNecklace", "GemstoneBlue", "GemstoneGreen",
            "GemstoneRed"
        };
        internal static readonly string[] French = { "Matériaux", "Précieux", "Nourriture",
            "Boissons", "Armes", "Boucliers", "Armures", "Accessoires",
            "Outils", "Munitions", "Poissons", "Trophées", "Divers" };
        internal static readonly string[] English = { "Materials", "Valuables", "Food",
            "Drinks", "Weapons", "Shields", "Armor", "Accessories", "Tools",
            "Ammunition", "Fish", "Trophies", "Miscellaneous" };

        // Returns the label for the active Valheim language.
        internal static string Label(int index)
        {
            return Localization.instance != null && Localization.instance.GetSelectedLanguage() == "French" ?
                French[index] : English[index];
        }

        // Assigns every item type to one category.
        internal static int For(ItemDrop.ItemData item)
        {
            if (item?.m_dropPrefab != null && Valuables.Contains(item.m_dropPrefab.name))
            {
                return 1;
            }
            switch (item.m_shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.Material: return 0;
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
    }
}
