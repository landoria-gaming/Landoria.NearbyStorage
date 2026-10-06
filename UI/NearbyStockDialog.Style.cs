using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.SuperStorage
{
    // Copies Valheim's live inventory graphics and slot dimensions.
    internal static partial class NearbyStockDialog
    {
        private static readonly FieldInfo SearchField = AccessTools.Field(typeof(BuildUi), "m_searchField");
        private static readonly FieldInfo TagScroll = AccessTools.Field(typeof(BuildUi), "m_tagListScrollRect");

        // Applies the inventory panel's native frame sprite and tint.
        private static void StylePanel(Image image)
        {
            Image source = _gui.m_player.Find("Bkg")?.GetComponent<Image>() ??
                _gui.m_container.Find("Bkg")?.GetComponent<Image>();
            CopyImage(source, image);
        }

        // Applies the build menu's native filter-field graphic.
        private static void StyleFilter(Image image)
        {
            BuildUi ui = Hud.instance?.m_buildUi;
            Component field = ui == null ? null : SearchField?.GetValue(ui) as Component;
            CopyImage(field?.GetComponent<Image>(), image);
        }

        // Mirrors the build filter's pointer and focus highlights.
        private static void StyleFilterInteraction(TMP_InputField target, Image background)
        {
            BuildUi ui = Hud.instance?.m_buildUi;
            Component field = ui == null ? null : SearchField?.GetValue(ui) as Component;
            TMP_InputField source = field?.GetComponent<TMP_InputField>();
            target.targetGraphic = background;
            target.transition = Selectable.Transition.ColorTint;
            if (source != null)
            {
                target.colors = source.colors;
            }
            else
            {
                ColorBlock colors = target.colors;
                colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
                colors.selectedColor = colors.highlightedColor;
                target.colors = colors;
            }
        }

        // Copies the build menu's filter placeholder typography.
        private static void StyleFilterHint(TextMeshProUGUI target)
        {
            BuildUi ui = Hud.instance?.m_buildUi;
            Component source = ui == null ? null : SearchField?.GetValue(ui) as Component;
            TMP_Text hint = source?.GetComponent<TMP_InputField>()?.placeholder as TMP_Text;
            if (hint != null)
            {
                target.font = hint.font;
                target.fontSharedMaterial = hint.fontSharedMaterial;
                target.fontSize = hint.fontSize;
                target.fontStyle = hint.fontStyle;
                target.color = hint.color;
                return;
            }
            target.fontStyle = FontStyles.Italic;
            target.color = new Color(1f, 1f, 1f, 0.45f);
        }

        // Reuses the selected recipe row graphic for the active category.
        private static void StyleCategorySelection(Image target)
        {
            Image source = _gui?.m_recipeElementPrefab?.transform.Find("selected")?.
                GetComponent<Image>();
            if (source == null)
            {
                target.color = new Color(0.76f, 0.46f, 0.15f, 0.95f);
            }
            else
            {
                CopyImage(source, target);
            }
            target.raycastTarget = false;
        }

        // Matches the text size and font of a vanilla crafting recipe row.
        private static void StyleCategoryText(TextMeshProUGUI target)
        {
            TMP_Text source = _gui?.m_recipeElementPrefab?.transform.Find("name")?
                .GetComponent<TMP_Text>();
            if (source == null || source.font == null) { return; }
            target.font = source.font;
            target.fontSharedMaterial = source.fontSharedMaterial;
            target.fontSize = source.fontSize;
            target.enableAutoSizing = source.enableAutoSizing;
            target.fontSizeMin = source.fontSizeMin;
            target.fontSizeMax = source.fontSizeMax;
            target.fontStyle = source.fontStyle;
        }

        // Applies the native item-slot background to one stock card.
        private static void StyleSlot(Image image)
        {
            InventoryElement element = SlotElement();
            CopyImage(element?.m_button.targetGraphic as Image, image);
            image.color = element?.m_button != null ? element.m_button.colors.normalColor :
                new Color(0.13235295f, 0.13235295f, 0.13235295f, 0.5019608f);
        }

        // Matches the native item icon's inset and image settings inside a slot.
        private static void StyleIcon(Image target)
        {
            Image source = SlotElement()?.m_icon;
            if (source == null) { return; }
            CopyImage(source, target);
            MatchIconRect(source.rectTransform, target.rectTransform,
                SlotElement().transform as RectTransform);
            target.color = Color.white;
            CopyIconShadows(source, target);
        }

        // Converts the icon's full prefab hierarchy into slot-local dimensions.
        private static void MatchIconRect(RectTransform source, RectTransform target,
            RectTransform slot)
        {
            if (slot == null) { return; }
            Vector3[] corners = new Vector3[4];
            source.GetWorldCorners(corners);
            Vector3 bottomLeft = slot.InverseTransformPoint(corners[0]);
            Vector3 topRight = slot.InverseTransformPoint(corners[2]);
            if (topRight.x <= bottomLeft.x || topRight.y <= bottomLeft.y) { return; }
            target.anchorMin = target.anchorMax = new Vector2(0f, 1f);
            target.pivot = new Vector2(0f, 1f);
            target.anchoredPosition = new Vector2(bottomLeft.x - slot.rect.xMin,
                topRight.y - slot.rect.yMax);
            target.sizeDelta = new Vector2(topRight.x - bottomLeft.x,
                topRight.y - bottomLeft.y);
        }

        // Reuses the inventory icon's mesh shadow effects.
        private static void CopyIconShadows(Image source, Image target)
        {
            Shadow[] originals = source.GetComponents<Shadow>();
            foreach (Shadow original in originals)
            {
                Shadow copy = target.gameObject.AddComponent(original.GetType()) as Shadow;
                if (copy == null) { continue; }
                copy.effectColor = original.effectColor;
                copy.effectDistance = original.effectDistance;
                copy.useGraphicAlpha = original.useGraphicAlpha;
            }
            if (originals.Length == 0)
            {
                Shadow shadow = target.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
                shadow.effectDistance = new Vector2(2f, -2f);
            }
        }

        // Returns the native inventory slot prefab's component.
        private static InventoryElement SlotElement()
        {
            return _gui?.m_playerGrid?.m_elementPrefab?.GetComponent<InventoryElement>();
        }

        // Uses the game's item-grid spacing for both card dimensions and placement.
        private static float SlotStep()
        {
            float step = _gui?.m_playerGrid == null ? 0f : _gui.m_playerGrid.m_elementSpace;
            return step > 0f ? step : 64f;
        }

        // Uses the game's item-slot width, clamped to its grid spacing.
        private static float SlotSize()
        {
            InventoryElement element = SlotElement();
            RectTransform rect = element == null ? null : element.transform as RectTransform;
            return rect != null && rect.rect.width > 0f ? rect.rect.width : SlotStep();
        }

        // Copies sprite and slicing settings from a native UI image.
        private static void CopyImage(Image source, Image target)
        {
            if (source == null || target == null) { return; }
            target.sprite = source.sprite;
            target.type = source.type;
            target.color = source.color;
            target.material = source.material;
            target.preserveAspect = source.preserveAspect;
            target.fillCenter = source.fillCenter;
            target.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        }

        // Copies the game's visible scrollbar track and handle graphics.
        private static void StyleScrollbar(Image track, Image handle)
        {
            BuildUi ui = Hud.instance?.m_buildUi;
            ScrollRect buildScroll = ui == null ? null : TagScroll?.GetValue(ui) as ScrollRect;
            Scrollbar source = buildScroll?.verticalScrollbar ??
                _gui?.ContainerGrid?.m_scrollbar ??
                _gui?.m_playerGrid?.m_scrollbar;
            if (source == null) { return; }
            CopyImage(source.GetComponent<Image>(), track);
            CopyImage(source.handleRect?.GetComponent<Image>(), handle);
        }

        // Copies the game's item-count typography.
        private static void StyleCount(TextMeshProUGUI target)
        {
            TMP_Text source = SlotElement()?.m_amount;
            if (source == null) { return; }
            target.font = source.font;
            target.fontSharedMaterial = source.fontSharedMaterial;
            target.fontSize = source.fontSize;
            target.color = source.color;
        }
    }
}
