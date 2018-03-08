using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class MegaAcorn : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Giant Acorn");
            Tooltip.SetDefault("Will not grow without help");
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
            r.alchemy = true;
            r.AddIngredient(ItemID.Acorn);
            r.AddIngredient(ItemID.PurificationPowder, 30);
            r.AddIngredient(ItemID.Daybloom, 1);
            r.AddIngredient(ItemID.Waterleaf, 1);
            r.AddTile(TileID.Bottles);
            r.SetResult(item.type);
            r.AddRecipe();
        }
    }
}
