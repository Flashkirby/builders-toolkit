using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

/// <summary>
/// Sprite by San#1917
/// </summary>
namespace BuildPlanner.Projectiles
{
    public class Architect : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 10;
            projectile.height = 10;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
        }

        public Point TargetTile
        { get { return new Point((int)projectile.ai[0], (int)projectile.ai[1]); } }

        public override void AI()
        {
            Player player = MaintainChannel();
            if (projectile.owner != Main.myPlayer) return;

            if (player.noItems || player.CCed || player.dead)
            {
                projectile.Kill();
                return;
            }
            if (Main.mouseRight && Main.mouseRightRelease)
            {
                projectile.Kill();
                player.mouseInterface = true;
                Main.blockMouse = true;
                return;
            }
            if (player.channel) return;

            PlaceTiles();

            projectile.Kill();
        }

        private Player MaintainChannel()
        {
            Player p = Main.player[projectile.owner];
            projectile.timeLeft++;
            int direction = Math.Sign(p.velocity.X);
            if (direction != 0)
            {
                p.ChangeDir(direction);
            }
            p.heldProj = p.whoAmI;
            p.itemTime = 2;
            p.itemAnimation = 2;
            p.itemRotation = 0f;
            return p;
        }

        private void PlaceTiles()
        {
            projectile.ai[0] = Main.MouseWorld.X / 16; projectile.ai[1] = Main.MouseWorld.Y / 16;
            Point StartTile = projectile.position.ToTileCoordinates();
            




            Dust d = Dust.NewDustPerfect(StartTile.ToWorldCoordinates(), DustID.FlameBurst);
            d.fadeIn = 2f;
            d.velocity = default(Vector2);
            d.noGravity = true;
            d = Dust.NewDustPerfect(TargetTile.ToWorldCoordinates(), DustID.FlameBurst);
            d.fadeIn = 2f;
            d.velocity = default(Vector2);
            d.noGravity = true;

        }
    }
}
