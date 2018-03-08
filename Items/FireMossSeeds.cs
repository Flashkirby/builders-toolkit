using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class FireMossSeeds : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fire Moss Seeds");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.GrassSeeds);
            item.createTile = TileID.LavaMoss;
            item.value *= 5;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.alchemy = true;
            r.needLava = true;
            r.AddIngredient(ItemID.BlinkrootSeeds);
            r.SetResult(item.type, 5);
            r.AddRecipe();
        }

        public override bool CanUseItem(Player player)
        { return ValidTarget(Player.tileTargetX, Player.tileTargetY); }
        public override bool ConsumeItem(Player player)
        { return ValidTarget(Player.tileTargetX, Player.tileTargetY); }
        
        internal static bool ValidTarget(int x, int y)
        {
            return Main.tile[x, y].nactive() &&
                  TileInRange(x, y) &&
                  Main.tile[x, y].type == TileID.Stone;
        }

        internal static bool TileInRange(int x, int y)
        {
            return Main.LocalPlayer.position.X / 16f - (float)Player.tileRangeX - (float)Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].tileBoost <= (float)Player.tileTargetX && (Main.LocalPlayer.TopRight.X) / 16f + (float)Player.tileRangeX + (float)Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].tileBoost - 1f >= (float)Player.tileTargetX && Main.LocalPlayer.position.Y / 16f - (float)Player.tileRangeY - (float)Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].tileBoost <= (float)Player.tileTargetY && (Main.LocalPlayer.BottomLeft.Y) / 16f + (float)Player.tileRangeY + (float)Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].tileBoost - 2f >= (float)Player.tileTargetY;
        }
    }
}
