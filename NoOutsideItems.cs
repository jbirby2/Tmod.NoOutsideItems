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
        private static Func<Item, object> itemPreBanCallback;
        private static Action<Item, object> itemPostBanCallback;

        public override void PostSetupContent()
        {
            itemBanMod = ModLoader.GetMod("ItemBan");
            BannedItemType = (int)itemBanMod.Call("GetBannedItemType");

            decideBanCallback = (Func<Item, bool>)onDecideBan;
            itemBanMod.Call("OnDecideBan", decideBanCallback, this);

            inventorySlotChangedCallback = (Action<Item>)onInventorySlotChanged;
            itemBanMod.Call("OnInventorySlotChanged", inventorySlotChangedCallback);

            itemPreBanCallback = (Func<Item, object>)onItemPreBan;
            itemPostBanCallback = (Action<Item, object>)onItemPostBan;
            itemBanMod.Call("OnItemBan", itemPreBanCallback, itemPostBanCallback);
        }

        public override void Unload()
        {
            itemBanMod.Call("OffDecideBan", decideBanCallback, this);
            itemBanMod.Call("OffInventorySlotChanged", inventorySlotChangedCallback);
            itemBanMod.Call("OffItemBan", itemPreBanCallback, itemPostBanCallback);
        }

        public void UpdatePlayerBans()
        {
            if (itemBanMod != null)
                itemBanMod.Call("UpdatePlayerBans");
        }

        public void UpdateWorldBans()
        {
            if (itemBanMod != null)
                itemBanMod.Call("UpdateWorldBans");
        }


        // private

        private bool onDecideBan(Item item)
        {
            var noiItem = item.GetGlobalItem<NoiGlobalItem>();

            if (Main.netMode == NetmodeID.SinglePlayer && ModContent.GetInstance<ClientConfig>().AllowOutsideItemsInSinglePlayer)
            {
                return false;
            }
            else
            {
                return !noiItem.WorldID.Equals(Main.ActiveWorldFileData.UniqueId);
            }
        }

        private void onInventorySlotChanged(Item item)
        {
            if (item.active && item.type != BannedItemType && item.type != ItemID.None)
            {
                var noiItem = item.GetGlobalItem<NoiGlobalItem>();

                if (noiItem.WorldID.Equals(Guid.Empty))
                    noiItem.SetWorldIDToCurrentWorld(item);
            }
        }

        private object onItemPreBan(Item item)
        {
            var noiItem = item.GetGlobalItem<NoiGlobalItem>();

            // So far, the only situation where I've seen this happen is when you drag a banned item from Cheat Sheet directly into the world, without it entering an inventory.
            // When that happens, the WorldID gets set in OnSpawn like normal, but for some reason it doesn't get retained, and will be back to its default value of Guid.Empty again at this point.
            if (noiItem.WorldID.Equals(Guid.Empty))
                noiItem.SetWorldIDToCurrentWorld(item);

            Logger.Debug("joestub NoOutsideItems.onItemPreBan(): " + item.ToString() + " " + noiItem.WorldID.ToString() + " " + noiItem.WorldName);

            return new object[] { noiItem.WorldID, noiItem.WorldName };
        }

        private void onItemPostBan(Item item, object preBanState)
        {
            var noiBannedItem = item.GetGlobalItem<NoiBannedItem>();
            
            var stateItems = (object[])preBanState;
            Logger.Debug("joestub NoOutsideItems.onItemPostBan(): " + item.ToString() + " " + stateItems[0].ToString() + " " + stateItems[1].ToString());
            noiBannedItem.OriginalWorldID = (Guid)stateItems[0];
            noiBannedItem.OriginalWorldName = (string)stateItems[1];
        }

    }
}