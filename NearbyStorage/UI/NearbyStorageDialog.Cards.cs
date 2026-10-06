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
                int remaining = items[i].Count -
                    (items[i].Key == _nearbyDragKey ? _nearbyDragAmount : 0);
                if (remaining <= 0)
                {
                    CreateCardBackground("Held item", i % columns, i / columns, step);
                }
                else
                {
                    DrawCard(items[i], i % columns, i / columns, step, remaining);
                }
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

        // Draws one clickable icon and aggregate quantity.
        private static void DrawCard(NearbyStorageItem item, int column, int row, float step,
            int visibleCount)
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
            image.raycastTarget = false;
            TextMeshProUGUI count = Label("Quantity", card, visibleCount.ToString("N0"), 15f,
                new Vector2(2f, -(size - 19f)), new Vector2(size - 4f, 18f));
            StyleCount(count);
            count.alignment = TextAlignmentOptions.BottomRight;
        }
    }
}
