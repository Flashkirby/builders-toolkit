using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class AutoSloper : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Resonance Roller");
            Tooltip.SetDefault("Hammers tiles into slopes\n<right> to remove slopes");
        }
        public const int tileRange = 8;
        public override void SetDefaults()
        {
            item.rare = 6;
            item.UseSound = SoundID.Item1;
            item.useStyle = 1;
            item.useTurn = true;
            item.useAnimation = 10;
            item.useTime = 10;
            item.autoReuse = true;
            item.tileBoost = 12;
            item.width = 20;
            item.height = 20;
            item.shoot = mod.ProjectileType<Projectiles.AutoSloper>();
            item.value = Item.buyPrice(0, 15, 0, 0);
        }
        public override bool AltFunctionUse(Player player) { return true; }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 mouseDistance = Main.MouseWorld - player.Center;
            int totalRange = tileRange + item.tileBoost + player.blockRange;
            if (mouseDistance.X >= 16 * totalRange) return false;
            if (mouseDistance.X <= 16 * -totalRange) return false;
            if (mouseDistance.Y >= 16 * totalRange) return false;
            if (mouseDistance.Y <= 16 * -totalRange) return false;

            Projectile.NewProjectile(player.Center + mouseDistance, default(Vector2), type, 0, 0f, player.whoAmI,
                player.altFunctionUse > 0 ? -1 : 1);
            return false;
        }
    }
}
