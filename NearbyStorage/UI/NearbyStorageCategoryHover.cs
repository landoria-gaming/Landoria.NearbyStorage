using UnityEngine;
using UnityEngine.EventSystems;

namespace Landoria.NearbyStorage
{
    // Colors category hover directly without Unity's button tint transition.
    internal sealed class NearbyStorageCategoryHover : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        internal int Index;

        public void OnPointerEnter(PointerEventData eventData)
        {
            NearbyStorageDialog.SetCategoryHover(Index, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            NearbyStorageDialog.SetCategoryHover(Index, false);
        }
    }
}
