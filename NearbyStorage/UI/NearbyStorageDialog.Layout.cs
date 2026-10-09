using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Creates the nearby storage panel's frame, controls, and scroll areas.
    internal static partial class NearbyStorageDialog
    {
        private const float GridTopPadding = 6f;
        private const float GridLeftPadding = 6f;
        private const float HeaderHeight = 106f;
        private const float CategoryRowHeight = 23f;

        // Builds the browser beside Valheim's inventory UI hierarchy.
        private static void Create(InventoryGui gui)
        {
            _gui = gui;
            _category = -1;
            Transform parent = gui.m_inventoryRoot.parent != null ?
                gui.m_inventoryRoot.parent : gui.transform;
            _root = Rect("Nearby Storage", parent, new Vector2(590f, 350f));
            _root.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            _root.pivot = new Vector2(0f, 1f);
            _root.anchorMin = _root.anchorMax = Vector2.zero;
            Image background = _root.gameObject.AddComponent<Image>();
            background.color = new Color(0.17f, 0.12f, 0.13f, 0.96f);
            StylePanel(background);
            background.raycastTarget = true;
            _root.gameObject.AddComponent<NearbyStorageDragCancel>();
            _root.SetSiblingIndex(gui.m_inventoryRoot.GetSiblingIndex() + 1);
            TextMeshProUGUI title = Label("Title", _root, "Nearby Storage", 32f,
                new Vector2(10f, -16f), new Vector2(570f, 35f));
            title.font = gui.m_containerName.font;
            title.fontSharedMaterial = gui.m_containerName.fontSharedMaterial;
            title.color = gui.m_containerName.color;
            title.alignment = TextAlignmentOptions.Center;
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.offsetMax = new Vector2(-10f, -16f);
            CreateFilter();
            CreateCategories();
            CreateGrid();
            CreateTooltip();
            Position();
            RefreshSoon();
        }

        // Aligns the dialog with the visible inventory frame above it.
        private static void Position()
        {
            if (_root == null || _gui?.m_player == null) { return; }
            RectTransform frame = _gui.m_player.Find("Bkg") as RectTransform ?? _gui.m_player;
            Vector3[] corners = new Vector3[4];
            frame.GetWorldCorners(corners);
            Vector3 bottomLeft = _root.parent.InverseTransformPoint(corners[0]);
            Vector3 bottomRight = _root.parent.InverseTransformPoint(corners[3]);
            _root.localPosition = bottomLeft + new Vector3(0f, -8f, 0f);
            _root.sizeDelta = new Vector2(bottomRight.x - bottomLeft.x,
                HeaderHeight + 10f + GridTopPadding + 4f * SlotStep());
            PositionDepositNotices(corners[2]);
        }

        // Adds the localized free-text item filter.
        private static void CreateFilter()
        {
            BuildUi ui = Hud.instance?.m_buildUi;
            Component source = ui == null ? null : SearchField?.GetValue(ui) as Component;
            TMP_InputField native = source?.GetComponent<TMP_InputField>();
            if (native != null)
            {
                GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, _root, false);
                clone.name = "Filter";
                PlaceFilter(clone.transform as RectTransform);
                _filter = clone.GetComponent<TMP_InputField>();
                _filter.onValueChanged = new TMP_InputField.OnChangeEvent();
                _filter.onEndEdit = new TMP_InputField.SubmitEvent();
                _filter.onSubmit = new TMP_InputField.SubmitEvent();
                _filter.SetTextWithoutNotify("");
                if (_filter.placeholder is TMP_Text hint)
                {
                    hint.text = IsFrench() ? "FILTRE" : "FILTER";
                }
                _filter.onValueChanged.AddListener(OnFilterChanged);
                HideFilterShortcutBadge(clone);
                clone.SetActive(true);
                return;
            }
            CreateFallbackFilter();
        }

        // Hides the native F badge while retaining the keyboard shortcut.
        private static void HideFilterShortcutBadge(GameObject clone)
        {
            foreach (TMP_Text label in clone.GetComponentsInChildren<TMP_Text>(true))
            {
                if (label == _filter.textComponent || label == _filter.placeholder ||
                    label.text.Trim() != "F") { continue; }
                Transform badge = label.transform.parent == clone.transform ?
                    label.transform : label.transform.parent;
                badge.gameObject.SetActive(false);
            }
            foreach (Text label in clone.GetComponentsInChildren<Text>(true))
            {
                if (label.text.Trim() != "F") { continue; }
                Transform badge = label.transform.parent == clone.transform ?
                    label.transform : label.transform.parent;
                badge.gameObject.SetActive(false);
            }
        }

        // Keeps the native field aligned with the item grid.
        private static void PlaceFilter(RectTransform box)
        {
            box.anchorMin = new Vector2(0f, 1f);
            box.anchorMax = new Vector2(1f, 1f);
            box.pivot = new Vector2(0f, 1f);
            box.offsetMin = new Vector2(16f, -96f);
            box.offsetMax = new Vector2(-34f, -60f);
        }

        // Supplies a local field if the build menu has not created its search box.
        private static void CreateFallbackFilter()
        {
            RectTransform box = Rect("Filter", _root, Vector2.zero);
            PlaceFilter(box);
            Image background = box.gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.48f);
            StyleFilter(background);
            _filter = box.gameObject.AddComponent<TMP_InputField>();
            StyleFilterInteraction(_filter, background);
            _filter.customCaretColor = true;
            _filter.caretColor = Color.white;
            _filter.caretWidth = 2;
            RectTransform textArea = Rect("Text Area", box, Vector2.zero);
            textArea.anchorMin = Vector2.zero;
            textArea.anchorMax = Vector2.one;
            textArea.offsetMin = Vector2.zero;
            textArea.offsetMax = Vector2.zero;
            textArea.gameObject.AddComponent<RectMask2D>();
            _filter.textViewport = textArea;
            TextMeshProUGUI value = Label("Input", textArea, "", 15f,
                new Vector2(7f, -4f), new Vector2(146f, 24f));
            StretchFilterText(value.rectTransform);
            value.alignment = TextAlignmentOptions.MidlineLeft;
            value.raycastTarget = true;
            _filter.textComponent = value;
            TextMeshProUGUI hint = Label("Hint", textArea, IsFrench() ? "FILTRE" : "FILTER",
                15f, new Vector2(7f, -4f), new Vector2(146f, 24f));
            StretchFilterText(hint.rectTransform);
            StyleFilterHint(hint);
            hint.alignment = TextAlignmentOptions.MidlineLeft;
            _filter.placeholder = hint;
            _filter.onValueChanged.AddListener(OnFilterChanged);
        }

        // A text search spans every category.
        private static void OnFilterChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && _category >= 0)
            {
                SelectCategory(-1);
            }
            RefreshSoon();
        }

        // Keeps filter text inside its width as the panel changes size.
        private static void StretchFilterText(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(7f, 2f);
            rect.offsetMax = new Vector2(-7f, -2f);
        }

        // Creates the clipped card viewport with a drag-only scrollbar.
        private static void CreateGrid()
        {
            RectTransform viewport = Rect("Items viewport", _root, Vector2.zero);
            viewport.anchorMin = new Vector2(0f, 0f);
            viewport.anchorMax = new Vector2(1f, 1f);
            viewport.offsetMin = new Vector2(200f, 10f);
            viewport.offsetMax = new Vector2(-34f, -HeaderHeight);
            Image background = viewport.gameObject.AddComponent<Image>();
            background.color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            _cards = Rect("Items", viewport, Vector2.zero);
            _cards.anchorMin = new Vector2(0f, 1f);
            _cards.anchorMax = new Vector2(1f, 1f);
            _cards.pivot = new Vector2(0f, 1f);
            _cards.anchoredPosition = Vector2.zero;
            var scroll = _root.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = _cards;
            scroll.horizontal = false;
            scroll.scrollSensitivity = 640f;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            CreateScrollbar(scroll);
        }

        // Adds a visible vertical scrollbar beside the item grid.
        private static void CreateScrollbar(ScrollRect scroll)
        {
            RectTransform track = Rect("Items scrollbar", _root, Vector2.zero);
            track.anchorMin = new Vector2(1f, 0f);
            track.anchorMax = new Vector2(1f, 1f);
            track.offsetMin = new Vector2(-24f, 10f);
            track.offsetMax = new Vector2(-8f, -HeaderHeight);
            Image trackImage = track.gameObject.AddComponent<Image>();
            trackImage.color = new Color(0f, 0f, 0f, 0.4f);
            RectTransform handle = Rect("Handle", track, Vector2.zero);
            handle.anchorMin = Vector2.zero;
            handle.anchorMax = Vector2.one;
            Image image = handle.gameObject.AddComponent<Image>();
            image.color = new Color(1f, 0.65f, 0.2f, 0.9f);
            StyleScrollbar(trackImage, image);
            Scrollbar bar = track.gameObject.AddComponent<Scrollbar>();
            bar.handleRect = handle;
            bar.targetGraphic = image;
            bar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        }

        // Makes a top-left anchored UI rectangle.
        private static RectTransform Rect(string name, Transform parent, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)obj.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            return rect;
        }

        // Makes a text label using Valheim's inventory font.
        private static TextMeshProUGUI Label(string name, Transform parent, string value,
            float size, Vector2 position, Vector2 dimensions)
        {
            RectTransform rect = Rect(name, parent, dimensions);
            rect.anchoredPosition = position;
            rect.gameObject.SetActive(false);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            TMP_Text native = name == "Quantity" ? SlotElement()?.m_amount :
                InventoryGui.instance?.m_recipeDecription;
            if (native == null || native.font == null)
            {
                native = InventoryGui.instance?.m_containerName;
            }
            if (native != null)
            {
                label.font = native.font;
                label.fontSharedMaterial = native.fontSharedMaterial;
            }
            label.text = value;
            label.fontSize = size;
            label.color = Color.white;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            rect.gameObject.SetActive(true);
            return label;
        }

        // Tests whether the current game's language is French.
        private static bool IsFrench()
        {
            return Localization.instance != null &&
                Localization.instance.GetSelectedLanguage() == "French";
        }
    }
}
