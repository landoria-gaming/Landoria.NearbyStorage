using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Shows items in nearby chests in vanilla's ingredient availability indicator.
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupRequirement))]
    internal static class RequirementDisplayPatch
    {
        // Updates the RequirementDisplayPatch result after Valheim runs it.
        private static void Postfix(Transform elementRoot, Piece.Requirement req, Player player,
            bool craft, int quality, int craftMultiplier, bool __result)
        {
            if (!__result || player != Player.m_localPlayer || req?.m_resItem == null)
            {
                return;
            }

            bool nearbyCraft = craft && CraftState.Checking;
            bool nearbyBuild = player.InPlaceMode() && NearbyStorageActionInput.IsHeld();
            if (!nearbyCraft && !nearbyBuild)
            {
                return;
            }

            string name = req.m_resItem.m_itemData.m_shared.m_name;
            int available = new StorageSupply(player).Available(null, name, -1);

            if (available >= req.GetAmount(quality) * craftMultiplier)
            {
                Transform amount = elementRoot.Find("res_amount");
                if (amount != null)
                {
                    amount.GetComponent<TMP_Text>().color = Color.white;
                }
            }
        }
    }
}
