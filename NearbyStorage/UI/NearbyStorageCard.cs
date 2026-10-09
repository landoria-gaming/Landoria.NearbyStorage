using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Forwards pointer hover from a nearby item card to its tooltip.
    internal sealed class NearbyStorageCard : MonoBehaviour, IPointerDownHandler,
        IPointerEnterHandler, IPointerExitHandler, IDropHandler
    {
        internal Color IdleColor;
        private static readonly Color HoverColor = new Color(0.42f, 0.36f, 0.32f, 0.96f);
        internal NearbyStorageItem Item;
        internal Image Background;

        // Matches Valheim's inventory slots by acting when the mouse is pressed.
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (NearbyStorageTransfer.UseConsumable(Item)) { eventData.Use(); }
                return;
            }
            if (eventData.button == PointerEventData.InputButton.Left &&
                ZInput.GetKey(KeyCode.LeftAlt))
            {
                Container chest = Item?.Sources.Count > 0 ? Item.Sources[0].Chest : null;
                Player player = Player.m_localPlayer;
                if (chest != null && player != null && Chat.instance != null &&
                    Plugin.Instance != null && StorageLocator.Eligible(chest, player,
                        StorageLocator.CurrentContainer(), Plugin.Instance.Settings.Radius.Value))
                {
                    Chat.instance.SendPing(chest.transform.position);
                    eventData.Use();
                    Plugin.Instance.StartCoroutine(CloseAfterPing());
                }
                return;
            }
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (!NearbyStorageTransfer.TryDropInPanel(Item))
                {
                    NearbyStorageTransfer.Select(Item);
                }
            }
        }

        // Keeps the inventory open until the ping click ends and half a second passes.
        private static IEnumerator CloseAfterPing()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            while (ZInput.GetMouseButton(0)) { yield return null; }
            if (InventoryGui.IsVisible()) { InventoryGui.instance?.Hide(); }
        }

        // Accepts a player or nearby drag over this item's container.
        public void OnDrop(PointerEventData eventData)
        {
            if (NearbyStorageTransfer.TryDropInPanel(Item)) { eventData.Use(); }
        }

        // Highlights immediately and schedules the tooltip after a pause.
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Background != null) { Background.color = HoverColor; }
            NearbyStorageDialog.ScheduleTooltip(Item);
        }

        // Hides the card's contribution tooltip.
        public void OnPointerExit(PointerEventData eventData)
        {
            if (Background != null) { Background.color = IdleColor; }
            NearbyStorageDialog.HideTooltip();
        }
    }
}
