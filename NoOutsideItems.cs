using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoOutsideItems
{
	public class NoOutsideItems : Mod
	{
        public static int BannedItemType { get; private set; }

        private static Mod itemBanMod = null;
        private static Func<Item, bool> decideBanCallback;
        private static Action<Item> inventorySlotChangedCallback;
        private static Action<Item, Item> itemBannedCallback;
        private static Action bansCompleteCallback;
        private static bool consumePlayerImportOnBansComplete = false;

        public override void PostSetupContent()
        {
            itemBanMod = ModLoader.GetMod("ItemBan");
            BannedItemType = (int)itemBanMod.Call("GetBannedItemType");

            decideBanCallback = (Func<Item, bool>)onDecideBan;
            itemBanMod.Call("OnDecideBan", decideBanCallback);

            inventorySlotChangedCallback = (Action<Item>)onInventorySlotChanged;
            itemBanMod.Call("OnInventorySlotChanged", inventorySlotChangedCallback);

            itemBannedCallback = (Action<Item, Item>)onItemBanned;
            itemBanMod.Call("OnItemBanned", itemBannedCallback);

            bansCompleteCallback = (Action)onBansComplete;
            itemBanMod.Call("OnBansComplete", bansCompleteCallback);
        }

        public override void Unload()
        {
            itemBanMod.Call("OffDecideBan", decideBanCallback);
            itemBanMod.Call("OffInventorySlotChanged", inventorySlotChangedCallback);
            itemBanMod.Call("OffItemBanned", itemBannedCallback);
            itemBanMod.Call("OffBansComplete", bansCompleteCallback);
        }

        public void DecideBans()
        {
            itemBanMod.Call("DecideBans");
        }

        // private

        private bool onDecideBan(Item item)
        {
            bool importUnknownItemsOnFirstLogin = ModContent.GetInstance<ServerConfig>().ImportUnknownItemsOnFirstLogin;
            var noiItem = item.GetGlobalItem<NoiGlobalItem>();
            var noiPlayer = Main.LocalPlayer.GetModPlayer<NoiPlayer>();

            if (noiItem.WorldID == "unknown" && importUnknownItemsOnFirstLogin && !noiPlayer.ServersWherePlayerHasUsedImport.Contains(NoiSystem.WorldID))
            {
                noiItem.SetWorldIDToCurrentWorld(item);
                consumePlayerImportOnBansComplete = true;
                return false;
            }
            else if (Main.netMode == NetmodeID.SinglePlayer && ModContent.GetInstance<ClientConfig>().AllowOutsideItemsInSinglePlayer)
            {
                return false;
            }
            else
            {
                return (noiItem.WorldID != NoiSystem.WorldID);
            }
        }

        private void onInventorySlotChanged(Item item)
        {
            if (item.active && item.type != BannedItemType && item.type != ItemID.None)
            {
                var noiItem = item.GetGlobalItem<NoiGlobalItem>();

                if (String.IsNullOrWhiteSpace(noiItem.WorldID))
                    noiItem.SetWorldIDToCurrentWorld(item);
                else if (noiItem.WorldID != NoiSystem.WorldID)
                    DecideBans();
            }
        }

        private void onItemBanned(Item item, Item cloneOfOriginalItem)
        {
            var noiBannedItem = item.GetGlobalItem<NoiBannedItem>();
            var noiOriginalItem = cloneOfOriginalItem.GetGlobalItem<NoiGlobalItem>();

            noiBannedItem.OriginalWorldID = noiOriginalItem.WorldID;
            noiBannedItem.OriginalWorldName = noiOriginalItem.WorldName;
        }

        private void onBansComplete()
        {
            if (consumePlayerImportOnBansComplete)
            {
                var noiPlayer = Main.LocalPlayer.GetModPlayer<NoiPlayer>();
                if (!noiPlayer.ServersWherePlayerHasUsedImport.Contains(NoiSystem.WorldID))
                    noiPlayer.ServersWherePlayerHasUsedImport.Add(NoiSystem.WorldID);
    
                consumePlayerImportOnBansComplete = false;
            }
        }
    }
}