using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.IO;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace NoOutsideItems
{
    /// <summary>
    /// Adds additional data properties to items of type BannedItem
    /// </summary>
    public class NoiBannedItem : GlobalItem
    {
        public string OriginalWorldID = "";
        public string OriginalWorldName = "";

        public override bool InstancePerEntity
        {
            get { return true; }
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            if (item.type == NoOutsideItems.BannedItemType)
            {
                tag["OriginalWorldID"] = OriginalWorldID;
                tag["OriginalWorldName"] = OriginalWorldName;
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            if (item.type == NoOutsideItems.BannedItemType)
            {
                OriginalWorldID = tag.Get<string>("OriginalWorldID");
                OriginalWorldName = tag.Get<string>("OriginalWorldName");
            }
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            if (item.type == NoOutsideItems.BannedItemType)
            {
                writer.Write(OriginalWorldID);
                writer.Write(OriginalWorldName);
            }
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            if (item.type == NoOutsideItems.BannedItemType)
            {
                OriginalWorldID = reader.ReadString();
                OriginalWorldName = reader.ReadString();
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.type == NoOutsideItems.BannedItemType)
            {
                var originalWorldNameLine = new TooltipLine(this.Mod, "OriginalWorldName", Language.GetTextValue("World") + ": " + OriginalWorldName);
                originalWorldNameLine.OverrideColor = new Color(150, 150, 150);
                tooltips.Add(originalWorldNameLine);

                if (ModContent.GetInstance<ClientConfig>().ShowWorldIDInItemTooltips)
                {
                    var originalWorldIDLine = new TooltipLine(this.Mod, "OriginalWorldID", "ID: " + OriginalWorldID);
                    originalWorldIDLine.OverrideColor = new Color(150, 150, 150);
                    tooltips.Add(originalWorldIDLine);
                }
            }
        }
    }
}
