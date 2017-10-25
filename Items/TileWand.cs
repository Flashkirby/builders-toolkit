using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    // TODO: sold by merchant when carrying scaffolding
    public class TileWand : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Architect: Blocks");
            Tooltip.SetDefault("Places Scaffolding\n<right> to mine tiles");
        }
        public static void WandDefaults(Item item)
        {
            Scaffold.SetDefaultsBasic(item);
            item.consumable = false;
            item.maxStack = 1;
            item.useAnimation = 2;
            item.useTime = 1;
            item.useStyle = 5;
            item.holdStyle = 3;

            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 2;
            item.autoReuse = true;
        }
        public override void SetDefaults()
        {
            TileWand.WandDefaults(item);
            item.useAnimation = 3;

            item.tileBoost = 12;
            item.tileWand = mod.ItemType<Scaffold>();
            item.createTile = mod.GetItem<Scaffold>().item.createTile;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(mod.ItemType<WallWand>());
            r.SetResult(item.type);
            r.AddRecipe();
        }

        public override Vector2? HoldoutOffset() { return new Vector2(2, 0); }
        public override void UpdateInventory(Player player)
        {
            player.rulerLine = true;
        }
        public override bool AltFunctionUse(Player player)
        {
            item.pick = 1;
            item.createTile = -1;
            item.tileWand = -1;
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse <= 0 && item.createTile == -1)
            {
                item.createTile = mod.GetItem<Scaffold>().item.createTile;
                item.tileWand = mod.ItemType<Scaffold>();
                item.pick = 0;
            }
            return true;
        }
    }
}
