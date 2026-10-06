using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Shows item details and one inventory row per contributing container.
    internal static partial class NearbyStorageDialog
    {
        private const float TooltipWidth = 340f;
        private const float SourceRowHeight = 36f;
        private const float TooltipDelay = 1f;
        private static TextMeshProUGUI _tooltipTitle;
        private static TextMeshProUGUI _tooltipDescription;
        private static RectTransform _tooltipSources;
        private static NearbyStorageItem _hoverItem;
        private static Vector3 _hoverPointerPosition;
        private static float _hoverReadyAt;

        // Creates a native-looking popup without intercepting inventory input.
        private static void CreateTooltip()
        {
            _tooltip = Rect("Storage tooltip", _root, new Vector2(TooltipWidth, 100f));
            Image background = _tooltip.gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.947f);
            background.raycastTarget = false;
            _tooltipTitle = Label("Topic", _tooltip, "", 22f,
                new Vector2(12f, -8f), new Vector2(TooltipWidth - 24f, 30f));
            StyleTooltipText(_tooltipTitle, "Topic");
            _tooltipTitle.alignment = TextAlignmentOptions.Center;
            _tooltipDescription = Label("Text", _tooltip, "", 17f,
                new Vector2(12f, -44f), new Vector2(TooltipWidth - 24f, 30f));
            StyleTooltipText(_tooltipDescription, "Text");
            _tooltipDescription.textWrappingMode = TextWrappingModes.Normal;
            _tooltipSources = Rect("Sources", _tooltip, Vector2.zero);
            _tooltipSources.anchoredPosition = new Vector2(10f, -80f);
            _tooltip.gameObject.SetActive(false);
        }

        // Starts a new delay for the item under the pointer.
        internal static void ScheduleTooltip(NearbyStorageItem item)
        {
            _hoverItem = item;
            _hoverPointerPosition = ZInput.pointerPosition;
            _hoverReadyAt = Time.unscaledTime + TooltipDelay;
            if (_tooltip != null) { _tooltip.gameObject.SetActive(false); }
        }

        // Waits for one second without pointer movement before showing the item.
        private static void UpdateTooltipHover()
        {
            if (_hoverItem == null || _tooltip == null) { return; }
            Vector3 pointer = ZInput.pointerPosition;
            if ((pointer - _hoverPointerPosition).sqrMagnitude > 1f)
            {
                _hoverPointerPosition = pointer;
                _hoverReadyAt = Time.unscaledTime + TooltipDelay;
                _tooltip.gameObject.SetActive(false);
            }
            if (!_tooltip.gameObject.activeSelf && Time.unscaledTime >= _hoverReadyAt)
            {
                ShowTooltip(_hoverItem);
            }
        }

        // Builds the description and all contributing storage rows.
        private static void ShowTooltip(NearbyStorageItem item)
        {
            if (_tooltip == null || item?.Sample == null || Player.m_localPlayer == null) { return; }
            ClearTooltipSources();
            _tooltipTitle.text = item.Name;
            _tooltipDescription.text = Localization.instance.Localize(
                item.Sample.GetTooltip());
            float descriptionHeight = Mathf.Max(26f, _tooltipDescription.GetPreferredValues(
                _tooltipDescription.text, TooltipWidth - 24f, Mathf.Infinity).y);
            _tooltipDescription.rectTransform.sizeDelta =
                new Vector2(TooltipWidth - 24f, descriptionHeight);
            float rowsTop = 52f + descriptionHeight + 10f;
            _tooltipSources.anchoredPosition = new Vector2(10f, -rowsTop);
            int row = 0;
            foreach (NearbyStorageSource source in item.Sources)
            {
                if (source.Chest != null) { CreateSourceRow(source, row++); }
            }
            _tooltip.sizeDelta = new Vector2(TooltipWidth,
                rowsTop + row * SourceRowHeight + 10f);
            _tooltip.gameObject.SetActive(true);
            _tooltip.SetAsLastSibling();
            PositionTooltip();
        }

        // Uses the vanilla popup's title and body typography.
        private static void StyleTooltipText(TextMeshProUGUI target, string part)
        {
            GameObject prefab = SlotElement()?.m_tooltip?.m_tooltipPrefab;
            TMP_Text source = prefab == null ? null :
                Utils.FindChild(prefab.transform, part)?.GetComponent<TMP_Text>();
            if (source == null || source.font == null) { return; }
            target.font = source.font;
            target.fontSharedMaterial = source.fontSharedMaterial;
            target.fontSize = source.fontSize;
            target.fontStyle = source.fontStyle;
            target.color = source.color;
        }

        // Removes rows from the previous hovered item before rebuilding them.
        private static void ClearTooltipSources()
        {
            foreach (Transform child in _tooltipSources)
            {
                child.gameObject.SetActive(false);
                Object.Destroy(child.gameObject);
            }
        }

        // Draws an icon, container name and distance, and right-aligned amount.
        private static void CreateSourceRow(NearbyStorageSource source, int index)
        {
            float width = TooltipWidth - 20f;
            RectTransform row = Rect("Container", _tooltipSources,
                new Vector2(width, SourceRowHeight - 2f));
            row.anchoredPosition = new Vector2(0f, -index * SourceRowHeight);
            Image background = row.gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.30f);
            background.raycastTarget = false;
            AddSourceIcon(row, source.Chest);
            Vector3 delta = source.Chest.transform.position -
                Player.m_localPlayer.transform.position;
            int metres = Mathf.RoundToInt(new Vector2(delta.x, delta.z).magnitude);
            TextMeshProUGUI name = Label("Container name", row,
                $"{StorageLabel.ShortLabel(source.Chest)} ({metres} m)", 16f,
                new Vector2(42f, -5f), new Vector2(width - 104f, 25f));
            StyleTooltipText(name, "Text");
            name.alignment = TextAlignmentOptions.MidlineLeft;
            name.textWrappingMode = TextWrappingModes.NoWrap;
            name.overflowMode = TextOverflowModes.Ellipsis;
            TextMeshProUGUI amount = Label("Amount", row, source.Count.ToString("N0"), 16f,
                new Vector2(width - 61f, -5f), new Vector2(54f, 25f));
            StyleTooltipText(amount, "Text");
            amount.alignment = TextAlignmentOptions.MidlineRight;
        }

        // Takes the build icon of a chest, wagon, or boat when available.
        private static void AddSourceIcon(RectTransform row, Container chest)
        {
            Piece piece = chest.GetComponent<Piece>() ?? chest.GetComponentInParent<Piece>();
            Sprite sprite = piece?.m_icon;
            if (sprite == null)
            {
                sprite = ZNetScene.instance?.GetPrefab("piece_chest_wood")?.
                    GetComponent<Piece>()?.m_icon;
            }
            RectTransform rect = Rect("Container icon", row, new Vector2(28f, 28f));
            rect.anchoredPosition = new Vector2(5f, -3f);
            Image icon = rect.gameObject.AddComponent<Image>();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }

        // Follows the pointer below and to its right, then fits on screen.
        private static void PositionTooltip()
        {
            if (_tooltip == null || !_tooltip.gameObject.activeSelf) { return; }
            Vector3 position = _tooltip.position;
            const float offset = 50f;
            position.x = ZInput.pointerPosition.x + offset;
            position.y = ZInput.pointerPosition.y - offset;
            _tooltip.position = position;
            Utils.ClampUIToScreen(_tooltip);
        }

        internal static void HideTooltip()
        {
            _hoverItem = null;
            if (_tooltip != null) { _tooltip.gameObject.SetActive(false); }
        }
    }
}
