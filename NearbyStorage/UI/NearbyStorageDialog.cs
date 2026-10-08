using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Shows nearby container contents below the player's inventory.
    internal static partial class NearbyStorageDialog
    {
        private static RectTransform _root;
        private static RectTransform _cards;
        private static RectTransform _categoryContent;
        private static RectTransform _tooltip;
        private static RectTransform _dragCancelOverlay;
        private static TMP_InputField _filter;
        private static InventoryGui _gui;
        private static float _nextRefresh;
        private static int _category;
        private static string _fingerprint;
        private static bool _dirtyView = true;
        private static bool _hasNearbyItems;
        private static string _dragKey;
        private static string _scrollToDragKey;
        private static string _scrollToSimilarName;
        private static bool _scrollCategoryToSelection;
        private static bool _selectFirstCategory = true;
        private static string _nearbyDragKey;

        internal static bool FilterFocused => _root != null && _root.gameObject.activeInHierarchy &&
            _filter != null && _filter.isActiveAndEnabled && _filter.isFocused;
        internal static bool IsOpen => _root != null && _root.gameObject.activeInHierarchy && _hasNearbyItems;

        // Selects and reveals the item targeted by an inventory Ctrl-click.
        internal static void FocusInventoryItem(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null || _root == null) { return; }
            SelectCategory(NearbyStorageCategory.For(item));
            _scrollToDragKey = NearbyStorageItemKey.For(item);
            _scrollToSimilarName = Localization.instance.Localize(item.m_shared.m_name);
            RefreshSoon();
        }

        // Creates, positions, and refreshes nearby storage while Tab is open.
        internal static void Update()
        {
            InventoryGui gui = InventoryGui.instance;
            if (gui == null || Player.m_localPlayer == null || !InventoryGui.IsVisible() ||
                StorageLocator.CurrentContainer() != null)
            {
                if (_root != null) { _root.gameObject.SetActive(false); }
                HideTooltip();
                _nextRefresh = 0f;
                return;
            }
            if (_root == null || _gui != gui)
            {
                Dispose();
                Create(gui);
            }
            if (!_root.gameObject.activeSelf && _filter != null &&
                !string.IsNullOrEmpty(_filter.text))
            {
                _filter.SetTextWithoutNotify("");
                RefreshSoon();
            }
            Position();
            UpdateDragFocus();
            if (Time.unscaledTime >= _nextRefresh) { Refresh(); }
            _root.gameObject.SetActive(_hasNearbyItems);
            if (_hasNearbyItems) { UpdateVisibleDialog(gui); }
        }

        // Tracks inventory and nearby storage drags for the visible grid.
        private static void UpdateDragFocus()
        {
            string dragKey = NearbyStorageTransfer.DraggedPlayerItemKey();
            if (_dragKey != dragKey)
            {
                if (dragKey != null)
                {
                    ItemDrop.ItemData dragged = NearbyStorageTransfer.DraggedPlayerItem();
                    if (dragged != null)
                    {
                        _filter?.SetTextWithoutNotify("");
                        SelectCategory(NearbyStorageCategory.For(dragged), false);
                    }
                }
                _dragKey = dragKey;
                if (dragKey != null)
                {
                    _scrollToDragKey = dragKey;
                    _scrollToSimilarName = null;
                }
                RefreshSoon();
            }
            string nearbyDragKey = NearbyStorageTransfer.DraggedNearbyItemKey();
            if (_nearbyDragKey != nearbyDragKey)
            {
                _nearbyDragKey = nearbyDragKey;
                RefreshSoon();
            }
        }

        // Keeps nearby items clickable while placing the split dialog in front.
        private static void UpdateVisibleDialog(InventoryGui gui)
        {
            if (_dragCancelOverlay != null)
            {
                _dragCancelOverlay.gameObject.SetActive(_nearbyDragKey != null);
            }
            int inventoryIndex = gui.m_inventoryRoot.GetSiblingIndex();
            int dialogIndex = _root.GetSiblingIndex();
            bool splitOpen = gui.m_splitDialog?.IsActive == true;
            if ((splitOpen && dialogIndex > inventoryIndex) ||
                (!splitOpen && dialogIndex < inventoryIndex))
            {
                _root.SetSiblingIndex(inventoryIndex);
            }
            UpdateTooltipHover();
            PositionTooltip();
            if (_filter != null && !_filter.isFocused && ZInput.GetKeyDown(KeyCode.F))
            {
                _filter.Select();
                _filter.ActivateInputField();
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
            List<NearbyStorageItem> items = NearbyStorageCatalog.Read(Player.m_localPlayer);
            _hasNearbyItems = items.Count > 0;
            bool unmatchedDrag = _dragKey != null &&
                !items.Exists(item => item.Key == _dragKey);
            bool categoryHasItems = _category >= 0 && items.Exists(item =>
                NearbyStorageCategory.For(item.Sample) == _category);
            string filter = NormalizeFilterText(_filter == null ? "" : _filter.text.Trim());
            bool[] occupied = new bool[NearbyStorageCategory.French.Length];
            foreach (NearbyStorageItem item in items)
            {
                if (!MatchesFilter(item.Name, filter))
                {
                    continue;
                }
                occupied[NearbyStorageCategory.For(item.Sample)] = true;
            }
            UpdateCategoryOpacity(occupied);
            items.RemoveAll(item =>
                (_category >= 0 && (!unmatchedDrag || categoryHasItems) &&
                    NearbyStorageCategory.For(item.Sample) != _category) ||
                !MatchesFilter(item.Name, filter));
            NearbyStorageSort.Sort(items);
            string fingerprint = Fingerprint(items);
            if (!_dirtyView && fingerprint == _fingerprint) { return; }
            _fingerprint = fingerprint;
            _dirtyView = false;
            HideTooltip();
            DrawCards(items);
        }

        // Matches item names without distinguishing case or accents.
        private static bool MatchesFilter(string name, string filter)
        {
            if (filter.Length == 0) { return true; }
            return NormalizeFilterText(name).IndexOf(filter, StringComparison.Ordinal) >= 0;
        }

        // Removes combining marks and folds Turkish dotless i for search.
        private static string NormalizeFilterText(string value)
        {
            string decomposed = value.Normalize(NormalizationForm.FormD);
            var result = new StringBuilder(decomposed.Length);
            foreach (char character in decomposed)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category == UnicodeCategory.NonSpacingMark ||
                    category == UnicodeCategory.SpacingCombiningMark ||
                    category == UnicodeCategory.EnclosingMark)
                {
                    continue;
                }
                result.Append(char.ToUpperInvariant(character == '\u0131' ? 'i' : character));
            }
            return result.ToString();
        }

        // Dims categories with no matching nearby items.
        private static void UpdateCategoryOpacity(bool[] occupied)
        {
            Transform list = _categoryContent;
            if (list == null) { return; }
            var order = new List<int>(occupied.Length);
            for (int i = 0; i < occupied.Length; i++) { order.Add(i); }
            order.Sort((a, b) =>
            {
                if (occupied[a] != occupied[b]) { return occupied[a] ? -1 : 1; }
                return string.Compare(NearbyStorageCategory.Label(a),
                    NearbyStorageCategory.Label(b), StringComparison.CurrentCultureIgnoreCase);
            });
            for (int position = 0; position < order.Count; position++)
            {
                RectTransform row = list.Find("Category " + order[position]) as RectTransform;
                if (row != null)
                {
                    row.anchoredPosition = new Vector2(0f, -position * CategoryRowHeight);
                }
            }
            if (_selectFirstCategory && order.Count > 0 && occupied[order[0]])
            {
                _category = order[0];
                _categoryContent.anchoredPosition = Vector2.zero;
                _scrollCategoryToSelection = false;
                _selectFirstCategory = false;
                for (int i = 0; i < occupied.Length; i++) { SetCategoryHover(i, false); }
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
            ScrollToSelectedCategory();
        }

        // Tracks visible quantities and source locations to avoid redraw flicker.
        private static string Fingerprint(List<NearbyStorageItem> items)
        {
            var value = new StringBuilder();
            foreach (NearbyStorageItem item in items)
            {
                value.Append(item.Key).Append(':').Append(item.Count).Append(':')
                    .Append(item.Sample.m_quality).Append('|');
                foreach (NearbyStorageSource source in item.Sources)
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

        // Destroys the transient UI without changing stored items.
        internal static void Dispose()
        {
            if (_root != null) { UnityEngine.Object.Destroy(_root.gameObject); }
            _root = null;
            _cards = null;
            _categoryContent = null;
            _tooltip = null;
            _hoverItem = null;
            _dragCancelOverlay = null;
            _tooltipTitle = null;
            _tooltipDescription = null;
            _tooltipSources = null;
            _filter = null;
            _gui = null;
            _fingerprint = null;
            _dirtyView = true;
            _hasNearbyItems = false;
            _dragKey = null;
            _scrollToDragKey = null;
            _scrollToSimilarName = null;
            _scrollCategoryToSelection = false;
            _selectFirstCategory = true;
            _nearbyDragKey = null;
        }
    }
}
