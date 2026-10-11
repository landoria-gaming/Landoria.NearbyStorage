using UnityEngine;
using UnityEngine.EventSystems;

namespace Landoria.NearbyStorage
{
    // Shows a short explanation while the pointer rests on Categories.
    internal sealed class CategoryButtonHover : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        // Opens the help popup on pointer entry.
        public void OnPointerEnter(PointerEventData eventData)
        {
            RenameDialog.ShowCategoryHelp();
        }

        // Closes the help popup on pointer exit.
        public void OnPointerExit(PointerEventData eventData)
        {
            RenameDialog.HideCategoryHelp();
        }
    }
}
