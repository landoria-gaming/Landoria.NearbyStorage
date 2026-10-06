using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Forwards pointer hover from a stock card to its tooltip.
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
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (!NearbyStorageTransfer.TryDropOn(Item))
                {
                    NearbyStorageTransfer.Select(Item);
                }
            }
        }

        // Accepts a held inventory drag over the same stock item.
        public void OnDrop(PointerEventData eventData)
        {
            if (NearbyStorageTransfer.TryDropOn(Item)) { eventData.Use(); }
        }

        // Shows the contributing containers while the pointer is over this card.
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Background != null) { Background.color = HoverColor; }
            NearbyStorageDialog.ShowTooltip(Item);
        }

        // Hides the card's contribution tooltip.
        public void OnPointerExit(PointerEventData eventData)
        {
            if (Background != null) { Background.color = IdleColor; }
            NearbyStorageDialog.HideTooltip();
        }
    }
}
