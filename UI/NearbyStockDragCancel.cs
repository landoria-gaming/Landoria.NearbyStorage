using UnityEngine;
using UnityEngine.EventSystems;

namespace Landoria.SuperStorage
{
    // Cancels a held nearby stack when the stock panel is clicked again.
    internal sealed class NearbyStockDragCancel : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) { return; }
            NearbyStockTransfer.CancelNearbyDrag();
            eventData.Use();
        }
    }
}
