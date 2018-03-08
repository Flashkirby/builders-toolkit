using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class Architect : ModItem
    {
        internal static int ID;
        internal static int UseAmmoID;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Architect");
            Tooltip.SetDefault("Allows ultimate control over Scaffolding Blocks!\n" +
                "Right Click while holding to edit placement settings");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.WireKite);
            item.width = 24;
            item.height = 24;
            item.rare = 8;
            item.shoot = mod.ProjectileType<Projectiles.Architect>();
            item.useAmmo = mod.ItemType<Scaffold>();
            item.value = Item.buyPrice(0, 15, 0, 0);
        }
        public override void UpdateInventory(Player player)
        {
            player.rulerLine = true;
            player.rulerGrid = true;
        }

        public override void HoldItem(Player player)
        {
            if (UI.ArchitectUI.Settings.MineTiles)
            { item.useAmmo = AmmoID.None; }
            else
            { item.useAmmo = UseAmmoID; }
        }

        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.LaserDrill);
            r.AddIngredient(mod.ItemType<TileWand>());
            r.AddIngredient(ItemID.Ruler);
            r.AddIngredient(ItemID.LaserRuler);
            r.AddTile(TileID.TinkerersWorkbench);
            r.SetResult(item.type);
            r.AddRecipe();
        }
        
        public override bool AltFunctionUse(Player player)
        {
            // Only on initial right click
            if(Main.mouseRight && Main.mouseRightRelease)
            { BuildPlanner.architectUI.ToggleVisibility(); }
            return false;
        }
        
        public override bool ConsumeAmmo(Player player) { return false; }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            position = Main.MouseWorld.ToTileCoordinates().ToWorldCoordinates();
            speedX = player.direction;
            speedY = 0;
            return true;
        }

        public override Vector2? HoldoutOffset() { return new Vector2(2, 0); }
    }
}
