using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Renders aggregated item cards in the nearby storage panel.
    internal static partial class NearbyStorageDialog
    {
        // Replaces the cards with the current filtered snapshot.
        private static void DrawCards(List<NearbyStorageItem> items)
        {
            foreach (Transform child in _cards)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            float width = Mathf.Max(260f,
                _cards.parent.GetComponent<RectTransform>().rect.width - GridLeftPadding);
            float step = SlotStep();
            int columns = Mathf.Max(5, Mathf.FloorToInt(width / step));
            for (int i = 0; i < items.Count; i++)
            {
                DrawCard(items[i], i % columns, i / columns, step,
                    items[i].Key == _nearbyDragKey);
            }
            int rows = Mathf.Max(4, (items.Count + columns - 1) / columns);
            for (int i = items.Count; i < rows * columns; i++)
            {
                CreateCardBackground("Empty slot", i % columns, i / columns, step);
            }
            _cards.sizeDelta = new Vector2(0f, GridTopPadding + rows * step);
            ScrollToDraggedItem(items, columns, step);
        }

        // Reveals the dragged item without changing the order of the grid.
        private static void ScrollToDraggedItem(List<NearbyStorageItem> items, int columns,
            float step)
        {
            if (_scrollToDragKey == null) { return; }
            int index = items.FindIndex(item => item.Key == _scrollToDragKey);
            if (index < 0)
            {
                string prefix = _scrollToDragKey + "|";
                index = items.FindIndex(item => item.Key.StartsWith(prefix,
                    StringComparison.Ordinal));
            }
            if (index < 0 && _scrollToSimilarName != null)
            {
                index = NearbyStorageSort.ClosestIndex(items, _scrollToSimilarName);
            }
            _scrollToDragKey = null;
            _scrollToSimilarName = null;
            if (index < 0) { return; }
            RectTransform viewport = _cards.parent as RectTransform;
            float viewHeight = viewport.rect.height;
            float top = GridTopPadding + index / columns * step;
            float bottom = top + SlotSize();
            float offset = _cards.anchoredPosition.y;
            if (top < offset) { offset = top; }
            if (bottom > offset + viewHeight) { offset = bottom - viewHeight; }
            float maxOffset = Mathf.Max(0f, _cards.rect.height - viewHeight);
            _root.GetComponent<ScrollRect>()?.StopMovement();
            _cards.anchoredPosition = new Vector2(_cards.anchoredPosition.x,
                Mathf.Clamp(offset, 0f, maxOffset));
        }

        // Makes an inventory-style slot for an available item.
        private static RectTransform CreateCardBackground(string name, int column, int row,
            float step)
        {
            float size = SlotSize();
            RectTransform card = Rect(name, _cards, new Vector2(size, size));
            card.anchoredPosition = new Vector2(GridLeftPadding + column * step,
                -GridTopPadding - row * step);
            Image background = card.gameObject.AddComponent<Image>();
            StyleSlot(background);
            return card;
        }

        // Draws one clickable icon and aggregate quantity, dimming a held item.
        private static void DrawCard(NearbyStorageItem item, int column, int row, float step,
            bool held)
        {
            RectTransform card = CreateCardBackground(item.Key, column, row, step);
            float size = SlotSize();
            NearbyStorageCard hover = card.gameObject.AddComponent<NearbyStorageCard>();
            hover.Item = item;
            hover.Background = card.GetComponent<Image>();
            hover.IdleColor = hover.Background.color;
            RectTransform icon = Rect("Icon", card, Vector2.zero);
            icon.anchorMin = Vector2.zero;
            icon.anchorMax = Vector2.one;
            icon.offsetMin = Vector2.zero;
            icon.offsetMax = Vector2.zero;
            Image image = icon.gameObject.AddComponent<Image>();
            StyleIcon(image);
            image.sprite = item.Sample.GetIcon();
            if (held) { image.color = new Color(0.65f, 0.65f, 0.65f, image.color.a); }
            image.raycastTarget = false;
            DrawItemMarkers(card, item.Sample);
            if (item.Sample.m_shared.m_maxStackSize > 1)
            {
                TextMeshProUGUI count = Label("Quantity", card, item.Count.ToString("N0"), 15f,
                    new Vector2(2f, -(size - 19f)), new Vector2(size - 4f, 18f));
                StyleCount(count);
                count.alignment = TextAlignmentOptions.BottomRight;
            }
        }

        // Copies the vanilla quality label and food marker into the nearby slot.
        private static void DrawItemMarkers(RectTransform card, ItemDrop.ItemData item)
        {
            InventoryElement element = SlotElement();
            RectTransform slot = element?.transform as RectTransform;
            if (slot == null) { return; }
            TMP_Text nativeQuality = element.m_quality;
            if (item.m_shared.m_maxQuality > 1 && nativeQuality != null)
            {
                TextMeshProUGUI quality = Label("Quality", card, item.m_quality.ToString(),
                    nativeQuality.fontSize, Vector2.zero, Vector2.zero);
                quality.font = nativeQuality.font;
                quality.fontSharedMaterial = nativeQuality.fontSharedMaterial;
                quality.fontStyle = nativeQuality.fontStyle;
                quality.color = nativeQuality.color;
                quality.alignment = nativeQuality.alignment;
                MatchIconRect(nativeQuality.rectTransform, quality.rectTransform, slot);
            }
            if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable ||
                item.m_shared.m_food <= 0f && item.m_shared.m_foodStamina <= 0f &&
                item.m_shared.m_foodEitr <= 0f || element.m_food == null) { return; }
            RectTransform marker = Rect("Food", card, Vector2.zero);
            Image food = marker.gameObject.AddComponent<Image>();
            CopyImage(element.m_food, food);
            MatchIconRect(element.m_food.rectTransform, marker, slot);
            food.color = FoodColor(item);
            food.raycastTarget = false;
        }

        // Uses the same food colors and thresholds as InventoryGrid.
        private static Color FoodColor(ItemDrop.ItemData item)
        {
            float health = item.m_shared.m_food;
            float stamina = item.m_shared.m_foodStamina;
            float eitr = item.m_shared.m_foodEitr;
            if (health < eitr / 2f && stamina < eitr / 2f)
            {
                return new Color(0.6f, 0.6f, 1f, 1f);
            }
            if (stamina < health / 2f) { return new Color(1f, 0.5f, 0.5f, 1f); }
            if (health < stamina / 2f) { return new Color(1f, 1f, 0.5f, 1f); }
            return Color.white;
        }
    }
}
