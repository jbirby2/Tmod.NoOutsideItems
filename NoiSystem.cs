﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoOutsideItems
{
    public class NoiSystem : ModSystem
    {
        public static string WorldID = null;

        public override void ClearWorld()
        {
            WorldID = null;
        }

        public override void OnWorldLoad()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                WorldID = Guid.NewGuid().ToString();
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("WorldID"))
                WorldID = tag.GetString("WorldID");
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (!String.IsNullOrEmpty(WorldID))
                tag["WorldID"] = WorldID;
        }

        public override void NetReceive(BinaryReader reader)
        {
            WorldID = reader.ReadString();
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(WorldID);
        }

        public override void PreSaveAndQuit()
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
}
