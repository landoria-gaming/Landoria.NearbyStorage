using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Holds the UI elements and fade deadline for one confirmed deposit.
    internal sealed class NearbyStorageDepositNotice
    {
        internal RectTransform Root;
        internal CanvasGroup Group;
        internal Image ItemIcon;
        internal TextMeshProUGUI Count;
        internal Image ChestIcon;
        internal TextMeshProUGUI ChestName;
        internal float Until;
    }
}
