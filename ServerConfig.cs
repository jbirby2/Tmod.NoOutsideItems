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
    public class ServerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(false)]
        public bool ImportUnknownItemsOnFirstLogin;


        public override void OnChanged()
        {
            var mod = ((NoOutsideItems)this.Mod);

            if (Main.netMode != NetmodeID.Server && Main.LocalPlayer.active)
                mod.DecideBansOnClient();

            if (Main.netMode != NetmodeID.MultiplayerClient)
                mod.DecideBansOnServer();
        }
    }
}
