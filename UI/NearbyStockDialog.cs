using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.SuperStorage
{
    // Shows nearby container contents below the player's inventory.
    internal static partial class NearbyStockDialog
    {
        private static readonly string[] SortNamesFr = { "Nom ↑", "Nom ↓", "Quantité ↑", "Quantité ↓" };
        private static readonly string[] SortNamesEn = { "Name ↑", "Name ↓", "Quantity ↑", "Quantity ↓" };
        private static RectTransform _root;
        private static RectTransform _cards;
        private static RectTransform _tooltip;
        private static TextMeshProUGUI _tooltipText;
        private static TextMeshProUGUI _sortText;
        private static TMP_InputField _filter;
        private static GameObject _sortOptions;
        private static InventoryGui _gui;
        private static float _nextRefresh;
        private static int _category;
        private static int _sort;
        private static string _fingerprint;
        private static bool _dirtyView = true;

        // Creates, positions, and refreshes the stock browser while Tab is open.
        internal static void Update()
        {
            InventoryGui gui = InventoryGui.instance;
            if (gui == null || Player.m_localPlayer == null || !InventoryGui.IsVisible())
            {
                if (_root != null) { _root.gameObject.SetActive(false); }
                return;
            }
            if (_root == null || _gui != gui)
            {
                Dispose();
                Create(gui);
            }
            _root.gameObject.SetActive(true);
            Position();
            PositionTooltip();
            if (Time.unscaledTime >= _nextRefresh)
            {
                Refresh();
            }
        }

        // Schedules an immediate refresh after a selection or inventory move.
        internal static void RefreshSoon()
        {
            _nextRefresh = 0f;
            _dirtyView = true;
        }

        // Filters and sorts a fresh snapshot before rebuilding visible cards.
        private static void Refresh()
        {
            _nextRefresh = Time.unscaledTime + 0.5f;
            if (_cards == null || Player.m_localPlayer == null) { return; }
            List<NearbyStockItem> items = NearbyStockCatalog.Read(Player.m_localPlayer);
            string filter = _filter == null ? "" : _filter.text.Trim();
            items.RemoveAll(item => (_category != 0 &&
                NearbyStockCategory.For(item.Sample.m_shared.m_itemType) != _category) ||
                item.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) < 0);
            items.Sort(Compare);
            string fingerprint = Fingerprint(items);
            if (!_dirtyView && fingerprint == _fingerprint) { return; }
            _fingerprint = fingerprint;
            _dirtyView = false;
            HideTooltip();
            DrawCards(items);
        }

        // Tracks visible quantities and source locations to avoid redraw flicker.
        private static string Fingerprint(List<NearbyStockItem> items)
        {
            var value = new StringBuilder();
            foreach (NearbyStockItem item in items)
            {
                value.Append(item.Key).Append(':').Append(item.Count).Append('|');
                foreach (NearbyStockSource source in item.Sources)
                {
                    if (source.Chest != null)
                    {
                        value.Append(source.Chest.GetInstanceID()).Append(':')
                            .Append(source.Count).Append(',');
                    }
                }
            }
            return value.ToString();
        }

        // Applies the selected name or quantity order with stable tie breaking.
        private static int Compare(NearbyStockItem a, NearbyStockItem b)
        {
            int names = string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
            int amount = a.Count.CompareTo(b.Count);
            if (_sort == 0) { return names; }
            if (_sort == 1) { return -names; }
            return _sort == 2 ? (amount != 0 ? amount : names) :
                (amount != 0 ? -amount : names);
        }

        // Returns a category or sort label in French or English.
        private static string SortLabel(int index)
        {
            bool french = Localization.instance != null &&
                Localization.instance.GetSelectedLanguage() == "French";
            return french ? SortNamesFr[index] : SortNamesEn[index];
        }

        // Destroys the transient UI without changing stored items.
        internal static void Dispose()
        {
            if (_root != null) { UnityEngine.Object.Destroy(_root.gameObject); }
            _root = null;
            _cards = null;
            _tooltip = null;
            _tooltipText = null;
            _sortText = null;
            _filter = null;
            _sortOptions = null;
            _gui = null;
            _fingerprint = null;
            _dirtyView = true;
        }
    }
}
