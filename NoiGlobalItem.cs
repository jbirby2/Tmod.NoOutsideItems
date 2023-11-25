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
    // joestub see WeaponWithGrowingDamage in ExampleMod

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
            if (Main.netMode != NetmodeID.Server && item.type != ModContent.ItemType<OutsideItem>())
            {
                if (!String.IsNullOrWhiteSpace(WorldID))
                    tag["WorldID"] = WorldID;

                if (!String.IsNullOrWhiteSpace(WorldName))
                    tag["WorldName"] = WorldName;
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            if (Main.netMode != NetmodeID.Server && item.type != ModContent.ItemType<OutsideItem>())
            {
                if (tag.ContainsKey("WorldID"))
                    WorldID = tag.Get<string>("WorldID");

                if (tag.ContainsKey("WorldName"))
                    WorldName = tag.Get<string>("WorldName");
            }
        }

        public void SetWorldIDToCurrentWorld()
        {
            WorldID = NoiSystem.WorldID;
            WorldName = Main.worldName ?? "";
        }
    }
}
