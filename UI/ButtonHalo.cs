using UnityEngine;
using UnityEngine.UI;

namespace Landoria.SuperStorage
{
    // Tints Valheim's own button_glow sprite for the two Super actions.
    internal static class ButtonHalo
    {
        private static Image _stack;
        private static Image _craft;
        private static Sprite _glowSprite;

        // Updates both highlights from the current UI and keyboard state.
        internal static void Update()
        {
            bool active = InventoryGui.IsVisible() && Player.m_localPlayer != null &&
                SuperActionInput.IsHeld();
            InventoryGui gui = InventoryGui.instance;
            if (gui == null)
            {
                Clear();
                return;
            }

            Set(ref _stack, gui.m_stackAllButton, active && gui.IsContainerOpen());
            Set(ref _craft, gui.m_craftButton, active && gui.m_craftButton.interactable);
        }

        internal static void Clear()
        {
            if (_stack != null) _stack.enabled = false;
            if (_craft != null) _craft.enabled = false;
        }

        internal static void Dispose()
        {
            if (_stack != null) Object.Destroy(_stack.gameObject);
            if (_craft != null) Object.Destroy(_craft.gameObject);
            _stack = null;
            _craft = null;
            _glowSprite = null;
        }

        private static void Set(ref Image glow, Button button, bool active)
        {
            if (button == null || button.targetGraphic == null)
            {
                if (glow != null) glow.enabled = false;
                return;
            }

            if (!active || !button.gameObject.activeInHierarchy)
            {
                if (glow != null) glow.enabled = false;
                return;
            }

            if (_glowSprite == null)
            {
                foreach (Sprite sprite in Resources.FindObjectsOfTypeAll<Sprite>())
                {
                    if (sprite.name == "button_glow")
                    {
                        _glowSprite = sprite;
                        break;
                    }
                }
                if (_glowSprite == null) return;
            }

            if (glow == null || glow.transform.parent != button.targetGraphic.transform)
            {
                if (glow != null) Object.Destroy(glow.gameObject);
                GameObject overlay = new GameObject("SuperStorageGlow", typeof(RectTransform), typeof(Image));
                overlay.transform.SetParent(button.targetGraphic.transform, false);
                overlay.transform.SetAsFirstSibling();
                RectTransform rect = (RectTransform)overlay.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                glow = overlay.GetComponent<Image>();
                glow.sprite = _glowSprite;
                glow.type = Image.Type.Sliced;
                glow.raycastTarget = false;
            }

            glow.enabled = true;
            float alpha = 0.5f + 0.15f * Mathf.Sin(Time.unscaledTime * 3f);
            glow.color = new Color(1f, 183f / 255f, 91f / 255f, alpha);
        }
    }
}
