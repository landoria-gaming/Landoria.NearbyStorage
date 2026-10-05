using UnityEngine;
using UnityEngine.EventSystems;

namespace Landoria.SuperStorage
{
    // Forwards pointer hover from a stock card to its tooltip.
    internal sealed class NearbyStockCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        internal NearbyStockItem Item;

        // Shows the contributing containers while the pointer is over this card.
        public void OnPointerEnter(PointerEventData eventData)
        {
            NearbyStockDialog.ShowTooltip(Item);
        }

        // Hides the card's contribution tooltip.
        public void OnPointerExit(PointerEventData eventData)
        {
            NearbyStockDialog.HideTooltip();
        }
    }
}
