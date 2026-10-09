using UnityEngine;
using UnityEngine.EventSystems;

namespace Landoria.NearbyStorage
{
    // Cancels a held nearby stack when the panel background is clicked.
    internal sealed class NearbyStorageDragCancel : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) { return; }
            if (NearbyStorageTransfer.CancelNearbyDrag()) { eventData.Use(); }
        }
    }
}
