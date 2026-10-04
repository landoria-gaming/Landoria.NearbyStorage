using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Landoria.SuperStorage
{
    // Runs the native inventory stack action across nearby chests.
    internal static class SuperStack
    {
        private static readonly HashSet<Container> Pending = new HashSet<Container>();
        private static readonly HashSet<Container> Expired = new HashSet<Container>();
        internal static bool Running { get; private set; }
        internal static bool InStackCall { get; private set; }
        private static int _moved;

        // Starts one best-effort stacking pass.
        internal static IEnumerator Run()
        {
            Player player = Player.m_localPlayer;
            if (player == null || Running)
            {
                yield break;
            }

            Running = true;
            _moved = 0;
            try
            {
                List<Container> chests = StorageLocator.Nearby(player);
                Container current = StorageLocator.CurrentContainer();
                float radius = Plugin.Instance.Settings.Radius.Value;
                var sourceNames = new HashSet<string>();
                foreach (ItemDrop.ItemData item in player.GetInventory().GetAllItems())
                    if (CanMove(item) && !player.IsItemEquiped(item)) sourceNames.Add(item.m_shared.m_name);
                if (chests.Count > 0 && GroundStack.AddSourceNames(player, radius, sourceNames))
                    yield return new WaitForSeconds(0.35f);

                foreach (Container chest in chests)
                {
                    if (!StorageLocator.Eligible(chest, player, current, radius)) continue;
                    bool hasMatch = false;
                    foreach (ItemDrop.ItemData item in chest.GetInventory().GetAllItems())
                    {
                        if (!sourceNames.Contains(item.m_shared.m_name)) continue;
                        hasMatch = true;
                        break;
                    }
                    if (!hasMatch) continue;
                    if (chest == current)
                    {
                        InStackCall = true;
                        try { chest.GetInventory().StackAll(player.GetInventory()); }
                        finally { InStackCall = false; }
                    }
                    else
                    {
                        Pending.Add(chest);
                        chest.StackAll();
                        float responseDeadline = Time.realtimeSinceStartup + 10f;
                        while (Pending.Contains(chest) && Time.realtimeSinceStartup < responseDeadline)
                            yield return new WaitForSecondsRealtime(0.1f);
                        if (Pending.Remove(chest)) Expired.Add(chest);
                    }

                    if (StorageLocator.Eligible(chest, player, current, radius))
                        _moved += GroundStack.Store(chest, player, radius);
                }

                if (player != null)
                    player.Message(MessageHud.MessageType.Center,
                        _moved > 0 ? "$msg_stackall " + _moved : "$msg_stackall_none");
            }
            finally
            {
                Running = false;
                InStackCall = false;
            }
        }

        // Adds the exact count moved by one inventory stack call.
        internal static void AddMoved(int amount)
        {
            if (Running) _moved += Mathf.Max(0, amount);
        }

        // Keeps items that right-click equips or consumes, plus non-equipable ammunition.
        internal static bool CanMove(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null || item.m_shared.m_questItem || item.IsEquipable())
                return false;

            switch (item.m_shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.AmmoNonEquipable:
                case ItemDrop.ItemData.ItemType.Consumable:
                case ItemDrop.ItemData.ItemType.Hands:
                case ItemDrop.ItemData.ItemType.Attach_Atgeir:
                case ItemDrop.ItemData.ItemType.None:
                    return false;
                default:
                    return true;
            }
        }

        // Suppresses rejected or expired native responses from this pass.
        internal static bool AllowResponse(Container chest, bool granted)
        {
            if (Expired.Remove(chest)) return false;
            if (!Running || !Pending.Contains(chest)) return true;
            InStackCall = granted;
            return granted;
        }

        // Marks a native chest response as completed.
        internal static void CompleteResponse(Container chest)
        {
            InStackCall = false;
            Pending.Remove(chest);
        }

        // Clears transient state when the plugin unloads.
        internal static void Reset()
        {
            Running = false;
            InStackCall = false;
            Pending.Clear();
            Expired.Clear();
            _moved = 0;
        }
    }

    // Moves ground pickups directly to matching stacks in an accessible chest.
    internal static class GroundStack
    {
        // Collects nearby pickup types and requests ownership before the transfer pass.
        internal static bool AddSourceNames(Player player, float radius, HashSet<string> names)
        {
            bool found = false;
            foreach (ItemDrop drop in UnityEngine.Object.FindObjectsByType<ItemDrop>(FindObjectsSortMode.None))
            {
                if (drop != null && !drop.IsPiece() && SuperStack.CanMove(drop.m_itemData) &&
                    (drop.transform.position - player.transform.position).sqrMagnitude <= radius * radius)
                {
                    found = true;
                    names.Add(drop.m_itemData.m_shared.m_name);
                    ZNetView view = drop.GetComponent<ZNetView>();
                    if (view != null && view.IsValid() && !view.IsOwner()) drop.RequestOwn();
                }
            }
            return found;
        }

        // Scans ground pickups inside the configured player radius.
        internal static int Store(Container chest, Player player, float radius)
        {
            ZNetView chestView = StorageLocator.View(chest);
            if (chestView == null || !chestView.IsValid())
            {
                return 0;
            }

            int moved = 0;
            chestView.ClaimOwnership();
            foreach (ItemDrop drop in UnityEngine.Object.FindObjectsByType<ItemDrop>(FindObjectsSortMode.None))
            {
                if (drop == null || drop.IsPiece() || !SuperStack.CanMove(drop.m_itemData) ||
                    (drop.transform.position - player.transform.position).sqrMagnitude > radius * radius)
                {
                    continue;
                }

                moved += TryStore(drop, chest);
            }

            return moved;
        }

        // Transfers only the amount the chest can currently hold.
        private static int TryStore(ItemDrop drop, Container chest)
        {
            Inventory inventory = chest.GetInventory();
            ItemDrop.ItemData source = drop.m_itemData;
            if (!inventory.ContainsItemByName(source.m_shared.m_name))
            {
                return 0;
            }

            ZNetView dropView = drop.GetComponent<ZNetView>();
            if (dropView == null || !dropView.IsValid() || !drop.CanPickup())
            {
                drop.RequestOwn();
                return 0;
            }

            int amount = Mathf.Min(source.m_stack, Capacity(inventory, source));
            if (amount <= 0)
            {
                return 0;
            }

            ItemDrop.ItemData copy = source.Clone();
            copy.m_stack = amount;
            if (!inventory.AddItem(copy))
            {
                return 0;
            }

            if (amount == source.m_stack)
            {
                ZNetScene.instance.Destroy(drop.gameObject);
            }
            else
            {
                source.m_stack -= amount;
                ItemDrop.SaveToZDO(source, dropView.GetZDO());
            }

            return amount;
        }

        // Counts compatible stack space plus empty chest slots.
        private static int Capacity(Inventory inventory, ItemDrop.ItemData item)
        {
            int free = inventory.GetEmptySlots() * item.m_shared.m_maxStackSize;
            foreach (ItemDrop.ItemData existing in inventory.GetAllItems())
            {
                if (existing.m_shared.m_name == item.m_shared.m_name &&
                    existing.m_quality == item.m_quality && existing.m_worldLevel == item.m_worldLevel)
                {
                    free += Mathf.Max(0, item.m_shared.m_maxStackSize - existing.m_stack);
                }
            }

            return free;
        }
    }

    // Replaces the native Stack All click only while the Super shortcut is held.
    [HarmonyPatch(typeof(InventoryGui), "OnStackAll")]
    internal static class StackButtonPatch
    {
        // Starts Super Stack and leaves normal clicks to Valheim.
        private static bool Prefix()
        {
            if (!SuperActionInput.IsHeld())
            {
                return true;
            }

            if (!SuperStack.Running) Plugin.Instance.StartCoroutine(SuperStack.Run());
            return false;
        }
    }

    // Keeps vanilla Stack All for normal clicks, but filters Super Stack sources.
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.StackAll))]
    internal static class StackCountPatch
    {
        private static bool Prefix(Inventory __instance, Inventory fromInventory, ref int __result)
        {
            if (!SuperStack.InStackCall || Player.m_localPlayer == null ||
                fromInventory != Player.m_localPlayer.GetInventory()) return true;

            int before = __instance.CountItems(null);
            var items = new List<ItemDrop.ItemData>(fromInventory.GetAllItems());
            foreach (ItemDrop.ItemData item in items)
            {
                if (!SuperStack.CanMove(item) || !__instance.ContainsItemByName(item.m_shared.m_name) ||
                    Player.m_localPlayer.IsItemEquiped(item)) continue;
                if (__instance.AddItem(item)) fromInventory.RemoveItem(item);
            }

            __result = Mathf.Max(0, __instance.CountItems(null) - before);
            SuperStack.AddMoved(__result);
            Game.instance.IncrementPlayerStat(PlayerStatType.PlaceStacks);
            return false;
        }
    }

    // Suppresses per-chest access messages and tracks native responses.
    [HarmonyPatch(typeof(Container), "RPC_StackResponse")]
    internal static class StackResponsePatch
    {
        // Lets successful responses stack silently and skips rejected responses.
        private static bool Prefix(Container __instance, bool granted)
        {
            return SuperStack.AllowResponse(__instance, granted);
        }

        // Clears the temporary flag even if the native response throws.
        private static System.Exception Finalizer(Container __instance, System.Exception __exception)
        {
            SuperStack.CompleteResponse(__instance);
            return __exception;
        }
    }
}
