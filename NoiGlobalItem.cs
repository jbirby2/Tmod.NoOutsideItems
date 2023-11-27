using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoOutsideItems
{
    public class NoiGlobalItem : GlobalItem
    {
        public string WorldID = "";
        public string WorldName = "";

        public override bool InstancePerEntity
        {
            get { return true; }
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            // On the server side, we only bother saving the WorldID and WorldName if they differ from the current world (just to avoid unnecessarily wasting disk space, memory, network bandwidth)
            if (item.type != NoOutsideItems.BannedItemType && (Main.netMode != NetmodeID.Server || WorldID != NoiSystem.WorldID))
            {
                tag["WorldID"] = WorldID;
                tag["WorldName"] = WorldName;
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            if (item.type != NoOutsideItems.BannedItemType)
            {
                WorldID = tag.Get<string>("WorldID");
                WorldName = tag.Get<string>("WorldName");

                // If we're running on the client and there's no WorldID stored for this item, then it's impossible to ever know what world this item came from.
                if (Main.netMode != NetmodeID.Server && String.IsNullOrWhiteSpace(WorldID))
                {
                    WorldID = "unknown";
                    WorldName = Language.GetTextValue("Unknown");
                }
            }
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            // Don't waste bandwidth sending the WorldID and WorldName if they're for the current world
            if (WorldID == NoiSystem.WorldID)
            {
                writer.Write(""); // WorldID
                writer.Write(""); // WorldName
            }
            else
            {
                writer.Write(WorldID);
                writer.Write(WorldName);
            }
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            WorldID = reader.ReadString();
            WorldName = reader.ReadString();
        }

        public override void OnCreated(Item item, ItemCreationContext context)
        {
            SetWorldIDToCurrentWorld(item);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            // Don't add these tooltips for BannedItemType because it handles its own tooltips in NoiBannedItem.cs; these tooltips are meant for all other item types.

            if (ModContent.GetInstance<ClientConfig>().ShowWorldNameInItemTooltips && item.type != NoOutsideItems.BannedItemType)
            {
                var worldNameLine = new TooltipLine(this.Mod, "WorldName", Language.GetTextValue("World") + ": " + (String.IsNullOrWhiteSpace(WorldID) ? Main.worldName : WorldName));
                worldNameLine.OverrideColor = new Color(150, 150, 150);
                tooltips.Add(worldNameLine);
            }

            if (ModContent.GetInstance<ClientConfig>().ShowWorldIDInItemTooltips && item.type != NoOutsideItems.BannedItemType)
            {
                var worldIDLine = new TooltipLine(this.Mod, "WorldID", "ID: " + (String.IsNullOrWhiteSpace(WorldID) ? NoiSystem.WorldID : WorldID));
                worldIDLine.OverrideColor = new Color(150, 150, 150);
                tooltips.Add(worldIDLine);
            }
        }

        public void SetWorldIDToCurrentWorld(Item item)
        {
            if (item.type != NoOutsideItems.BannedItemType)
            {
                WorldID = NoiSystem.WorldID;
                WorldName = Main.worldName ?? "";
            }
        }
    }
}
