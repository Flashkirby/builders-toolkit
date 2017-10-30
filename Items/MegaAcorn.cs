using Terraria;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class MegaAcorn : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Living Acorn");
            Tooltip.SetDefault("It might grow with a little help\n'Don't stand too close!'");
        }
        public override void SetDefaults()
        {
            item.useStyle = 1;
            item.useTurn = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.autoReuse = true;
            item.consumable = true;
            item.width = 20;
            item.height = 20;
            item.createTile = mod.TileType(this.GetType().Name);
            item.rare = 1;
            item.value = Item.buyPrice(0, 5, 0, 0);
        }
    }
}
