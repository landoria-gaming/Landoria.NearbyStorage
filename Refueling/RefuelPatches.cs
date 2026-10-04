using HarmonyLib;

namespace Landoria.SuperStorage
{
    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Interact))]
    internal static class FireplaceRefuelPatch
    {
        private static void Prefix(Fireplace __instance, Humanoid user, ref bool alt)
        {
            if (!SuperActionInput.IsHeld() || user != Player.m_localPlayer ||
                !__instance.m_canRefill || __instance.m_infiniteFuel) return;
            alt = true;
            RefuelContext.Begin(user, __instance.m_fuelItem);
        }

        private static void Finalizer() => RefuelContext.Reset();
    }

    [HarmonyPatch(typeof(Smelter), "OnAddFuel")]
    internal static class SmelterRefuelPatch
    {
        private static void Prefix(Smelter __instance, Humanoid user, ItemDrop.ItemData item)
        {
            if (item == null) RefuelContext.Begin(user, __instance.m_fuelItem);
        }

        private static void Finalizer() => RefuelContext.Reset();
    }

    [HarmonyPatch(typeof(CookingStation), "OnAddFuelSwitch")]
    internal static class CookingRefuelPatch
    {
        private static void Prefix(CookingStation __instance, Humanoid user, ItemDrop.ItemData item)
        {
            if (item == null) RefuelContext.Begin(user, __instance.m_fuelItem);
        }

        private static void Finalizer() => RefuelContext.Reset();
    }

    [HarmonyPatch(typeof(ShieldGenerator), "OnAddFuel")]
    internal static class ShieldRefuelPatch
    {
        private static void Prefix(ShieldGenerator __instance, Humanoid user, ItemDrop.ItemData item)
        {
            if (item == null) RefuelContext.Begin(user, __instance.m_fuelItems);
        }

        private static void Finalizer() => RefuelContext.Reset();
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.HaveItem), typeof(string), typeof(bool))]
    internal static class RefuelHaveItemPatch
    {
        private static void Postfix(Inventory __instance, string name, ref bool __result)
        {
            if (!__result && RefuelContext.Matches(__instance, name)) __result = true;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem),
        typeof(string), typeof(int), typeof(int), typeof(bool))]
    internal static class RefuelRemoveItemPatch
    {
        private static bool Prefix(Inventory __instance, string name, int amount)
        {
            if (amount != 1 || !RefuelContext.Matches(__instance, name)) return true;
            return !RefuelContext.Withdraw();
        }
    }
}
