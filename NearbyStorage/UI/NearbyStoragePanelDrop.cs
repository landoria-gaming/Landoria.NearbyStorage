using UnityEngine;
using UnityEngine.EventSystems;

namespace Landoria.NearbyStorage
{
    // Accepts inventory drags on empty slots and elsewhere in the panel.
    internal sealed class NearbyStoragePanelDrop : MonoBehaviour, IPointerDownHandler, IDropHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left &&
                NearbyStorageTransfer.TryDropInPanel())
            {
                eventData.Use();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (NearbyStorageTransfer.TryDropInPanel()) { eventData.Use(); }
        }
    }
}
