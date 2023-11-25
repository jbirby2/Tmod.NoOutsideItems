using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoOutsideItems
{
    internal class OutsideItem : ModItem
    {
        public string OriginalWorldID = "";
        public string OriginalWorldName = "";
        public int OriginalType = 0;
        public int OriginalStack = 0;
        public int OriginalPrefix = 0;
        public TagCompound OriginalData = new TagCompound();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 0;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 1;
            Item.value = 0;
        }

        public override bool CanReforge()
        {
            return false;
        }

        public override bool CanResearch()
        {
            return false;
        }

        public override bool CanStack(Item source)
        {
            return false;
        }

        public override bool CanStackInWorld(Item source)
        {
            return false;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["OriginalWorldID"] = OriginalWorldID;
            tag["OriginalWorldName"] = OriginalWorldName;
            tag["OriginalType"] = OriginalType;
            tag["OriginalStack"] = OriginalStack;
            tag["OriginalPrefix"] = OriginalPrefix;
            tag["OriginalData"] = OriginalData;
        }

        public override void LoadData(TagCompound tag)
        {
            OriginalWorldID = tag.GetString("OriginalWorldID");
            OriginalWorldName = tag.GetString("OriginalWorldName");
            OriginalType = tag.GetInt("OriginalType");
            OriginalStack = tag.GetInt("OriginalStack");
            OriginalPrefix = tag.GetInt("OriginalPrefix");
            OriginalData = tag.GetCompound("OriginalData");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(OriginalWorldID);
            writer.Write(OriginalWorldName);
            writer.Write(OriginalType);
            writer.Write(OriginalStack);
            writer.Write(OriginalPrefix);
            TagIO.Write(OriginalData, writer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            OriginalWorldID = reader.ReadString();
            OriginalWorldName = reader.ReadString();
            OriginalType = reader.ReadInt32();
            OriginalStack = reader.ReadInt32();
            OriginalPrefix = reader.ReadInt32();
            OriginalData = TagIO.Read(reader);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(this.Mod, "OriginalItem", Lang.GetItemNameValue(OriginalType) + (OriginalStack < 2 ? "" : " [" + OriginalStack.ToString() + "]")));

            if (!String.IsNullOrWhiteSpace(OriginalWorldID))
            {
                var worldLine = new TooltipLine(this.Mod, "OriginalWorld", Language.GetTextValue("World") + ": " + OriginalWorldName);
                worldLine.OverrideColor = new Color(150, 150, 150);
                tooltips.Add(worldLine);
            }
        }
    }
}
