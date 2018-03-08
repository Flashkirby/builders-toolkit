using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    internal class PurificationPower : GlobalItem
    {
        internal static int powderAmmo = AmmoID.None;
        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.PurificationPowder)
            {
                if (item.ammo == AmmoID.None)
                { item.ammo = item.type; }
                powderAmmo = item.ammo;
            }
        }
    }
    /// <summary>
    /// Sprite by San#1917
    /// </summary>
    public class WateringCan : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Watering Can");
            Tooltip.SetDefault("Uses Purification Powder\nCauses plants to sprout");
        }

        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.useAnimation = 15;
            item.useTime = 14;
            item.autoReuse = true;
            item.useStyle = 5;
            item.shoot = mod.ProjectileType<Projectiles.GrowRay>();
            item.shootSpeed = 6f;
            item.UseSound = SoundID.Item13;
            item.useAmmo = PurificationPower.powderAmmo;
            item.scale = 0.85f;

            item.rare = 2;
            item.value = Item.buyPrice(0, 15, 0, 0);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            speedX += player.velocity.X / 2;
            speedY += player.velocity.Y / 2;
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), item.shoot, 0, 0f, player.whoAmI, 0, Main.rand.Next(0, 65536));
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6, 6);
        }
    }
}