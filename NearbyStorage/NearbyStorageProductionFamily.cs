using System;
using System.Collections.Generic;
using UnityEngine;

namespace Landoria.NearbyStorage
{
    // Finds items produced by the same Valheim processing machine.
    internal static class NearbyStorageProductionFamily
    {
        private static readonly Dictionary<string, HashSet<string>> Machines =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        private static ZNetScene _scene;
        private static int _prefabCount = -1;

        // Groups outputs that share a machine and an item type.
        internal static bool SameMachine(ItemDrop.ItemData left, ItemDrop.ItemData right)
        {
            if (left?.m_shared == null || right?.m_shared == null ||
                left.m_shared.m_itemType != right.m_shared.m_itemType) { return false; }
            HashSet<string> leftMachines = MachinesFor(left);
            HashSet<string> rightMachines = MachinesFor(right);
            if (leftMachines == null || rightMachines == null) { return false; }
            foreach (string machine in leftMachines)
            {
                if (rightMachines.Contains(machine)) { return true; }
            }
            return false;
        }

        // Gets machine identifiers by the item's internal name.
        private static HashSet<string> MachinesFor(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null || ZNetScene.instance == null) { return null; }
            ZNetScene scene = ZNetScene.instance;
            if (_scene != scene || _prefabCount != scene.m_prefabs.Count) { Refresh(scene); }
            return Machines.TryGetValue(item.m_shared.m_name, out HashSet<string> machines) ?
                machines : null;
        }

        // Rebuilds the lookup when Valheim or a mod replaces the prefab list.
        private static void Refresh(ZNetScene scene)
        {
            Machines.Clear();
            foreach (GameObject prefab in scene.m_prefabs)
            {
                if (prefab == null) { continue; }
                Smelter smelter = prefab.GetComponent<Smelter>();
                if (smelter != null)
                {
                    foreach (Smelter.ItemConversion conversion in smelter.m_conversion)
                    { Add(prefab.name, conversion?.m_to); }
                }
                CookingStation cooking = prefab.GetComponent<CookingStation>();
                if (cooking != null)
                {
                    foreach (CookingStation.ItemConversion conversion in cooking.m_conversion)
                    { Add(prefab.name, conversion?.m_to); }
                }
                Fermenter fermenter = prefab.GetComponent<Fermenter>();
                if (fermenter != null)
                {
                    foreach (Fermenter.ItemConversion conversion in fermenter.m_conversion)
                    { Add(prefab.name, conversion?.m_to); }
                }
            }
            _scene = scene;
            _prefabCount = scene.m_prefabs.Count;
        }

        // Records a conversion output under its producing prefab.
        private static void Add(string machine, ItemDrop output)
        {
            string item = output?.m_itemData?.m_shared?.m_name;
            if (string.IsNullOrEmpty(item)) { return; }
            if (!Machines.TryGetValue(item, out HashSet<string> machines))
            {
                machines = new HashSet<string>(StringComparer.Ordinal);
                Machines.Add(item, machines);
            }
            machines.Add(machine);
        }
    }
}
