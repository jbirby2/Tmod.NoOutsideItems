using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NoOutsideItems
{
	public class NoOutsideItems : Mod
	{
        public static int OutsideItemType { get; private set; }

        public override void PostSetupContent()
        {
            OutsideItemType = ModContent.ItemType<OutsideItem>();
        }

        public void ChangeToOutsideItem(Item item)
        {
            if (!item.active || item.type == ItemID.None || item.type == OutsideItemType)
                throw new Exception("Cannot change this item into an OutsideItem: " + item.ToString());

            var noiItem = item.GetGlobalItem<NoiGlobalItem>();

            Logger.Debug("Changing item " + item.Name + " (" + item.netID + ") from " + (String.IsNullOrWhiteSpace(noiItem.WorldID) ? "unknown server" : noiItem.WorldName + " (" + noiItem.WorldID + ")"));

            var originalWorldID = noiItem.WorldID;
            var originalWorldName = noiItem.WorldName;
            var originalType = item.type;
            var originalStack = item.stack;
            var originalPrefix = item.prefix;
            var originalData = item.SerializeData();

            item.ChangeItemType(ModContent.ItemType<OutsideItem>());

            var outsideItem = (OutsideItem)item.ModItem;
            outsideItem.OriginalWorldID = originalWorldID;
            outsideItem.OriginalWorldName = originalWorldName;
            outsideItem.OriginalType = originalType;
            outsideItem.OriginalStack = originalStack;
            outsideItem.OriginalPrefix = originalPrefix;
            outsideItem.OriginalData = originalData;
        }

        public void ChangeBackToOriginalItem(Item item)
        {
            if (!item.active || item.type != OutsideItemType)
                throw new Exception("Cannot change this item from an OutsideItem back to its original type: " + item.ToString());

            var noiItem = item.GetGlobalItem<NoiGlobalItem>();

            Logger.Debug("Changing back item " + item.Name + " (" + item.netID + ") from " + (String.IsNullOrWhiteSpace(noiItem.WorldID) ? "unknown server" : noiItem.WorldName + " (" + noiItem.WorldID + ")"));

            var outsideItem = (OutsideItem)item.ModItem;

            var originalType = outsideItem.OriginalType;
            var originalStack = outsideItem.OriginalStack;
            var originalPrefix = outsideItem.OriginalPrefix;
            var originalData = outsideItem.OriginalData;

            item.ChangeItemType(originalType);
            item.stack = originalStack;
            item.Prefix(originalPrefix);
            ItemIO.Load(item, originalData);
        }
    }
}