using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Shows nearby container contents below the player's inventory.
    internal static partial class NearbyStockDialog
    {
        private static RectTransform _root;
        private static RectTransform _cards;
        private static RectTransform _tooltip;
        private static TextMeshProUGUI _tooltipText;
        private static TMP_InputField _filter;
        private static InventoryGui _gui;
        private static float _nextRefresh;
        private static int _category;
        private static string _fingerprint;
        private static bool _dirtyView = true;
        private static bool _hasStock;
        private static string _dragKey;

        internal static bool FilterFocused => _root != null && _root.gameObject.activeInHierarchy &&
            _filter != null && _filter.isActiveAndEnabled && _filter.isFocused;
        internal static bool IsOpen => _root != null && _root.gameObject.activeInHierarchy && _hasStock;

        // Creates, positions, and refreshes the stock browser while Tab is open.
        internal static void Update()
        {
            InventoryGui gui = InventoryGui.instance;
            if (gui == null || Player.m_localPlayer == null || !InventoryGui.IsVisible() ||
                StorageLocator.CurrentContainer() != null)
            {
                if (_root != null) { _root.gameObject.SetActive(false); }
                _nextRefresh = 0f;
                return;
            }
            if (_root == null || _gui != gui)
            {
                Dispose();
                Create(gui);
            }
            Position();
            string dragKey = NearbyStockTransfer.DraggedPlayerItemKey();
            if (_dragKey != dragKey)
            {
                _dragKey = dragKey;
                RefreshSoon();
            }
            if (Time.unscaledTime >= _nextRefresh)
            {
                Refresh();
            }
            _root.gameObject.SetActive(_hasStock);
            if (_hasStock)
            {
                int inventoryIndex = gui.m_inventoryRoot.GetSiblingIndex();
                int stockIndex = _root.GetSiblingIndex();
                bool splitOpen = gui.m_splitDialog?.IsActive == true;
                if ((splitOpen && stockIndex > inventoryIndex) ||
                    (!splitOpen && stockIndex < inventoryIndex))
                {
                    _root.SetSiblingIndex(inventoryIndex);
                }
                PositionTooltip();
                if (_filter != null && !_filter.isFocused && ZInput.GetKeyDown(KeyCode.F))
                {
                    _filter.Select();
                    _filter.ActivateInputField();
                }
            }
        }

        // Schedules an immediate refresh after a selection or inventory move.
        internal static void RefreshSoon()
        {
            _nextRefresh = 0f;
            _dirtyView = true;
        }

        // Filters a fresh snapshot before rebuilding visible cards.
        private static void Refresh()
        {
            _nextRefresh = Time.unscaledTime + 0.5f;
            if (_cards == null || Player.m_localPlayer == null) { return; }
            List<NearbyStockItem> items = NearbyStockCatalog.Read(Player.m_localPlayer);
            _hasStock = items.Count > 0;
            string filter = _dragKey != null || _filter == null ? "" : _filter.text.Trim();
            bool[] occupied = new bool[NearbyStockCategory.French.Length];
            foreach (NearbyStockItem item in items)
            {
                if ((_dragKey != null && item.Key != _dragKey) ||
                    item.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) < 0)
                {
                    continue;
                }
                occupied[0] = true;
                occupied[NearbyStockCategory.For(item.Sample.m_shared.m_itemType)] = true;
            }
            UpdateCategoryOpacity(occupied);
            items.RemoveAll(item => _dragKey != null ? item.Key != _dragKey :
                (_category != 0 && NearbyStockCategory.For(item.Sample.m_shared.m_itemType) != _category) ||
                item.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) < 0);
            items.Sort(CompareByName);
            string fingerprint = Fingerprint(items);
            if (!_dirtyView && fingerprint == _fingerprint) { return; }
            _fingerprint = fingerprint;
            _dirtyView = false;
            HideTooltip();
            DrawCards(items);
        }

        // Dims categories with no matching nearby items.
        private static void UpdateCategoryOpacity(bool[] occupied)
        {
            Transform list = _root?.Find("Categories");
            if (list == null) { return; }
            int position = 0;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < occupied.Length; i++)
                {
                    if (occupied[i] != (pass == 0)) { continue; }
                    RectTransform row = list.Find("Category " + i) as RectTransform;
                    if (row != null) { row.anchoredPosition = new Vector2(0f, -position++ * 25f); }
                }
            }
            for (int i = 0; i < occupied.Length; i++)
            {
                TextMeshProUGUI label = list.Find("Category " + i)?.Find("Text")?
                    .GetComponent<TextMeshProUGUI>();
                if (label == null) { continue; }
                Color color = label.color;
                color.a = occupied[i] ? 1f : 0.4f;
                label.color = color;
            }
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

        // Keeps item order predictable without a sorting control.
        private static int CompareByName(NearbyStockItem a, NearbyStockItem b)
        {
            int names = string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
            return names != 0 ? names : string.CompareOrdinal(a.Key, b.Key);
        }

        // Destroys the transient UI without changing stored items.
        internal static void Dispose()
        {
            if (_root != null) { UnityEngine.Object.Destroy(_root.gameObject); }
            _root = null;
            _cards = null;
            _tooltip = null;
            _tooltipText = null;
            _filter = null;
            _gui = null;
            _fingerprint = null;
            _dirtyView = true;
            _hasStock = false;
            _dragKey = null;
        }
    }
}
