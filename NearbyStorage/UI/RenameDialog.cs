using GUIFramework;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Copies Valheim's text dialog for Nearby Storage name editing.
    [HarmonyPatch]
    internal static partial class RenameDialog
    {
        private const float MoreButtonWidth = 160f;
        private const float InputButtonGap = 12f;
        private const float ExtraDialogHeight = 24f;
        private static GameObject _originalPanel;
        private static TMP_Text _originalTopic;
        private static GuiInputField _originalInput;
        private static GameObject _panel;
        private static TMP_Text _topic;
        private static GuiInputField _input;
        private static TextInput _owner;
        private static GameObject _menu;
        private static GameObject _menuBlocker;
        private static RectTransform _categoryHelp;
        private static TextMeshProUGUI _categoryHelpText;
        internal static bool IsOpen => _owner != null && _panel != null &&
            _owner.m_panel == _panel && _panel.activeInHierarchy;

        // Creates the editable copy before opening one of our name editors.
        [HarmonyPatch(typeof(TextInput), nameof(TextInput.RequestText))]
        [HarmonyPrefix]
        private static void RequestText(TextInput __instance, TextReceiver sign)
        {
            if (_owner != __instance) { Reset(__instance); }
            bool ours = sign is ChestRename;
            if (ours && _panel == null) { Create(__instance); }
            if (_panel == null) { return; }
            if (ours)
            {
                SetCategoryButtonText();
                SetInputExample();
            }
            HideMenu();
            HideCategoryHelp();
            if (__instance.m_panel.activeSelf) { __instance.Hide(); }
            __instance.m_panel = ours ? _panel : _originalPanel;
            __instance.m_topic = ours ? _topic : _originalTopic;
            __instance.m_inputField = ours ? _input : _originalInput;
        }

        // Saves the original controls when Valheim creates a new text dialog.
        private static void Reset(TextInput owner)
        {
            _owner = owner;
            _originalPanel = owner.m_panel;
            _originalTopic = owner.m_topic;
            _originalInput = owner.m_inputField;
            _panel = null;
            _topic = null;
            _input = null;
            _menu = null;
            _menuBlocker = null;
            _categoryHelp = null;
            _categoryHelpText = null;
        }

        // Clones the panel and locates matching controls inside the copy.
        private static void Create(TextInput owner)
        {
            if (_originalPanel == null || _originalTopic == null || _originalInput == null)
            {
                return;
            }
            GameObject copy = Object.Instantiate(_originalPanel, _originalPanel.transform.parent);
            copy.name = "NearbyStorageRenameDialog";
            copy.SetActive(false);
            TMP_Text topic = CopyControl(_originalTopic.transform, copy.transform)?.GetComponent<TMP_Text>();
            GuiInputField input = CopyControl(_originalInput.transform, copy.transform)?
                .GetComponent<GuiInputField>();
            if (topic == null || input == null)
            {
                Object.Destroy(copy);
                return;
            }
            _panel = copy;
            _topic = topic;
            _input = input;
            Style(copy.GetComponent<RectTransform>(), input);
        }

        // Finds a cloned control using its relative path in the native panel.
        private static Transform CopyControl(Transform source, Transform copy)
        {
            string path = source.name;
            Transform current = source.parent;
            while (current != _originalPanel.transform)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return copy.Find(path);
        }

        // Makes room beside the input and adds a matching category button.
        private static void Style(RectTransform panel, GuiInputField input)
        {
            if (panel == null) { return; }
            panel.localScale = Vector3.one;
            SpaceDialogVertically(panel);
            CenterActionButtons(panel);
            RectTransform field = input.transform as RectTransform;
            Button nativeButton = panel.GetComponentInChildren<Button>(true);
            if (field == null || nativeButton == null) { return; }
            GameObject copy = Object.Instantiate(nativeButton.gameObject, field.parent, false);
            copy.name = "CategoryButton";
            Button button = copy.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(ToggleMenu);
            copy.AddComponent<CategoryButtonHover>();
            StyleButtonText(copy);
            foreach (Button action in panel.GetComponentsInChildren<Button>(true))
            {
                if (action != button) { StyleButtonText(action.gameObject); }
            }
            RectTransform more = copy.transform as RectTransform;
            more.anchorMin = new Vector2(field.anchorMax.x, field.anchorMin.y);
            more.anchorMax = field.anchorMax;
            more.offsetMin = new Vector2(field.offsetMax.x - MoreButtonWidth,
                field.offsetMin.y - 4f);
            more.offsetMax = new Vector2(field.offsetMax.x, field.offsetMax.y + 4f);
            field.offsetMax = new Vector2(field.offsetMax.x - MoreButtonWidth - InputButtonGap,
                field.offsetMax.y);
            LayoutElement layout = copy.GetComponent<LayoutElement>() ??
                copy.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            copy.SetActive(true);
            CreateMenu(panel, more);
            CreateCategoryHelp(panel, more);
        }

        // Adds breathing room above and below the input field.
        private static void SpaceDialogVertically(RectTransform panel)
        {
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                panel.rect.height + ExtraDialogHeight);
            _topic.transform.position += panel.TransformVector(new Vector3(0f, 8f, 0f));
            foreach (Button action in panel.GetComponentsInChildren<Button>(true))
            {
                action.transform.position += panel.TransformVector(new Vector3(0f, -8f, 0f));
            }
        }

        // Uses the same slightly larger text size on all three dialog buttons.
        private static void StyleButtonText(GameObject button)
        {
            foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>(true))
            {
                text.fontSize += 2f;
                text.fontSizeMax = text.fontSize;
            }
            foreach (Text text in button.GetComponentsInChildren<Text>(true))
            {
                text.fontSize += 2;
                text.resizeTextMaxSize = text.fontSize;
            }
        }

        // Makes the native action buttons equally wide and centers them as a pair.
        private static void CenterActionButtons(RectTransform panel)
        {
            Button[] actions = panel.GetComponentsInChildren<Button>(true);
            if (actions.Length != 2) { return; }
            RectTransform first = actions[0].transform as RectTransform;
            RectTransform second = actions[1].transform as RectTransform;
            if (first == null || second == null) { return; }
            if (LeftEdge(panel, first) > LeftEdge(panel, second))
            {
                RectTransform swap = first;
                first = second;
                second = swap;
            }
            float width = first.rect.width;
            float gap = LeftEdge(panel, second) - RightEdge(panel, first);
            second.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            float left = panel.rect.center.x - (2f * width + gap) / 2f;
            MoveLeftEdge(panel, first, left);
            MoveLeftEdge(panel, second, left + width + gap);
        }

        // Reads the left edge of a control in panel coordinates.
        private static float LeftEdge(RectTransform panel, RectTransform control)
        {
            Vector3[] corners = new Vector3[4];
            control.GetWorldCorners(corners);
            return panel.InverseTransformPoint(corners[0]).x;
        }

        // Reads the right edge of a control in panel coordinates.
        private static float RightEdge(RectTransform panel, RectTransform control)
        {
            Vector3[] corners = new Vector3[4];
            control.GetWorldCorners(corners);
            return panel.InverseTransformPoint(corners[3]).x;
        }

        // Moves a control horizontally without changing its height.
        private static void MoveLeftEdge(RectTransform panel, RectTransform control,
            float target)
        {
            float shift = target - LeftEdge(panel, control);
            control.position += panel.TransformVector(new Vector3(shift, 0f, 0f));
        }

        // Updates the button label when Valheim's selected language changes.
        private static void SetCategoryButtonText()
        {
            Transform button = _input.transform.parent.Find("CategoryButton");
            if (button == null) { return; }
            string label = ModText.Get("ItemCategories");
            foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>(true))
            {
                text.text = label;
                text.enableAutoSizing = true;
                text.fontSizeMin = 14f;
            }
            foreach (Text text in button.GetComponentsInChildren<Text>(true))
            {
                text.text = label;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 14;
            }
        }

        // Shows a localized category example while the name field is empty.
        private static void SetInputExample()
        {
            if (!(_input.placeholder is TMP_Text placeholder)) { return; }
            string language = ModText.SelectedLanguage;
            string wood = ModText.CategoryText("CategoryWood", language);
            string stone = ModText.CategoryText("CategoryStone", language);
            placeholder.text = ModText.Get("ExamplePrefix") + ": " + wood + ", " + stone;
        }
    }
}
