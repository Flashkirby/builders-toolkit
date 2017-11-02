using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class Architect : ModItem
    {
        internal static int ID;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Architect");
            Tooltip.SetDefault("Allows ultimate control over scaffolding!\n" +
                "Right Click while holding to edit placement settings");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.WireKite);
            item.width = 24;
            item.height = 24;
            item.rare = 8;
            item.shoot = mod.ProjectileType<Projectiles.Architect>();
            item.value = Item.buyPrice(0, 25, 0, 0);
        }

        public override bool AltFunctionUse(Player player)
        {
            // Only on initial right click
            if(Main.mouseRight && Main.mouseRightRelease)
            { BuildPlanner.architectUI.ToggleVisibility(); }
            return false;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            position = Main.MouseWorld.ToTileCoordinates().ToWorldCoordinates();
            speedX = 0;
            speedY = 0;
            return true;
        }

        public override Vector2? HoldoutOffset() { return new Vector2(2, 0); }
    }
}
