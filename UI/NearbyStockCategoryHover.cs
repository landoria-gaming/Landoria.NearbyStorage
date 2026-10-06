using UnityEngine;
using UnityEngine.EventSystems;

namespace Landoria.SuperStorage
{
    // Colors category hover directly without Unity's button tint transition.
    internal sealed class NearbyStockCategoryHover : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        internal int Index;

        public void OnPointerEnter(PointerEventData eventData)
        {
            NearbyStockDialog.SetCategoryHover(Index, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            NearbyStockDialog.SetCategoryHover(Index, false);
        }
    }
}
