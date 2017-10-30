using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class WateringCan : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Speeds up tree growth");
        }

        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            //item.maxStack = 99;
            item.rare = 1;
            item.tileBoost = 24;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = 1;
            item.UseSound = SoundID.Item81;
            //item.consumable = true;
        }

        public override bool CanUseItem(Player player)
        {
            return TileLoader.IsSapling(Main.tile[Player.tileTargetX, Player.tileTargetY].type) || Main.tile[Player.tileTargetX, Player.tileTargetY].type == mod.TileType("MegaAcorn");
        }

        public override bool AltFunctionUse(Player player) { return true; }
        // Note that this item does not work in Multiplayer, but serves as a learning tool for other things.
        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse > 0)
            {
                CustomLivingTree.GrowLivingTree(Player.tileTargetX, Player.tileTargetY);
            }
            else if (WorldGen.GrowTree(Player.tileTargetX, Player.tileTargetY))
            {
                WorldGen.TreeGrowFXCheck(Player.tileTargetX, Player.tileTargetY);
            }
            else
            {
                //item.stack++;
            }
            return true;
        }


    }
}