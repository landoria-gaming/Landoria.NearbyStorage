using UnityEngine;
using UnityEngine.EventSystems;

namespace Landoria.NearbyStorage
{
    // Cancels a held nearby stack when the nearby storage panel is clicked again.
    internal sealed class NearbyStorageDragCancel : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) { return; }
            NearbyStorageTransfer.CancelNearbyDrag();
            eventData.Use();
        }
    }
}
