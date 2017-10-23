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
            Scaffold.SetDefaultsBasic(item);
            item.consumable = false;
            item.maxStack = 1;
            item.useAnimation = 2;
            item.useTime = 1;
            item.useStyle = 5;

            item.tileBoost = 12;
            item.tileWand = mod.ItemType<ScaffoldWall>();
            item.createWall = mod.GetItem<ScaffoldWall>().item.createWall;

            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 2;
            item.autoReuse = true;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(mod.ItemType<TileWand>());
            r.SetResult(item.type);
            r.AddRecipe();
        }

        public override void UpdateInventory(Player player)
        {
            player.rulerGrid = true;
        }
        public override bool AltFunctionUse(Player player)
        {
            item.hammer = 67; // Exactly enough to one-shot walls
            item.createWall = -1;
            item.tileWand = -1;

            if (player.whoAmI == Main.myPlayer)
            {
                try
                {
                    if (Main.wallHouse[Main.tile[Player.tileTargetX, Player.tileTargetY].wall]) return true;
                }
                catch { return false; }
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
