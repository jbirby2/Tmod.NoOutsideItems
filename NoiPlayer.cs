using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

namespace NoOutsideItems
{
    public class NoiPlayer : ModPlayer
    {

        public IEnumerable<Item> GetAllActiveItems()
        {
            var allItems = new List<Item>();
            allItems.AddRange(this.Player.inventory.Where(item => item.active && item.type != ItemID.None));
            allItems.AddRange(this.Player.bank.item.Where(item => item.active && item.type != ItemID.None));
            allItems.AddRange(this.Player.bank2.item.Where(item => item.active && item.type != ItemID.None));
            allItems.AddRange(this.Player.bank3.item.Where(item => item.active && item.type != ItemID.None));
            allItems.AddRange(this.Player.bank4.item.Where(item => item.active && item.type != ItemID.None));
            allItems.AddRange(this.Player.armor.Where(item => item.active && item.type != ItemID.None));
            allItems.AddRange(this.Player.dye.Where(item => item.active && item.type != ItemID.None));
            allItems.AddRange(this.Player.miscEquips.Where(item => item.active && item.type != ItemID.None));
            allItems.AddRange(this.Player.miscDyes.Where(item => item.active && item.type != ItemID.None));
            if (this.Player.trashItem.active && this.Player.trashItem.type != ItemID.None)
                allItems.Add(this.Player.trashItem);
    
            return allItems;
        }

        public override void OnEnterWorld()
        {
            var noiMod = (NoOutsideItems)this.Mod;
            bool allowOutsideItemsInSinglePlayer = ModContent.GetInstance<ClientConfig>().AllowOutsideItemsInSinglePlayer;

            noiMod.Logger.Debug("******* ENTERING WORLD *******"); // joestub

            // Loop through every item in the player's inventory.
            // If the item isn't from this world, then change it to an OutsideItem.
            // If the item is from this world, but it was previously changed to an OutsideItem, then change it back.
            bool needsResync = false;
            foreach (var item in GetAllActiveItems())
            {
                needsResync = onEnterWorld_HandleItem(item, noiMod, allowOutsideItemsInSinglePlayer) || needsResync;
            }

            if (needsResync && Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, Main.myPlayer);
        }

        // returns true if the item was changed
        private bool onEnterWorld_HandleItem(Item item, NoOutsideItems noiMod, bool allowOutsideItemsInSinglePlayer)
        {
            noiMod.Logger.Debug("onEnterWorld_HandleItem for " +  item.ToString());

            var noiItem = item.GetGlobalItem<NoiGlobalItem>();

            if (item.type == NoOutsideItems.OutsideItemType && noiItem.WorldID == NoiSystem.WorldID)
            {
                noiMod.ChangeBackToOriginalItem(item);
                return true;
            }
            else if (item.type != NoOutsideItems.OutsideItemType)
            {
                bool changed = false;

                if (String.IsNullOrEmpty(noiItem.WorldID))
                {
                    noiItem.WorldID = "unknown";
                    noiItem.WorldName = Language.GetTextValue("Unknown");
                    changed = true;
                }

                if (noiItem.WorldID != NoiSystem.WorldID && (Main.netMode != NetmodeID.SinglePlayer || !allowOutsideItemsInSinglePlayer))
                {
                    noiMod.ChangeToOutsideItem(item);
                    changed = true;
                }

                return changed;
            }

            return false;
        }
    }
}
