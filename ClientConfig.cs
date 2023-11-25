using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace NoOutsideItems
{
    public class ClientConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        public bool AllowOutsideItemsInSinglePlayer;


        public override void OnChanged()
        {
            if (Main.netMode == NetmodeID.SinglePlayer && Main.LocalPlayer.active)
            {
                var noiMod = (NoOutsideItems)this.Mod;
                var noiPlayer = Main.LocalPlayer.GetModPlayer<NoiPlayer>();

                foreach (var item in noiPlayer.GetAllActiveItems())
                {
                    if (AllowOutsideItemsInSinglePlayer && item.type == NoOutsideItems.OutsideItemType)
                    {
                        noiMod.ChangeBackToOriginalItem(item);
                    }
                    else if (!AllowOutsideItemsInSinglePlayer && item.type != NoOutsideItems.OutsideItemType)
                    {
                        var noiItem = item.GetGlobalItem<NoiGlobalItem>();
                        if (noiItem.WorldID != NoiSystem.WorldID)
                            noiMod.ChangeToOutsideItem(item);
                    }
                }
            }
        }
    }
}
