using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
	public class Scaffold : ModItem
    {
        public static void SetDefaultsBasic(Item item)
        {
            item.useStyle = 1;
            item.useTurn = true;
            item.useAnimation = 15;
            item.useTime = 3;
            item.autoReuse = true;
            item.maxStack = 4096;
            item.consumable = true;
            item.width = 12;
            item.height = 12;
        }

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Scaffolding Block");
            Tooltip.SetDefault("Can be replaced with other blocks");
        }
		public override void SetDefaults()
        {
            Scaffold.SetDefaultsBasic(item);
            item.createTile = mod.TileType(this.GetType().Name);
        }

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Wood);
            recipe.anyWood = true;
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this, 64);
			recipe.AddRecipe();
            // Can also make near self
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Wood);
            recipe.anyWood = true;
            recipe.AddTile(item.createTile);
            recipe.SetResult(this, 64);
            recipe.AddRecipe();
        }
    }
    public class ScaffoldPlatform : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scaffolding Platform");
            Tooltip.SetDefault("Can be replaced with other blocks");
        }
        public override void SetDefaults()
        {
            Scaffold.SetDefaultsBasic(item);
            item.width = 8;
            item.height = 10;
            item.createTile = mod.TileType(this.GetType().Name);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<Scaffold>(), 16);
            recipe.SetResult(this, 32);
            recipe.AddRecipe();
        }
    }
    public class ScaffoldWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scaffolding Wall");
        }
        public override void SetDefaults()
        {
            Scaffold.SetDefaultsBasic(item);
            item.useTime = 2;
            item.createWall = mod.WallType(this.GetType().Name);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(mod.ItemType<Scaffold>(), 16);
            recipe.SetResult(this, 64);
            recipe.AddRecipe();
        }

        public override bool UseItem(Player player)
        {
            player.wallSpeed = 1f; return false;
        }
    }
}
