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
        public static readonly Guid UnknownWorldID = Guid.Parse("00000000-0000-0000-0000-000000000001");

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
                return !noiItem.WorldID.Equals(Main.ActiveWorldFileData.UniqueId);
            }
            else
            {
                bool importUnknownItemsOnFirstLogin = ModContent.GetInstance<ServerConfig>().ImportUnknownItemsOnFirstLogin;
                var noiPlayer = Main.LocalPlayer.GetModPlayer<NoiPlayer>();

                if (noiItem.WorldID.Equals(NoOutsideItems.UnknownWorldID) && importUnknownItemsOnFirstLogin && !noiPlayer.ServersWherePlayerHasUsedImport.Contains(Main.ActiveWorldFileData.UniqueId.ToString()))
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
                    return !noiItem.WorldID.Equals(Main.ActiveWorldFileData.UniqueId);
                }
            }
        }

        private void onInventorySlotChanged(Item item)
        {
            if (item.active && item.type != BannedItemType && item.type != ItemID.None)
            {
                var noiItem = item.GetGlobalItem<NoiGlobalItem>();

                if (noiItem.WorldID.Equals(Guid.Empty))
                    noiItem.SetWorldIDToCurrentWorld(item);
                else if (!noiItem.WorldID.Equals(Main.ActiveWorldFileData.UniqueId))
                    DecideBansOnClient();
            }
        }

        private void onItemBanned(Item item, Item cloneOfOriginalItem)
        {
            var noiBannedItem = item.GetGlobalItem<NoiBannedItem>();
            var noiOriginalItem = cloneOfOriginalItem.GetGlobalItem<NoiGlobalItem>();

            noiBannedItem.OriginalWorldID = noiOriginalItem.WorldID;
            noiBannedItem.OriginalWorldName = noiOriginalItem.WorldName;
        }

        private void onClientBansComplete()
        {
            if (consumePlayerImportOnClientBansComplete)
            {
                Logger.Debug("Player " + Main.LocalPlayer.name + " has used their one-time automatic import of items from unknown worlds (WorldID " + Main.ActiveWorldFileData.UniqueId.ToString() + ")");

                var noiPlayer = Main.LocalPlayer.GetModPlayer<NoiPlayer>();
                if (!noiPlayer.ServersWherePlayerHasUsedImport.Contains(Main.ActiveWorldFileData.UniqueId.ToString()))
                    noiPlayer.ServersWherePlayerHasUsedImport.Add(Main.ActiveWorldFileData.UniqueId.ToString());
    
                consumePlayerImportOnClientBansComplete = false;
            }
        }

        private void onServerBansComplete()
        {
        }
    }
}