using UnityEngine;
using UnityEngine.EventSystems;

namespace Landoria.NearbyStorage
{
    // Accepts an automatic deposit on an unoccupied part of the item grid.
    internal sealed class NearbyStorageEmptyDropTarget : MonoBehaviour,
        IPointerDownHandler, IDropHandler
    {
        // Handles Valheim's click-to-place inventory drag.
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left &&
                NearbyStorageTransfer.TryDropInPanel(null)) { eventData.Use(); }
        }

        // Handles a released inventory drag over an empty grid cell.
        public void OnDrop(PointerEventData eventData)
        {
            if (NearbyStorageTransfer.TryDropInPanel(null)) { eventData.Use(); }
        }
    }
}
