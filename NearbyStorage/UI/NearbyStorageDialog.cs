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
        private static TMP_InputField _filter;
        private static InventoryGui _gui;
        private static float _nextRefresh;
        private static int _category;
        private static string _fingerprint;
        private static bool _dirtyView = true;
        private static bool _hasNearbyItems;
        private static bool _wasPanelAvailable;
        private static string _dragKey;
        private static string _scrollToDragKey;
        private static string _scrollToSimilarName;
        private static bool _scrollCategoryToSelection;
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
                _wasPanelAvailable = false;
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
            bool opening = !_wasPanelAvailable;
            if (opening)
            {
                _filter?.SetTextWithoutNotify("");
                SelectCategory(-1);
                _categoryContent.anchoredPosition = Vector2.zero;
            }
            _wasPanelAvailable = true;
            Position();
            UpdateDepositNotices();
            UpdateDragFocus();
            if (Time.unscaledTime >= _nextRefresh) { Refresh(); }
            _root.gameObject.SetActive(_hasNearbyItems);
            if (opening && _hasNearbyItems && _filter != null)
            {
                _filter.Select();
                _filter.ActivateInputField();
            }
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
                SetHeldCardAppearance(_nearbyDragKey, false);
                _nearbyDragKey = nearbyDragKey;
                SetHeldCardAppearance(_nearbyDragKey, true);
                RefreshSoon();
            }
        }

        // Keeps nearby items clickable while placing the split dialog in front.
        private static void UpdateVisibleDialog(InventoryGui gui)
        {
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
            if (NearbyStorageTransfer.DraggedNearbyItemKey() != null) { return; }
            List<NearbyStorageItem> items = NearbyStorageCatalog.Read(Player.m_localPlayer);
            _hasNearbyItems = items.Count > 0;
            bool unmatchedDrag = _dragKey != null &&
                !items.Exists(item => item.Key == _dragKey ||
                    item.Key.StartsWith(_dragKey + "|", StringComparison.Ordinal));
            bool categoryHasItems = _category >= 0 && items.Exists(item =>
                NearbyStorageCategory.For(item.Sample) == _category);
            string filter = NormalizeFilterText(_filter == null ? "" : _filter.text.Trim());
            bool[] occupied = new bool[ModText.CategoryCount];
            foreach (NearbyStorageItem item in items)
            {
                if (!MatchesFilter(item, filter))
                {
                    continue;
                }
                occupied[NearbyStorageCategory.For(item.Sample)] = true;
            }
            UpdateCategoryVisibility(occupied);
            items.RemoveAll(item =>
                (_category >= 0 && (!unmatchedDrag || categoryHasItems) &&
                    NearbyStorageCategory.For(item.Sample) != _category) ||
                !MatchesFilter(item, filter));
            NearbyStorageSort.Sort(items);
            string fingerprint = Fingerprint(items);
            if (!_dirtyView && fingerprint == _fingerprint) { return; }
            _fingerprint = fingerprint;
            _dirtyView = false;
            HideTooltip();
            DrawCards(items);
        }

        // Matches an item name or category without distinguishing case or accents.
        private static bool MatchesFilter(NearbyStorageItem item, string filter)
        {
            if (filter.Length == 0) { return true; }
            return NormalizeFilterText(item.Name).IndexOf(filter, StringComparison.Ordinal) >= 0 ||
                NormalizeFilterText(NearbyStorageCategory.Label(
                    NearbyStorageCategory.For(item.Sample))).IndexOf(
                        filter, StringComparison.Ordinal) >= 0;
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
            DepositNotices.Clear();
            PendingDepositNotices.Clear();
            _depositNoticeOrigin = Vector2.zero;
            _hoverItem = null;
            _tooltipTitle = null;
            _tooltipDescription = null;
            _tooltipSources = null;
            _filter = null;
            _gui = null;
            _fingerprint = null;
            _dirtyView = true;
            _hasNearbyItems = false;
            _wasPanelAvailable = false;
            _dragKey = null;
            _scrollToDragKey = null;
            _scrollToSimilarName = null;
            _scrollCategoryToSelection = false;
            _nearbyDragKey = null;
        }
    }
}
