using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.SuperStorage
{
    // Shows recent storage transfers in white below the player's inventory panel.
    internal static class InventoryMoveFeed
    {
        private const float HoldDuration = 5f;
        private const float FadeDuration = 1f;
        private const float Gap = 3f;

        private sealed class Entry
        {
            internal TextMeshProUGUI Text;
            internal float Started;
        }

        private static readonly List<Entry> Entries = new List<Entry>();
        private static RectTransform _root;
        private static InventoryGui _gui;

        // Adds one line for a chest that actually received items.
        internal static void Add(string message)
        {
            if (!EnsureRoot()) return;
            GameObject line = new GameObject("SuperStorageMove", typeof(RectTransform), typeof(TextMeshProUGUI));
            line.transform.SetParent(_root, false);
            RectTransform rect = (RectTransform)line.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            TextMeshProUGUI text = line.GetComponent<TextMeshProUGUI>();
            text.font = _gui.m_containerName.font;
            text.fontSharedMaterial = _gui.m_containerName.fontSharedMaterial;
            text.fontSize = 18f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            text.text = message;
            Entries.Add(new Entry { Text = text, Started = Time.unscaledTime });
            Update();
        }

        // Tracks the inventory panel and fades each line over its final second.
        internal static void Update()
        {
            if (_root == null || _gui == null) return;
            for (int i = Entries.Count - 1; i >= 0; i--)
            {
                float age = Time.unscaledTime - Entries[i].Started;
                if (age < HoldDuration + FadeDuration) continue;
                Object.Destroy(Entries[i].Text.gameObject);
                Entries.RemoveAt(i);
            }

            _root.gameObject.SetActive(InventoryGui.IsVisible() && Entries.Count > 0);
            if (!_root.gameObject.activeSelf) return;
            PositionRoot();
            float offset = 0f;
            foreach (Entry entry in Entries)
            {
                float age = Time.unscaledTime - entry.Started;
                entry.Text.color = new Color(1f, 1f, 1f,
                    Mathf.Clamp01((HoldDuration + FadeDuration - age) / FadeDuration));
                RectTransform rect = (RectTransform)entry.Text.transform;
                float height = Mathf.Max(22f,
                    entry.Text.GetPreferredValues(entry.Text.text, _root.rect.width, Mathf.Infinity).y);
                rect.sizeDelta = new Vector2(_root.rect.width, height);
                rect.anchoredPosition = new Vector2(0f, -offset);
                offset += height + Gap;
            }
        }

        // Creates an overlay under the inventory root without intercepting input.
        private static bool EnsureRoot()
        {
            InventoryGui gui = InventoryGui.instance;
            if (gui == null || gui.m_inventoryRoot == null || gui.m_containerName == null) return false;
            if (_root != null && _gui == gui) return true;
            Dispose();
            _gui = gui;
            GameObject overlay = new GameObject("SuperStorageMoveFeed", typeof(RectTransform), typeof(LayoutElement));
            Transform parent = gui.m_inventoryRoot.parent != null ?
                gui.m_inventoryRoot.parent : gui.transform;
            overlay.transform.SetParent(parent, false);
            overlay.transform.SetAsLastSibling();
            overlay.GetComponent<LayoutElement>().ignoreLayout = true;
            _root = (RectTransform)overlay.transform;
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.zero;
            _root.pivot = new Vector2(0f, 1f);
            PositionRoot();
            return true;
        }

        // Places the first line just below the player inventory panel.
        private static void PositionRoot()
        {
            if (_root == null || _gui.m_player == null) return;
            Vector3[] corners = new Vector3[4];
            _gui.m_player.GetWorldCorners(corners);
            Vector3 bottomLeft = _root.parent.InverseTransformPoint(corners[0]);
            _root.localPosition = bottomLeft + new Vector3(0f, -8f, 0f);
            _root.sizeDelta = new Vector2(_gui.m_player.rect.width, 0f);
        }

        // Removes transient UI when the plugin unloads or the inventory is replaced.
        internal static void Dispose()
        {
            if (_root != null) Object.Destroy(_root.gameObject);
            _root = null;
            _gui = null;
            Entries.Clear();
        }
    }
}
