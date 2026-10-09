using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Landoria.NearbyStorage
{
    // Shows confirmed deposits in separate fading rows beside the inventory.
    internal static partial class NearbyStorageDialog
    {
        private const float DepositNoticeHold = 4f;
        private const float DepositNoticeFade = 1.5f;
        private const int MaxDepositNotices = 3;
        private static readonly List<NearbyStorageDepositNotice> DepositNotices =
            new List<NearbyStorageDepositNotice>();
        private static readonly Queue<(Sprite ItemIcon, string Count, Sprite ChestIcon,
            string ChestName)> PendingDepositNotices =
            new Queue<(Sprite, string, Sprite, string)>();
        private static Vector2 _depositNoticeOrigin;

        // Keeps the first row beside the inventory and stacks later rows below it.
        private static void PositionDepositNotices(Vector3 inventoryTopRight)
        {
            if (_root == null) { return; }
            Vector3 corner = _root.InverseTransformPoint(inventoryTopRight);
            _depositNoticeOrigin = new Vector2(corner.x + 88f, corner.y);
            ArrangeDepositNotices();
        }

        // Places visible rows in arrival order below the first row.
        private static void ArrangeDepositNotices()
        {
            for (int i = 0; i < DepositNotices.Count; i++)
            {
                DepositNotices[i].Root.anchoredPosition =
                    _depositNoticeOrigin + new Vector2(0f, -i * 76f);
            }
        }

        // Queues a snapshot of each confirmed deposit in arrival order.
        internal static void ShowDeposit(ItemDrop.ItemData item, int amount, Container chest)
        {
            if (_root == null || item == null || chest == null) { return; }
            Piece piece = chest.GetComponent<Piece>() ?? chest.GetComponentInParent<Piece>();
            Sprite chestIcon = piece?.m_icon ?? ZNetScene.instance?.
                GetPrefab("piece_chest_wood")?.GetComponent<Piece>()?.m_icon;
            PendingDepositNotices.Enqueue((item.GetIcon(),
                amount > 1 ? amount.ToString("N0") : "", chestIcon,
                StorageLabel.ShortLabel(chest)));
            FillDepositNotices();
        }

        // Displays queued deposits when one of the three visible slots is free.
        private static void FillDepositNotices()
        {
            while (_root != null && DepositNotices.Count < MaxDepositNotices &&
                PendingDepositNotices.Count > 0)
            {
                var pending = PendingDepositNotices.Dequeue();
                NearbyStorageDepositNotice notice = CreateDepositNotice();
                notice.ItemIcon.sprite = pending.ItemIcon;
                notice.Count.text = pending.Count;
                notice.ChestIcon.sprite = pending.ChestIcon;
                notice.ChestName.text = pending.ChestName;
                notice.Until = Time.unscaledTime + DepositNoticeHold + DepositNoticeFade;
                DepositNotices.Add(notice);
            }
            ArrangeDepositNotices();
        }

        // Creates one transparent row with the item and destination visuals.
        private static NearbyStorageDepositNotice CreateDepositNotice()
        {
            var notice = new NearbyStorageDepositNotice();
            notice.Root = Rect("Deposit notice", _root, new Vector2(460f, 72f));
            notice.Root.anchorMin = notice.Root.anchorMax = new Vector2(0f, 1f);
            notice.Root.pivot = new Vector2(0f, 1f);
            notice.Group = notice.Root.gameObject.AddComponent<CanvasGroup>();
            notice.Group.blocksRaycasts = false;
            CreateDepositItem(notice);
            CreateDepositDestination(notice);
            return notice;
        }

        // Draws a 64-unit item icon and its deposited quantity.
        private static void CreateDepositItem(NearbyStorageDepositNotice notice)
        {
            RectTransform slot = Rect("Deposited item", notice.Root, new Vector2(70f, 70f));
            slot.anchoredPosition = new Vector2(3f, -3f);
            RectTransform icon = Rect("Item icon", slot, new Vector2(64f, 64f));
            notice.ItemIcon = icon.gameObject.AddComponent<Image>();
            StyleIcon(notice.ItemIcon);
            icon.anchorMin = icon.anchorMax = new Vector2(0f, 1f);
            icon.pivot = new Vector2(0f, 1f);
            icon.anchoredPosition = new Vector2(3f, -3f);
            icon.sizeDelta = new Vector2(64f, 64f);
            notice.ItemIcon.preserveAspect = true;
            notice.ItemIcon.raycastTarget = false;
            notice.Count = Label("Quantity", slot, "", 15f,
                new Vector2(2f, -51f), new Vector2(66f, 16f));
            StyleCount(notice.Count);
            notice.Count.alignment = TextAlignmentOptions.BottomRight;
        }

        // Adds the arrow, 64-unit container icon, and container name.
        private static void CreateDepositDestination(NearbyStorageDepositNotice notice)
        {
            TextMeshProUGUI arrow = Label("Arrow", notice.Root, "→", 28f,
                new Vector2(72f, -17f), new Vector2(26f, 38f));
            arrow.alignment = TextAlignmentOptions.Center;
            RectTransform chestIcon = Rect("Container icon", notice.Root,
                new Vector2(64f, 64f));
            chestIcon.anchoredPosition = new Vector2(102f, -4f);
            notice.ChestIcon = chestIcon.gameObject.AddComponent<Image>();
            notice.ChestIcon.preserveAspect = true;
            notice.ChestIcon.raycastTarget = false;
            notice.ChestName = Label("Container name", notice.Root, "", 18f,
                new Vector2(176f, -21f), new Vector2(250f, 30f));
            notice.ChestName.alignment = TextAlignmentOptions.MidlineLeft;
            notice.ChestName.overflowMode = TextOverflowModes.Ellipsis;
        }

        // Holds each row briefly, then fades it independently.
        private static void UpdateDepositNotices()
        {
            for (int i = DepositNotices.Count - 1; i >= 0; i--)
            {
                NearbyStorageDepositNotice notice = DepositNotices[i];
                notice.Group.alpha = Mathf.Clamp01(
                    (notice.Until - Time.unscaledTime) / DepositNoticeFade);
                if (notice.Group.alpha > 0f) { continue; }
                UnityEngine.Object.Destroy(notice.Root.gameObject);
                DepositNotices.RemoveAt(i);
            }
            FillDepositNotices();
        }
    }
}
