using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Allows scrolling the rename menu only with the wheel or scrollbar.
    internal sealed class RenameMenuScrollRect : ScrollRect
    {
        // Ignores a drag starting on the list.
        public override void OnBeginDrag(PointerEventData eventData)
        {
        }

        // Ignores pointer movement over the list.
        public override void OnDrag(PointerEventData eventData)
        {
        }

        // Ignores a drag ending on the list.
        public override void OnEndDrag(PointerEventData eventData)
        {
        }
    }
}
