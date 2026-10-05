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
            float width = Mathf.Max(260f, _cards.parent.GetComponent<RectTransform>().rect.width);
            int columns = Mathf.Max(1, Mathf.FloorToInt((width - 4f) / 88f));
            float cellWidth = (width - 4f) / columns;
            for (int i = 0; i < items.Count; i++)
            {
                DrawCard(items[i], i % columns, i / columns, cellWidth);
            }
            int rows = (items.Count + columns - 1) / columns;
            _cards.sizeDelta = new Vector2(0f, Mathf.Max(255f, rows * 93f));
        }

        // Draws one clickable icon, name, and aggregate quantity.
        private static void DrawCard(NearbyStockItem item, int column, int row, float width)
        {
            RectTransform card = Rect(item.Key, _cards, new Vector2(width - 3f, 90f));
            card.anchoredPosition = new Vector2(column * width, -row * 93f);
            card.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.48f);
            card.gameObject.AddComponent<Button>().onClick.AddListener(() =>
                NearbyStockTransfer.Select(item));
            card.gameObject.AddComponent<NearbyStockCard>().Item = item;
            RectTransform icon = Rect("Icon", card, new Vector2(50f, 50f));
            icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 1f);
            icon.pivot = new Vector2(0.5f, 1f);
            icon.anchoredPosition = new Vector2(0f, -22f);
            Image image = icon.gameObject.AddComponent<Image>();
            image.sprite = item.Sample.GetIcon();
            image.preserveAspect = true;
            image.raycastTarget = false;
            TextMeshProUGUI name = Label("Name", card, item.Name, 12f,
                new Vector2(3f, -3f), new Vector2(width - 9f, 18f));
            name.alignment = TextAlignmentOptions.Center;
            TextMeshProUGUI count = Label("Quantity", card, item.Count.ToString("N0"), 17f,
                new Vector2(3f, -67f), new Vector2(width - 9f, 21f));
            count.alignment = TextAlignmentOptions.BottomRight;
        }

        // Creates a small noninteractive tooltip above the stock dialog.
        private static void CreateTooltip()
        {
            _tooltip = Rect("Stock tooltip", _root, new Vector2(260f, 80f));
            _tooltip.pivot = new Vector2(0f, 1f);
            Image image = _tooltip.gameObject.AddComponent<Image>();
            image.color = new Color(0.08f, 0.06f, 0.06f, 0.98f);
            image.raycastTarget = false;
            _tooltipText = Label("Details", _tooltip, "", 15f,
                new Vector2(8f, -7f), new Vector2(244f, 66f));
            _tooltipText.textWrappingMode = TextWrappingModes.Normal;
            _tooltip.gameObject.SetActive(false);
        }

        // Shows each contributing container, its distance, and its item count.
        internal static void ShowTooltip(NearbyStockItem item)
        {
            if (_tooltip == null || item == null || Player.m_localPlayer == null) { return; }
            var lines = new StringBuilder();
            foreach (NearbyStockSource source in item.Sources)
            {
                if (source.Chest == null) { continue; }
                Vector3 delta = source.Chest.transform.position -
                    Player.m_localPlayer.transform.position;
                int metres = Mathf.RoundToInt(new Vector2(delta.x, delta.z).magnitude);
                if (lines.Length > 0) { lines.Append('\n'); }
                lines.Append(ChestChatLog.ShortLabel(source.Chest));
                lines.Append(" (").Append(metres).Append(" m) — ");
                lines.Append(source.Count).Append(' ').Append(item.Name);
            }
            _tooltipText.text = lines.ToString();
            float height = Mathf.Max(38f, _tooltipText.GetPreferredValues(
                _tooltipText.text, 244f, Mathf.Infinity).y + 15f);
            _tooltip.sizeDelta = new Vector2(260f, height);
            _tooltipText.rectTransform.sizeDelta = new Vector2(244f, height - 14f);
            _tooltip.gameObject.SetActive(true);
            _tooltip.SetAsLastSibling();
            PositionTooltip();
        }

        // Follows the mouse while keeping details inside the dialog bounds.
        private static void PositionTooltip()
        {
            if (_tooltip == null || !_tooltip.gameObject.activeSelf) { return; }
            Canvas canvas = _root.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ?
                canvas.worldCamera : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_root,
                Input.mousePosition, camera, out Vector2 point);
            _tooltip.anchoredPosition = new Vector2(
                Mathf.Clamp(point.x + 14f, 0f, _root.rect.width - _tooltip.rect.width),
                Mathf.Clamp(point.y - 14f, -_root.rect.height + _tooltip.rect.height, 0f));
        }

        // Hides the contributing-container tooltip after pointer exit.
        internal static void HideTooltip()
        {
            if (_tooltip != null) { _tooltip.gameObject.SetActive(false); }
        }
    }
}
