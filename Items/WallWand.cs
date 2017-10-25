using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class WallWand : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Architect: Walls");
            Tooltip.SetDefault("Places Scaffolding\n<right> to remove housing\n");
        }
        public override void SetDefaults()
        {
            TileWand.WandDefaults(item);
            item.useAnimation = 2;

            item.tileBoost = 12;
            item.tileWand = mod.ItemType<ScaffoldWall>();
            item.createWall = mod.GetItem<ScaffoldWall>().item.createWall;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(mod.ItemType<TileWand>());
            r.SetResult(item.type);
            r.AddRecipe();
        }

        public override Vector2? HoldoutOffset() { return new Vector2(2, 0); }
        public override void UpdateInventory(Player player)
        {
            player.rulerGrid = true;
        }
        public override bool AltFunctionUse(Player player)
        {
            item.hammer = 0; // Don't hammer
            item.createWall = -1;
            item.tileWand = -1;

            if (player.whoAmI == Main.myPlayer)
            {
                try
                {
                    if (Main.wallHouse[Main.tile[Player.tileTargetX, Player.tileTargetY].wall])
                    {
                        item.hammer = 67; // Exactly enough to one-shot walls
                        player.poundRelease = false; // Prevent hammering tiles
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse <= 0 && item.createTile == -1)
            {
                item.createWall = mod.GetItem<ScaffoldWall>().item.createWall;
                item.tileWand = mod.ItemType<ScaffoldWall>();
                item.hammer = 0;
            }
            return true;
        }
    }
}
