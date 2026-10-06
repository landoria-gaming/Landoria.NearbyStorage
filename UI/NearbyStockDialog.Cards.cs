using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.SuperStorage
{
    // Renders aggregated item cards and per-container hover details.
    internal static partial class NearbyStockDialog
    {
        // Replaces the cards with the current filtered snapshot.
        private static void DrawCards(List<NearbyStockItem> items)
        {
            foreach (Transform child in _cards)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            float width = Mathf.Max(260f,
                _cards.parent.GetComponent<RectTransform>().rect.width - GridLeftPadding);
            float step = SlotStep();
            int columns = Mathf.Max(1, Mathf.FloorToInt(width / step));
            for (int i = 0; i < items.Count; i++)
            {
                DrawCard(items[i], i % columns, i / columns, step);
            }
            int rows = Mathf.Max(4, (items.Count + columns - 1) / columns);
            _cards.sizeDelta = new Vector2(0f, GridTopPadding + rows * step);
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
            background.color = NearbyStockCard.IdleColor;
            return card;
        }

        // Draws one clickable icon and aggregate quantity.
        private static void DrawCard(NearbyStockItem item, int column, int row, float step)
        {
            RectTransform card = CreateCardBackground(item.Key, column, row, step);
            float size = SlotSize();
            NearbyStockCard hover = card.gameObject.AddComponent<NearbyStockCard>();
            hover.Item = item;
            hover.Background = card.GetComponent<Image>();
            RectTransform icon = Rect("Icon", card, Vector2.zero);
            icon.anchorMin = Vector2.zero;
            icon.anchorMax = Vector2.one;
            icon.offsetMin = Vector2.zero;
            icon.offsetMax = Vector2.zero;
            Image image = icon.gameObject.AddComponent<Image>();
            StyleIcon(image);
            image.sprite = item.Sample.GetIcon();
            image.raycastTarget = false;
            TextMeshProUGUI count = Label("Quantity", card, item.Count.ToString("N0"), 15f,
                new Vector2(2f, -(size - 19f)), new Vector2(size - 4f, 18f));
            StyleCount(count);
            count.alignment = TextAlignmentOptions.BottomRight;
        }

        // Creates a translucent details panel below the stock dialog.
        private static void CreateTooltip()
        {
            _tooltip = Rect("Stock tooltip", _root, new Vector2(260f, 120f));
            _tooltip.pivot = new Vector2(0f, 1f);
            Image image = _tooltip.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.68f);
            image.raycastTarget = false;
            _tooltipText = Label("Details", _tooltip, "", 15f,
                new Vector2(10f, -8f), new Vector2(240f, 104f));
            _tooltipText.textWrappingMode = TextWrappingModes.Normal;
            _tooltip.gameObject.SetActive(false);
        }

        // Shows each contributing container, its distance, and its item count.
        internal static void ShowTooltip(NearbyStockItem item)
        {
            if (_tooltip == null || item == null || Player.m_localPlayer == null) { return; }
            var lines = new StringBuilder(item.Name);
            lines.Append('\n');
            foreach (NearbyStockSource source in item.Sources)
            {
                if (source.Chest == null) { continue; }
                Vector3 delta = source.Chest.transform.position -
                    Player.m_localPlayer.transform.position;
                int metres = Mathf.RoundToInt(new Vector2(delta.x, delta.z).magnitude);
                lines.Append('\n');
                lines.Append(StorageLabel.ShortLabel(source.Chest));
                lines.Append(" (").Append(metres).Append(" m) — ");
                lines.Append(source.Count);
            }
            _tooltipText.text = lines.ToString();
        float width = (_cards.parent as RectTransform).rect.width + 4f;
            float textWidth = width - 20f;
            float height = Mathf.Max(120f, _tooltipText.GetPreferredValues(
                _tooltipText.text, textWidth, Mathf.Infinity).y + 16f);
            _tooltip.sizeDelta = new Vector2(width, height);
            _tooltipText.rectTransform.sizeDelta = new Vector2(textWidth, height - 16f);
            _tooltip.gameObject.SetActive(true);
            _tooltip.SetAsLastSibling();
            PositionTooltip();
        }

        // Centers the details below the stock panel's item grid.
        private static void PositionTooltip()
        {
            if (_tooltip == null || !_tooltip.gameObject.activeSelf || _cards == null) { return; }
            RectTransform viewport = _cards.parent as RectTransform;
            if (viewport == null) { return; }
            Vector3[] corners = new Vector3[4];
            viewport.GetWorldCorners(corners);
            float left = _root.InverseTransformPoint(corners[0]).x;
        _tooltip.anchoredPosition = new Vector2(
            left + GridLeftPadding,
            -_root.rect.height - 14f);
        }

        // Hides the contributing-container tooltip after pointer exit.
        internal static void HideTooltip()
        {
            if (_tooltip != null) { _tooltip.gameObject.SetActive(false); }
        }
    }
}
