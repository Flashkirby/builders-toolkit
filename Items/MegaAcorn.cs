using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class MegaAcorn : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Living Acorn");
            Tooltip.SetDefault("'It might grow with some assistance'");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.Acorn);
            item.createTile = mod.TileType(this.GetType().Name);
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.width = 24;
            item.height = 24;
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.Acorn);
            r.AddIngredient(ItemID.PurificationPowder, 30);
            r.SetResult(item.type);
            r.AddRecipe();
        }
    }
}
