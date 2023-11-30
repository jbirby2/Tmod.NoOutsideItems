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
        public Guid WorldID = Guid.Empty;
        public string WorldName = "";

        public override bool InstancePerEntity
        {
            get { return true; }
        }

        public override bool CanStack(Item destination, Item source)
        {
            if (destination.active && source.active && !destination.GetGlobalItem<NoiGlobalItem>().WorldID.Equals(source.GetGlobalItem<NoiGlobalItem>().WorldID))
                return false;
            else
                return base.CanStack(destination, source);
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            if (item.type != NoOutsideItems.BannedItemType)
            {
                tag["WorldID"] = WorldID.ToByteArray();
                tag["WorldName"] = WorldName;
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            if (item.type != NoOutsideItems.BannedItemType)
            {
                if (tag.ContainsKey("WorldID"))
                    WorldID = new Guid(tag.GetByteArray("WorldID"));

                if (tag.ContainsKey("WorldName"))
                    WorldName = tag.Get<string>("WorldName");
            }
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            if (item.type != NoOutsideItems.BannedItemType)
            {
                writer.Write(WorldID.ToByteArray());

                // Save a little bandwidth by not sending the current world name
                if (WorldID.Equals(Main.ActiveWorldFileData.UniqueId))
                    writer.Write("");
                else
                    writer.Write(WorldName);
            }
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            if (item.type != NoOutsideItems.BannedItemType)
            {
                WorldID = new Guid(reader.ReadBytes(16));
                WorldName = reader.ReadString();

                // Save a little bandwidth by not sending the current world name
                if (WorldID.Equals(Main.ActiveWorldFileData.UniqueId))
                    WorldName = Main.worldName;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.type != NoOutsideItems.BannedItemType)
            {
                if (ModContent.GetInstance<ClientConfig>().ShowWorldNameInItemTooltips)
                {
                    var worldNameLine = new TooltipLine(this.Mod, "WorldName", Language.GetTextValue("World") + ": " + WorldName);
                    worldNameLine.OverrideColor = new Color(150, 150, 150);
                    tooltips.Add(worldNameLine);
                }

                if (ModContent.GetInstance<ClientConfig>().ShowWorldIDInItemTooltips && !WorldID.Equals(NoOutsideItems.UnknownWorldID))
                {
                    var worldIDLine = new TooltipLine(this.Mod, "WorldID", "ID: " + WorldID.ToString());
                    worldIDLine.OverrideColor = new Color(150, 150, 150);
                    tooltips.Add(worldIDLine);
                }
            }
        }

        public override void OnCreated(Item item, ItemCreationContext context)
        {
            if (item.type != NoOutsideItems.BannedItemType)
            {
                if (WorldID.Equals(Guid.Empty))
                    SetWorldIDToCurrentWorld(item);
            }
        }

        public override void OnSpawn(Item item, IEntitySource source)
        {
            if (item.type != NoOutsideItems.BannedItemType)
            {
                if (WorldID.Equals(Guid.Empty))
                    SetWorldIDToCurrentWorld(item);
            }
        }

        public void SetWorldIDToCurrentWorld(Item item)
        {
            if (item.type != NoOutsideItems.BannedItemType)
            {
                WorldID = Main.ActiveWorldFileData.UniqueId;
                WorldName = Main.worldName ?? "";
            }
        }
    }
}
