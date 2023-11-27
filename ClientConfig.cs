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

        [DefaultValue(false)]
        public bool ShowWorldNameInItemTooltips;

        [DefaultValue(false)]
        public bool ShowWorldIDInItemTooltips;


        public override void OnChanged()
        {
            if (Main.LocalPlayer.active)
                ((NoOutsideItems)this.Mod).DecideBans();
        }
    }
}
