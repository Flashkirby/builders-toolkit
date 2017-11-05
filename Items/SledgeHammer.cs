using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class SledgeHammer : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Clears scaffolding");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.TheBreaker);
            item.damage = 27;
            item.useAnimation = 25;
            item.hammer = 0;
            item.scale = 1.2f;
            item.rare = 2;
        }
    }
}
