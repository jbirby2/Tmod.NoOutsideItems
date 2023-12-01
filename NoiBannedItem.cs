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
        public Guid OriginalWorldID = Guid.Empty;
        public string OriginalWorldName = "";

        public override bool InstancePerEntity
        {
            get { return true; }
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            if (item.type == NoOutsideItems.BannedItemType)
            {
                tag["OriginalWorldID"] = OriginalWorldID.ToByteArray();
                tag["OriginalWorldName"] = OriginalWorldName;
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            if (item.type == NoOutsideItems.BannedItemType)
            {
                OriginalWorldID = new Guid(tag.GetByteArray("OriginalWorldID"));
                OriginalWorldName = tag.Get<string>("OriginalWorldName");
            }
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            if (item.type == NoOutsideItems.BannedItemType)
            {
                writer.Write(OriginalWorldID.ToByteArray());

                // Save a little bandwidth by not sending the current world name
                if (OriginalWorldID.Equals(Main.ActiveWorldFileData.UniqueId))
                    writer.Write("");
                else
                    writer.Write(OriginalWorldName);
            }
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            if (item.type == NoOutsideItems.BannedItemType)
            {
                OriginalWorldID = new Guid(reader.ReadBytes(16));
                OriginalWorldName = reader.ReadString();

                // Save a little bandwidth by not sending the current world name
                if (OriginalWorldID.Equals(Main.ActiveWorldFileData.UniqueId))
                    OriginalWorldName = Main.worldName;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.type == NoOutsideItems.BannedItemType)
            {
                var clientConfig = ModContent.GetInstance<ClientConfig>();

                if (clientConfig.ShowWorldNameInItemTooltips || !OriginalWorldName.Equals(Main.ActiveWorldFileData.UniqueId))
                {
                    var originalWorldNameLine = new TooltipLine(this.Mod, "OriginalWorldName", Language.GetTextValue("World") + ": " + OriginalWorldName);
                    originalWorldNameLine.OverrideColor = new Color(150, 150, 150);
                    tooltips.Add(originalWorldNameLine);
                }
                
                if (clientConfig.ShowWorldIDInItemTooltips && !OriginalWorldID.Equals(NoOutsideItems.UnknownWorldID))
                {
                    var originalWorldIDLine = new TooltipLine(this.Mod, "OriginalWorldID", "ID: " + OriginalWorldID.ToString());
                    originalWorldIDLine.OverrideColor = new Color(150, 150, 150);
                    tooltips.Add(originalWorldIDLine);
                }
            }
        }
    }
}
