using System.Collections.Generic;

namespace Landoria.NearbyStorage
{
    // Supplies one vanilla refuel interaction from nearby storage or ground items.
    internal static class RefuelContext
    {
        private static Player _player;
        private static Container _chest;
        private static ItemDrop _drop;
        private static ItemDrop.ItemData _groundItem;
        private static string _fuel;

        // Starts a context for an eligible local interaction.
        internal static void Begin(Humanoid user, IEnumerable<ItemDrop> fuels)
        {
            Reset();
            if (!NearbyStorageActionInput.IsHeld() || user == null || user != Player.m_localPlayer ||
                fuels == null)
            {
                return;
            }

            Player player = Player.m_localPlayer;
            Inventory inventory = player.GetInventory();
            List<string> names = AvailableFuelNames(inventory, fuels);
            if (names == null)
            {
                return;
            }

            Withdrawal source = new StorageSupply(player).FindStored(inventorySource =>
            {
                foreach (string name in names)
                {
                    ItemDrop.ItemData item = inventorySource.GetItem(name);
                    if (StorageSupply.CanWithdrawOne(inventorySource, item)) { return item; }
                }
                return null;
            }, item => names.Contains(item.m_shared.m_name));
            if (source == null)
            {
                return;
            }

            _player = player;
            _chest = source.Chest;
            _drop = source.Drop;
            _groundItem = source.GroundItem;
            _fuel = source.Name;
        }

        // Lists allowed fuels while keeping carried fuel ahead of items in chests.
        private static List<string> AvailableFuelNames(Inventory inventory, IEnumerable<ItemDrop> fuels)
        {
            var names = new List<string>();
            foreach (ItemDrop fuel in fuels)
            {
                if (fuel == null)
                {
                    continue;
                }

                string name = fuel.m_itemData.m_shared.m_name;
                if (inventory.HaveItem(name))
                {
                    return null;
                }

                names.Add(name);
            }

            return names;
        }

        // Starts a context for an eligible local interaction.
        internal static void Begin(Humanoid user, ItemDrop fuel)
        {
            if (fuel == null) { Reset(); return; }
            Begin(user, new[] { fuel });
        }

        // Checks whether the active context matches this inventory action.
        internal static bool Matches(Inventory inventory, string name)
        {
            return _player != null && inventory == _player.GetInventory() && name == _fuel;
        }

        // Removes one unit from the selected chest or ground drop.
        internal static bool Withdraw()
        {
            if (_drop != null)
            {
                return GroundSource.RemoveOne(_drop, _player, _groundItem);
            }

            if (_chest == null || _player == null ||
                !StorageLocator.Eligible(_chest, _player, StorageLocator.CurrentContainer(),
                    Plugin.Instance.Settings.Radius.Value))
            {
                return false;
            }

            ZNetView view = StorageLocator.View(_chest);
            if (view == null || !view.IsOwner() || !StorageSupply.CanWithdrawOne(
                _chest.GetInventory(), _chest.GetInventory().GetItem(_fuel)))
            {
                return false;
            }

            _chest.GetInventory().RemoveItem(_fuel, 1);
            return true;
        }

        // Clears the active context.
        internal static void Reset()
        {
            _player = null;
            _chest = null;
            _drop = null;
            _groundItem = null;
            _fuel = null;
        }

    }
}
