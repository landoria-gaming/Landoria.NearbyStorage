using UnityEngine;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Tints Valheim's own button_glow sprite for Nearby Storage Craft.
    internal static class ButtonHalo
    {
        private static Image _craft;
        private static Sprite _glowSprite;

        // Updates the Craft highlight from the current UI and keyboard state.
        internal static void Update()
        {
            bool active = InventoryGui.IsVisible() && Player.m_localPlayer != null &&
                NearbyStorageActionInput.IsHeld();
            InventoryGui gui = InventoryGui.instance;
            if (gui == null)
            {
                Clear();
                return;
            }

            Set(ref _craft, gui.m_craftButton, active && gui.m_craftButton.interactable);
        }

        // Hides the active button glow.
        internal static void Clear()
        {
            if (_craft != null)
            {
                _craft.enabled = false;
            }
        }

        // Releases the button glow and its cached sprite.
        internal static void Dispose()
        {
            if (_craft != null)
            {
                Object.Destroy(_craft.gameObject);
            }

            _craft = null;
            _glowSprite = null;
        }

        // Updates the glow on the active button.
        private static void Set(ref Image glow, Button button, bool active)
        {
            if (button == null || button.targetGraphic == null)
            {
                if (glow != null)
                {
                    glow.enabled = false;
                }

                return;
            }

            if (!active || !button.gameObject.activeInHierarchy)
            {
                if (glow != null)
                {
                    glow.enabled = false;
                }

                return;
            }

            if (_glowSprite == null)
            {
                _glowSprite = FindGlowSprite();
                if (_glowSprite == null)
                {
                    return;
                }
            }

            if (glow == null || glow.transform.parent != button.targetGraphic.transform)
            {
                glow = CreateGlow(glow, button);
            }

            glow.enabled = true;
            float alpha = 0.5f + 0.15f * Mathf.Sin(Time.unscaledTime * 3f);
            glow.color = new Color(1f, 183f / 255f, 91f / 255f, alpha);
        }

        // Finds the button glow sprite in Valheim's loaded assets.
        private static Sprite FindGlowSprite()
        {
            foreach (Sprite sprite in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (sprite.name == "button_glow")
                {
                    return sprite;
                }
            }
            return null;
        }

        // Adds a glow image behind the button graphic.
        private static Image CreateGlow(Image existingGlow, Button button)
        {
            if (existingGlow != null)
            {
                Object.Destroy(existingGlow.gameObject);
            }

            GameObject overlay = new GameObject("NearbyStorageGlow", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(button.targetGraphic.transform, false);
            overlay.transform.SetAsFirstSibling();
            RectTransform rect = (RectTransform)overlay.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image glow = overlay.GetComponent<Image>();
            glow.sprite = _glowSprite;
            glow.type = Image.Type.Sliced;
            glow.raycastTarget = false;
            return glow;
        }
    }
}
