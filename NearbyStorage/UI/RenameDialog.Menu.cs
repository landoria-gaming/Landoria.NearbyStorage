using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Builds the family and category picker for the name editor.
    internal static partial class RenameDialog
    {
        private const float MenuWidth = 260f;
        private const float MenuHeight = 230f;
        private const float RowHeight = 29f;
        private const float ScrollbarWidth = 12f;
        private static string _menuLanguage;

        // Opens or closes the picker from the ellipsis button.
        private static void ToggleMenu()
        {
            if (_menu == null) { return; }
            HideCategoryHelp();
            if (_menuLanguage != ModText.SelectedLanguage) { PopulateMenu(); }
            bool open = !_menu.activeSelf;
            _menuBlocker.SetActive(open);
            _menu.SetActive(open);
            if (open) { _menu.transform.SetAsLastSibling(); }
        }

        // Closes the picker without changing the current name.
        private static void HideMenu()
        {
            if (_menu != null) { _menu.SetActive(false); }
            if (_menuBlocker != null) { _menuBlocker.SetActive(false); }
        }

        // Captures clicks outside the menu before they reach other controls.
        private static void CreateBlocker(RectTransform panel)
        {
            RectTransform blocker = Rect("Suggestion click blocker", panel,
                new Vector2(10000f, 10000f));
            blocker.anchorMin = blocker.anchorMax = new Vector2(0.5f, 0.5f);
            blocker.anchoredPosition = Vector2.zero;
            Image image = blocker.gameObject.AddComponent<Image>();
            image.color = Color.clear;
            Button button = blocker.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(HideMenu);
            _menuBlocker = blocker.gameObject;
            _menuBlocker.SetActive(false);
        }

        // Places a scrolling menu below the input, flush with the button's right edge.
        private static void CreateMenu(RectTransform panel, RectTransform button)
        {
            CreateBlocker(panel);
            Vector3[] corners = new Vector3[4];
            button.GetWorldCorners(corners);
            Vector3 right = panel.InverseTransformPoint(corners[3]);
            RectTransform root = Rect("Name suggestions", panel,
                new Vector2(MenuWidth, MenuHeight));
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(1f, 1f);
            root.anchoredPosition = new Vector2(right.x, right.y - 5f);
            Image background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.947f);
            RectTransform viewport = Rect("Viewport", root, Vector2.zero);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = new Vector2(-ScrollbarWidth, 0f);
            viewport.gameObject.AddComponent<RectMask2D>();
            RectTransform content = Rect("Content", viewport, Vector2.zero);
            content.anchorMin = content.anchorMax = new Vector2(0f, 1f);
            content.pivot = new Vector2(0f, 1f);
            ScrollRect scroll = root.gameObject.AddComponent<RenameMenuScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 800f;
            CreateScrollbar(root, scroll);
            _menu = root.gameObject;
            PopulateMenu();
            HideMenu();
        }

        // Adds a visible thumb that can also be dragged through the suggestions.
        private static void CreateScrollbar(RectTransform root, ScrollRect scroll)
        {
            RectTransform track = Rect("Scrollbar", root, Vector2.zero);
            track.anchorMin = new Vector2(1f, 0f);
            track.anchorMax = new Vector2(1f, 1f);
            track.offsetMin = new Vector2(-ScrollbarWidth, 0f);
            track.offsetMax = Vector2.zero;
            Image trackImage = track.gameObject.AddComponent<Image>();
            trackImage.color = Color.clear;
            RectTransform handle = Rect("Handle", track, Vector2.zero);
            handle.anchorMin = Vector2.zero;
            handle.anchorMax = Vector2.one;
            handle.offsetMin = handle.offsetMax = Vector2.zero;
            Image handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = new Color(1f, 0.68f, 0.3f, 0.85f);
            Scrollbar bar = track.gameObject.AddComponent<Scrollbar>();
            bar.handleRect = handle;
            bar.targetGraphic = handleImage;
            bar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        }

        // Rebuilds suggestions in the selected game language.
        private static void PopulateMenu()
        {
            if (_menu == null) { return; }
            _menuLanguage = ModText.SelectedLanguage;
            RectTransform content = _menu.transform.Find("Viewport/Content") as RectTransform;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(content.GetChild(i).gameObject);
            }
            int position = 0;
            int[] order = NearbyStorageCategory.DisplayOrder();
            for (int family = 0; family < NearbyStorageCategoryFamily.Count; family++)
            {
                AddRow(content, position++, NearbyStorageCategoryFamily.Label(family), true);
                foreach (int category in order)
                {
                    if (category < 0 || NearbyStorageCategoryFamily.For(category) != family)
                    {
                        continue;
                    }
                    AddRow(content, position++, NearbyStorageCategory.Label(category), false);
                }
            }
            content.sizeDelta = new Vector2(MenuWidth - ScrollbarWidth,
                position * RowHeight);
            content.anchoredPosition = Vector2.zero;
        }

        // Adds one clickable family or category row.
        private static void AddRow(RectTransform content, int position, string label, bool family)
        {
            RectTransform row = Rect("Suggestion " + position, content,
                new Vector2(MenuWidth - ScrollbarWidth, RowHeight));
            row.anchorMin = row.anchorMax = new Vector2(0f, 1f);
            row.pivot = new Vector2(0f, 1f);
            row.anchoredPosition = new Vector2(0f, -position * RowHeight);
            Image image = row.gameObject.AddComponent<Image>();
            image.color = Color.white;
            Button button = row.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = new Color(1f, 0.62f, 0.25f, 0.32f);
            colors.pressedColor = new Color(1f, 0.62f, 0.25f, 0.48f);
            colors.selectedColor = Color.clear;
            button.colors = colors;
            button.onClick.AddListener(() => AppendLabel(label));
            RectTransform textRect = Rect("Label", row, Vector2.zero);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(family ? 9f : 24f, 0f);
            textRect.offsetMax = new Vector2(-6f, 0f);
            TextMeshProUGUI text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = _topic.font;
            text.fontSize = 18f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.text = label;
            text.raycastTarget = false;
        }

        // Adds the chosen label after the current name and returns focus to typing.
        private static void AppendLabel(string label)
        {
            if (_input == null) { return; }
            string current = _input.text.TrimEnd().TrimEnd(',').TrimEnd();
            _input.text = current.Length == 0 ? label : current + ", " + label;
            HideMenu();
            _input.ActivateInputField();
            _owner.StartCoroutine(PlaceCaretAtEnd());
        }

        // Places the caret after Unity finishes restoring input focus.
        private static IEnumerator PlaceCaretAtEnd()
        {
            yield return null;
            if (_input == null || !IsOpen) { yield break; }
            _input.MoveTextEnd(false);
            _input.caretPosition = _input.text.Length;
        }

        // Creates a UI rectangle under the requested parent.
        private static RectTransform Rect(string name, Transform parent, Vector2 size)
        {
            GameObject item = new GameObject(name, typeof(RectTransform));
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = size;
            return rect;
        }
    }
}
