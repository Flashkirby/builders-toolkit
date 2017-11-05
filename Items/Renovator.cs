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
                "Only replaces tiles with matching paint\n" +
                "'To use: Place blocks/walls in the forge'");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.Extractinator);
            item.createTile = mod.TileType<Tiles.Renovator>();
            item.rare = 9;

        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.LunarBar, 5);
            r.AddIngredient(ItemID.FragmentSolar, 5);
            r.AddIngredient(ItemID.FragmentVortex, 5);
            r.AddIngredient(ItemID.FragmentNebula, 5);
            r.AddIngredient(ItemID.FragmentStardust, 5);
            r.AddTile(TileID.LunarCraftingStation);
            r.SetResult(item.type);
            r.AddRecipe();
        }
    }
}
