using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.SuperStorage
{
    // Builds and scrolls the nearby stock category list.
    internal static partial class NearbyStockDialog
    {
        // Creates an inventory-style list of categories on the left.
        private static void CreateCategories()
        {
            RectTransform list = Rect("Categories", _root, Vector2.zero);
            list.anchorMin = new Vector2(0f, 0f);
            list.anchorMax = new Vector2(0f, 1f);
            list.offsetMin = new Vector2(16f, 10f);
            list.offsetMax = new Vector2(196f, -HeaderHeight);
            Image listBackground = list.gameObject.AddComponent<Image>();
            listBackground.color = new Color(0f, 0f, 0f, 0.72f);
            listBackground.raycastTarget = false;
            list.gameObject.AddComponent<RectMask2D>();
            _categoryContent = Rect("Category content", list,
                new Vector2(164f, NearbyStockCategory.French.Length * CategoryRowHeight));
            ScrollRect scroll = list.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = list;
            scroll.content = _categoryContent;
            scroll.horizontal = false;
            scroll.scrollSensitivity = 100f;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            for (int i = 0; i < NearbyStockCategory.French.Length; i++)
            {
                int index = i;
                RectTransform row = Rect("Category " + i, _categoryContent,
                    new Vector2(164f, CategoryRowHeight));
                row.anchoredPosition = new Vector2(0f, -i * CategoryRowHeight);
                Image image = row.gameObject.AddComponent<Image>();
                image.color = Color.clear;
                RectTransform selected = Rect("Selected", row, Vector2.zero);
                selected.anchorMin = Vector2.zero;
                selected.anchorMax = Vector2.one;
                selected.offsetMin = selected.offsetMax = Vector2.zero;
                StyleCategorySelection(selected.gameObject.AddComponent<Image>());
                TextMeshProUGUI text = Label("Text", row, NearbyStockCategory.Label(i), 14f,
                    new Vector2(5f, -3f), new Vector2(158f, 22f));
                StyleCategoryText(text);
                Button button = row.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                row.gameObject.AddComponent<NearbyStockCategoryHover>().Index = index;
                button.onClick.AddListener(() => SelectCategory(
                    _category == index ? -1 : index));
            }
            CreateCategoryScrollbar(scroll, list);
            SelectCategory(_category);
        }

        // Shows a native-style scrollbar beside the category rows when needed.
        private static void CreateCategoryScrollbar(ScrollRect scroll, RectTransform list)
        {
            RectTransform track = Rect("Categories scrollbar", list, Vector2.zero);
            track.anchorMin = new Vector2(1f, 0f);
            track.anchorMax = new Vector2(1f, 1f);
            track.offsetMin = new Vector2(-16f, 0f);
            track.offsetMax = Vector2.zero;
            Image background = track.gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.4f);
            RectTransform handle = Rect("Handle", track, Vector2.zero);
            handle.anchorMin = Vector2.zero;
            handle.anchorMax = Vector2.one;
            Image image = handle.gameObject.AddComponent<Image>();
            image.color = new Color(1f, 0.65f, 0.2f, 0.9f);
            StyleScrollbar(background, image);
            Scrollbar bar = track.gameObject.AddComponent<Scrollbar>();
            bar.handleRect = handle;
            bar.targetGraphic = image;
            bar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        }

        // Highlights the selected category and refreshes matching items.
        private static void SelectCategory(int index, bool clearFilter = true)
        {
            if (clearFilter && index >= 0 && _filter != null && !string.IsNullOrEmpty(_filter.text))
            {
                _filter.SetTextWithoutNotify("");
            }
            _category = index;
            _scrollCategoryToSelection = index >= 0;
            for (int i = 0; i < NearbyStockCategory.French.Length; i++)
            {
                SetCategoryHover(i, false);
            }
            RefreshSoon();
        }

        // Applies the selected or hovered color directly without button tinting.
        internal static void SetCategoryHover(int index, bool hovered)
        {
            Transform row = _categoryContent?.Find("Category " + index);
            if (row == null) { return; }
            row.Find("Selected")?.gameObject.SetActive(index == _category);
            row.GetComponent<Image>().color = hovered && index != _category ?
                new Color(0.45f, 0.44f, 0.44f, 0.9f) :
                Color.clear;
        }

        // Keeps an automatically selected category visible after rows are reordered.
        private static void ScrollToSelectedCategory()
        {
            if (!_scrollCategoryToSelection || _category < 0 || _categoryContent == null)
            {
                return;
            }
            _scrollCategoryToSelection = false;
            RectTransform row = _categoryContent.Find("Category " + _category) as RectTransform;
            RectTransform viewport = _categoryContent.parent as RectTransform;
            if (row == null || viewport == null) { return; }
            float top = -row.anchoredPosition.y;
            float offset = _categoryContent.anchoredPosition.y;
            if (top < offset) { offset = top; }
            if (top + CategoryRowHeight > offset + viewport.rect.height)
            {
                offset = top + CategoryRowHeight - viewport.rect.height;
            }
            float maxOffset = Mathf.Max(0f, _categoryContent.rect.height - viewport.rect.height);
            viewport.GetComponent<ScrollRect>()?.StopMovement();
            _categoryContent.anchoredPosition = new Vector2(0f,
                Mathf.Clamp(offset, 0f, maxOffset));
        }

    }
}
