using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
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
            // On the server side, we only bother saving the WorldID and WorldName if they differ from the current world
            if (item.type != ModContent.ItemType<OutsideItem>() && (Main.netMode != NetmodeID.Server || WorldID != NoiSystem.WorldID))
            {
                tag["WorldID"] = WorldID;
                tag["WorldName"] = WorldName;
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            if (item.type != ModContent.ItemType<OutsideItem>())
            {
                WorldID = tag.Get<string>("WorldID");
                WorldName = tag.Get<string>("WorldName");
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


        public void SetWorldIDToCurrentWorld()
        {
            WorldID = NoiSystem.WorldID;
            WorldName = Main.worldName ?? "";
        }
    }
}
