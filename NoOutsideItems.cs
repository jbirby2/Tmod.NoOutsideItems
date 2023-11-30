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
        private static Action clientBansCompleteCallback;
        private static Action serverBansCompleteCallback;
        private static bool consumePlayerImportOnClientBansComplete = false;

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

            clientBansCompleteCallback = (Action)onClientBansComplete;
            itemBanMod.Call("OnClientBansComplete", clientBansCompleteCallback);

            serverBansCompleteCallback = (Action)onServerBansComplete;
            itemBanMod.Call("OnServerBansComplete", serverBansCompleteCallback);
        }

        public override void Unload()
        {
            itemBanMod.Call("OffDecideBan", decideBanCallback);
            itemBanMod.Call("OffInventorySlotChanged", inventorySlotChangedCallback);
            itemBanMod.Call("OffItemBanned", itemBannedCallback);
            itemBanMod.Call("OffClientBansComplete", clientBansCompleteCallback);
            itemBanMod.Call("OffServerBansComplete", serverBansCompleteCallback);
        }

        public void DecideBansOnClient()
        {
            if (itemBanMod != null)
                itemBanMod.Call("DecideBansOnClient");
        }

        public void DecideBansOnServer()
        {
            if (itemBanMod != null)
                itemBanMod.Call("DecideBansOnServer");
        }


        // private

        private bool onDecideBan(Item item)
        {
            var noiItem = item.GetGlobalItem<NoiGlobalItem>();

            if (Main.netMode == NetmodeID.Server)
            {
                return (!String.IsNullOrWhiteSpace(noiItem.WorldID) && noiItem.WorldID != NoiSystem.WorldID);
            }
            else
            {
                bool importUnknownItemsOnFirstLogin = ModContent.GetInstance<ServerConfig>().ImportUnknownItemsOnFirstLogin;
                var noiPlayer = Main.LocalPlayer.GetModPlayer<NoiPlayer>();

                if (noiItem.WorldID == "unknown" && importUnknownItemsOnFirstLogin && !noiPlayer.ServersWherePlayerHasUsedImport.Contains(NoiSystem.WorldID))
                {
                    noiItem.SetWorldIDToCurrentWorld(item);
                    consumePlayerImportOnClientBansComplete = true;
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
        }

        private void onInventorySlotChanged(Item item)
        {
            if (item.active && item.type != BannedItemType && item.type != ItemID.None)
            {
                var noiItem = item.GetGlobalItem<NoiGlobalItem>();

                if (String.IsNullOrWhiteSpace(noiItem.WorldID))
                    noiItem.SetWorldIDToCurrentWorld(item);
                else if (noiItem.WorldID != NoiSystem.WorldID)
                    DecideBansOnClient();
            }
        }

        private void onItemBanned(Item item, Item cloneOfOriginalItem)
        {
            var noiBannedItem = item.GetGlobalItem<NoiBannedItem>();
            var noiOriginalItem = cloneOfOriginalItem.GetGlobalItem<NoiGlobalItem>();

            if (Main.netMode == NetmodeID.Server && String.IsNullOrWhiteSpace(noiOriginalItem.WorldID))
            {
                // To avoid wasting resources, the server normally doesn't save its own WorldID and WorldName on server-side items.
                noiBannedItem.OriginalWorldID = NoiSystem.WorldID;
                noiBannedItem.OriginalWorldName = Main.worldName ?? "";
            }
            else
            {
                noiBannedItem.OriginalWorldID = noiOriginalItem.WorldID;
                noiBannedItem.OriginalWorldName = noiOriginalItem.WorldName;
            }
        }

        private void onClientBansComplete()
        {
            if (consumePlayerImportOnClientBansComplete)
            {
                var noiPlayer = Main.LocalPlayer.GetModPlayer<NoiPlayer>();
                if (!noiPlayer.ServersWherePlayerHasUsedImport.Contains(NoiSystem.WorldID))
                    noiPlayer.ServersWherePlayerHasUsedImport.Add(NoiSystem.WorldID);
    
                consumePlayerImportOnClientBansComplete = false;
            }
        }

        private void onServerBansComplete()
        {
        }
    }
}