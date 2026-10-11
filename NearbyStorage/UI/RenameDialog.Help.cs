using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Displays the category sorting hint beside the rename dialog.
    internal static partial class RenameDialog
    {
        private const float CategoryHelpWidth = 370f;

        // Creates a dark popup below the category button.
        private static void CreateCategoryHelp(RectTransform panel, RectTransform button)
        {
            Vector3[] corners = new Vector3[4];
            button.GetWorldCorners(corners);
            Vector3 right = panel.InverseTransformPoint(corners[3]);
            RectTransform root = Rect("Category help", panel,
                new Vector2(CategoryHelpWidth, 80f));
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(1f, 1f);
            root.anchoredPosition = new Vector2(right.x, right.y - 5f);
            Image background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.947f);
            background.raycastTarget = false;
            RectTransform label = Rect("Text", root, Vector2.zero);
            label.anchorMin = Vector2.zero;
            label.anchorMax = Vector2.one;
            label.offsetMin = new Vector2(12f, 10f);
            label.offsetMax = new Vector2(-12f, -10f);
            _categoryHelpText = label.gameObject.AddComponent<TextMeshProUGUI>();
            _categoryHelpText.font = _topic.font;
            _categoryHelpText.fontSize = 18f;
            _categoryHelpText.color = Color.white;
            _categoryHelpText.textWrappingMode = TextWrappingModes.Normal;
            _categoryHelpText.raycastTarget = false;
            _categoryHelp = root;
            HideCategoryHelp();
        }

        // Shows the localized hint with enough room for wrapped text.
        internal static void ShowCategoryHelp()
        {
            if (_categoryHelp == null || !IsOpen || _menu?.activeSelf == true) { return; }
            string message = ModText.Get("CategoryButtonHelp");
            _categoryHelpText.text = message;
            float height = _categoryHelpText.GetPreferredValues(message,
                CategoryHelpWidth - 24f, Mathf.Infinity).y;
            _categoryHelp.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Max(50f, height + 20f));
            _categoryHelp.gameObject.SetActive(true);
            _categoryHelp.SetAsLastSibling();
        }

        // Hides the hint without changing the name or menu.
        internal static void HideCategoryHelp()
        {
            if (_categoryHelp != null) { _categoryHelp.gameObject.SetActive(false); }
        }
    }
}
