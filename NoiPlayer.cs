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
        // TagCompound can't easily store Lists of Guids, so store them as strings instead
        public IList<string> ServersWherePlayerHasUsedImport = new List<string>();

        public override void OnEnterWorld()
        {
            foreach (var item in GetAllActiveItems())
            {
                var noiItem = item.GetGlobalItem<NoiGlobalItem>();

                // If there's no WorldID stored for this item, then it's impossible to ever know what world this item came from.
                if (noiItem.WorldID.Equals(Guid.Empty))
                {
                    noiItem.WorldID = NoOutsideItems.UnknownWorldID;
                    noiItem.WorldName = Language.GetTextValue("Unknown");
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
