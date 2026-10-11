using HarmonyLib;

namespace Landoria.NearbyStorage
{
    // Prevents mouse wheel input from zooming the camera during our rename dialog.
    [HarmonyPatch]
    internal static class RenameCameraScrollPatch
    {
        private static bool _readingCameraScroll;

        // Marks camera input while the name editor is visible.
        [HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
        [HarmonyPrefix]
        private static void BeforeCamera()
        {
            _readingCameraScroll = RenameDialog.IsOpen;
        }

        // Restores mouse wheel input for UI scrolling after the camera update.
        [HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
        [HarmonyPostfix]
        private static void AfterCamera()
        {
            _readingCameraScroll = false;
        }

        // Gives the camera no wheel movement while the rename dialog is open.
        [HarmonyPatch(typeof(ZInput), nameof(ZInput.GetMouseScrollWheel))]
        [HarmonyPrefix]
        private static bool CameraWheel(ref float __result)
        {
            if (!_readingCameraScroll) { return true; }
            __result = 0f;
            return false;
        }
    }
}
