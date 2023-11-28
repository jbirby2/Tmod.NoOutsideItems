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
        public IList<string> ServersWherePlayerHasUsedImport = new List<string>();

        public override void PreSavePlayer()
        {
            if (Main.LocalPlayer.active)
            {
                var noiPlayer = Main.LocalPlayer.GetModPlayer<NoiPlayer>();

                foreach (var item in noiPlayer.GetAllActiveItems())
                {
                    if (item.type != NoOutsideItems.BannedItemType)
                    {
                        var noiItem = item.GetGlobalItem<NoiGlobalItem>();

                        if (String.IsNullOrWhiteSpace(noiItem.WorldID) && !String.IsNullOrWhiteSpace(NoiSystem.WorldID))
                        {
                            // This item must have been obtained during this play session, so set its WorldID and WorldName
                            noiItem.SetWorldIDToCurrentWorld(item);
                        }
                    }
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["ServersWherePlayerHasUsedImport"] = ServersWherePlayerHasUsedImport;
        }

        public override void LoadData(TagCompound tag)
        {
            ServersWherePlayerHasUsedImport = tag.GetList<string>("ServersWherePlayerHasUsedImport");
        }

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
    }
}
