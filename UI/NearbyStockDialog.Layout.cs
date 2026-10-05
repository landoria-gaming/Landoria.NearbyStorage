using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.SuperStorage
{
    // Creates the stock browser's frame, controls, and scroll areas.
    internal static partial class NearbyStockDialog
    {
        // Builds the browser beside Valheim's inventory UI hierarchy.
        private static void Create(InventoryGui gui)
        {
            _gui = gui;
            Transform parent = gui.m_inventoryRoot.parent != null ?
                gui.m_inventoryRoot.parent : gui.transform;
            _root = Rect("Nearby Stock", parent, new Vector2(590f, 350f));
            _root.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            _root.pivot = new Vector2(0f, 1f);
            _root.anchorMin = _root.anchorMax = Vector2.zero;
            Image background = _root.gameObject.AddComponent<Image>();
            background.color = new Color(0.17f, 0.12f, 0.13f, 0.96f);
            _root.SetAsLastSibling();
            Label("Title", _root, "Nearby Stock", 22f, new Vector2(12f, -8f),
                new Vector2(310f, 30f));
            CreateFilter();
            CreateSort();
            CreateCategories();
            CreateGrid();
            CreateTooltip();
            PutSortOnTop();
            Position();
            RefreshSoon();
        }

        // Keeps the expanded sorting menu above item cards.
        private static void PutSortOnTop()
        {
            _root.Find("Sort")?.SetAsLastSibling();
        }

        // Aligns the dialog directly below the player inventory panel.
        private static void Position()
        {
            if (_root == null || _gui?.m_player == null) { return; }
            Vector3[] corners = new Vector3[4];
            _gui.m_player.GetWorldCorners(corners);
            Vector3 bottomLeft = _root.parent.InverseTransformPoint(corners[0]);
            _root.localPosition = bottomLeft + new Vector3(0f, -8f, 0f);
            _root.sizeDelta = new Vector2(Mathf.Max(500f, _gui.m_player.rect.width), 350f);
        }

        // Adds the localized free-text item filter.
        private static void CreateFilter()
        {
            RectTransform box = Rect("Filter", _root, new Vector2(170f, 32f));
            box.anchoredPosition = new Vector2(10f, -46f);
            box.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.48f);
            _filter = box.gameObject.AddComponent<TMP_InputField>();
            TextMeshProUGUI value = Label("Input", box, "", 15f,
                new Vector2(7f, -5f), new Vector2(156f, 24f));
            value.raycastTarget = true;
            _filter.textComponent = value;
            TextMeshProUGUI hint = Label("Hint", box, IsFrench() ? "Filtrer…" : "Filter…",
                15f, new Vector2(7f, -5f), new Vector2(156f, 24f));
            hint.color = new Color(1f, 1f, 1f, 0.55f);
            _filter.placeholder = hint;
            _filter.onValueChanged.AddListener(_ => RefreshSoon());
        }

        // Adds one button that opens the four sorting choices.
        private static void CreateSort()
        {
            RectTransform box = Rect("Sort", _root, new Vector2(150f, 30f));
            box.anchorMin = box.anchorMax = new Vector2(1f, 1f);
            box.pivot = new Vector2(1f, 1f);
            box.anchoredPosition = new Vector2(-10f, -48f);
            box.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
            _sortText = Label("Sort label", box, SortLabel(_sort) + " ▾", 15f,
                new Vector2(6f, -4f), new Vector2(138f, 24f));
            box.gameObject.AddComponent<Button>().onClick.AddListener(() =>
                _sortOptions.SetActive(!_sortOptions.activeSelf));
            _sortOptions = new GameObject("Sort options", typeof(RectTransform));
            _sortOptions.transform.SetParent(box, false);
            RectTransform options = (RectTransform)_sortOptions.transform;
            options.anchorMin = options.anchorMax = new Vector2(0f, 0f);
            options.pivot = new Vector2(0f, 1f);
            options.anchoredPosition = new Vector2(0f, -2f);
            options.sizeDelta = new Vector2(150f, 120f);
            for (int i = 0; i < 4; i++) { CreateSortOption(options, i); }
            _sortOptions.SetActive(false);
        }

        // Adds a single choice to the sort dropdown.
        private static void CreateSortOption(RectTransform parent, int index)
        {
            RectTransform row = Rect("Sort " + index, parent, new Vector2(150f, 30f));
            row.anchoredPosition = new Vector2(0f, -index * 30f);
            row.gameObject.AddComponent<Image>().color = new Color(0.13f, 0.1f, 0.1f, 0.98f);
            Label("Text", row, SortLabel(index), 15f,
                new Vector2(6f, -4f), new Vector2(138f, 24f));
            row.gameObject.AddComponent<Button>().onClick.AddListener(() =>
            {
                _sort = index;
                _sortText.text = SortLabel(index) + " ▾";
                _sortOptions.SetActive(false);
                RefreshSoon();
            });
        }

        // Creates an inventory-style list of categories on the left.
        private static void CreateCategories()
        {
            RectTransform list = Rect("Categories", _root, new Vector2(180f, 255f));
            list.anchoredPosition = new Vector2(10f, -86f);
            for (int i = 0; i < NearbyStockCategory.French.Length; i++)
            {
                int index = i;
                RectTransform row = Rect("Category " + i, list, new Vector2(180f, 25f));
                row.anchoredPosition = new Vector2(0f, -i * 25f);
                row.gameObject.AddComponent<Image>().color = i == 0 ?
                    new Color(0.25f, 0.43f, 0.6f, 0.9f) : new Color(0f, 0f, 0f, 0.3f);
                Label("Text", row, NearbyStockCategory.Label(i), 14f,
                    new Vector2(5f, -3f), new Vector2(174f, 22f));
                row.gameObject.AddComponent<Button>().onClick.AddListener(() => SelectCategory(index));
            }
        }

        // Highlights the selected category and refreshes matching items.
        private static void SelectCategory(int index)
        {
            _category = index;
            Transform list = _root.Find("Categories");
            for (int i = 0; i < list.childCount; i++)
            {
                list.GetChild(i).GetComponent<Image>().color = i == index ?
                    new Color(0.25f, 0.43f, 0.6f, 0.9f) : new Color(0f, 0f, 0f, 0.3f);
            }
            RefreshSoon();
        }

        // Creates the clipped card viewport with a drag-only scrollbar.
        private static void CreateGrid()
        {
            RectTransform viewport = Rect("Items viewport", _root, Vector2.zero);
            viewport.anchorMin = new Vector2(0f, 0f);
            viewport.anchorMax = new Vector2(1f, 1f);
            viewport.offsetMin = new Vector2(200f, 10f);
            viewport.offsetMax = new Vector2(-25f, -85f);
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
            scroll.scrollSensitivity = 20f;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            CreateScrollbar(scroll);
        }

        // Adds a visible vertical scrollbar beside the item grid.
        private static void CreateScrollbar(ScrollRect scroll)
        {
            RectTransform track = Rect("Items scrollbar", _root, Vector2.zero);
            track.anchorMin = new Vector2(1f, 0f);
            track.anchorMax = new Vector2(1f, 1f);
            track.offsetMin = new Vector2(-20f, 10f);
            track.offsetMax = new Vector2(-8f, -85f);
            track.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);
            RectTransform handle = Rect("Handle", track, Vector2.zero);
            handle.anchorMin = Vector2.zero;
            handle.anchorMax = Vector2.one;
            Image image = handle.gameObject.AddComponent<Image>();
            image.color = new Color(1f, 0.65f, 0.2f, 0.9f);
            Scrollbar bar = track.gameObject.AddComponent<Scrollbar>();
            bar.handleRect = handle;
            bar.targetGraphic = image;
            bar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
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
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            TMP_Text native = InventoryGui.instance?.m_recipeDecription;
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
