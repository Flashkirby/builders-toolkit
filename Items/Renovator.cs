using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class Renovator : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Furious Worldforge"); // Name blessed by the man himself (27/10/17)
            Tooltip.SetDefault(
                "Replaces nearby scaffolding\n" +
                "Painted scaffolding filtered by paint\n" +
                "'To use: Place blocks/walls in the forge'");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.Extractinator);
            item.createTile = mod.TileType<Tiles.Renovator>();
            item.rare = 9;

        }
    }
}
