using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Landoria.SuperStorage
{
    // Supplies one vanilla refuel interaction from an accessible nearby chest.
    internal static class RefuelContext
    {
        private static readonly System.Reflection.MethodInfo LoadContainer =
            AccessTools.Method(typeof(Container), "Load");
        private static Player _player;
        private static Container _chest;
        private static string _fuel;

        internal static void Begin(Humanoid user, IEnumerable<ItemDrop> fuels)
        {
            Reset();
            if (!SuperActionInput.IsHeld() || user == null || user != Player.m_localPlayer ||
                fuels == null) return;

            Player player = Player.m_localPlayer;
            Inventory inventory = player.GetInventory();
            var names = new List<string>();
            foreach (ItemDrop fuel in fuels)
            {
                if (fuel == null) continue;
                string name = fuel.m_itemData.m_shared.m_name;
                if (inventory.HaveItem(name)) return;
                names.Add(name);
            }

            foreach (Container chest in StorageLocator.Nearby(player))
            {
                foreach (string name in names)
                {
                    if (!chest.GetInventory().HaveItem(name) || !Claim(chest, player)) continue;
                    if (!chest.GetInventory().HaveItem(name)) continue;
                    _player = player;
                    _chest = chest;
                    _fuel = name;
                    return;
                }
            }
        }

        internal static void Begin(Humanoid user, ItemDrop fuel)
        {
            if (fuel == null) { Reset(); return; }
            Begin(user, new[] { fuel });
        }

        internal static bool Matches(Inventory inventory, string name)
        {
            return _player != null && inventory == _player.GetInventory() && name == _fuel;
        }

        internal static bool Withdraw()
        {
            if (_chest == null || _player == null ||
                !StorageLocator.Eligible(_chest, _player, StorageLocator.CurrentContainer(),
                    Plugin.Instance.Settings.Radius.Value)) return false;
            ZNetView view = StorageLocator.View(_chest);
            if (view == null || !view.IsOwner() || !_chest.GetInventory().HaveItem(_fuel)) return false;
            _chest.GetInventory().RemoveItem(_fuel, 1);
            return true;
        }

        internal static void Reset()
        {
            _player = null;
            _chest = null;
            _fuel = null;
        }

        private static bool Claim(Container chest, Player player)
        {
            try
            {
                if (!StorageLocator.Eligible(chest, player, StorageLocator.CurrentContainer(),
                    Plugin.Instance.Settings.Radius.Value)) return false;
                ZNetView view = StorageLocator.View(chest);
                view.ClaimOwnership();
                if (!view.IsOwner()) return false;
                if (chest != StorageLocator.CurrentContainer())
                    LoadContainer?.Invoke(chest, null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
