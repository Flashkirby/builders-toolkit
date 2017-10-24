using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Projectiles
{
    public class Wallbeam : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wonderbeam");
        }
        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 36;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.magic = true;
            projectile.penetrate = -1;
            projectile.alpha = 255;
            projectile.timeLeft = 15;
            projectile.tileCollide = false;
        }

        public float LaserLength { get { return projectile.localAI[1]; } set { projectile.localAI[1] = value; } }
        public const float LaserLengthMax = 420f;
        public const int hammerSpeed = 6;
        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (player.dead || !player.active) { projectile.timeLeft = 0; return; }
            projectile.gfxOffY = player.gfxOffY;

            projectile.rotation = projectile.velocity.ToRotation() - 1.57079637f;
            projectile.velocity = Vector2.Normalize(projectile.velocity);
            projectile.Center = player.Center + projectile.velocity * 16;

            float[] sampleArray = new float[2];
            Collision.LaserScan(projectile.Center, projectile.velocity, 0, LaserLengthMax, sampleArray);
            float sampledLength = 0f;
            for (int i = 0; i < sampleArray.Length; i++)
            {
                sampledLength += sampleArray[i];
            }
            sampledLength /= sampleArray.Length;
            float amount = 0.75f; // last prism is 0.75 rather than 0.5?
            LaserLength = MathHelper.Lerp(LaserLength, sampledLength, amount);

            #region Dusts
            Vector2 endPoint = projectile.Center + projectile.velocity * (projectile.localAI[1] - 14f);
            for (int i = 0; i < 2; i++)
            {
                float num809 = projectile.velocity.ToRotation() + ((Main.rand.Next(2) == 1) ? -1f : 1f) * 1.57079637f;
                float num810 = (float)Main.rand.NextDouble() * 2f + 2f;
                Vector2 vector79 = new Vector2((float)Math.Cos((double)num809) * num810, (float)Math.Sin((double)num809) * num810);
                int num811 = Dust.NewDust(endPoint, 0, 0, 229, vector79.X, vector79.Y, 0, default(Color), 1f);
                Main.dust[num811].noGravity = true;
                Main.dust[num811].scale = 1.7f;
            }
            if (Main.rand.Next(5) == 0)
            {
                Vector2 value29 = projectile.velocity.RotatedBy(1.5707963705062866, default(Vector2)) * ((float)Main.rand.NextDouble() - 0.5f) * (float)projectile.width;
                int num812 = Dust.NewDust(endPoint + value29 - Vector2.One * 4f, 8, 8, 31, 0f, 0f, 100, default(Color), 1.5f);
                Dust dust3 = Main.dust[num812];
                dust3.velocity *= 0.5f;
                Main.dust[num812].velocity.Y = -Math.Abs(Main.dust[num812].velocity.Y);
            }
            #endregion

            projectile.ai[0]++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            return (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + projectile.velocity * LaserLength, projHitbox.Width, ref collisionPoint));
        }
        public override bool? CanCutTiles()
        {
            DelegateMethods.tilecut_0 = Terraria.Enums.TileCuttingContext.AttackProjectile;
            Utils.PlotTileLine(projectile.Center, projectile.Center + projectile.velocity * LaserLength, (float)projectile.width * projectile.scale * 2, new Utils.PerLinePoint(CutTilesAndBreakWalls));
            return true;
        }

        private bool CutTilesAndBreakWalls(int x, int y)
        {
            BreakWalls(x, y);
            return DelegateMethods.CutTiles(x, y);
        }

        public void BreakWalls(int x, int y)
        {
            if ((projectile.ai[0] - 1) % hammerSpeed != 0) return; // Not digging
            if ((int)projectile.ai[1] <= 0) return; // No hammer power given...

            // How about give me a real man's tile value
            if (!WorldGen.InWorld(x, y, 1)) return;

            // Dude there's no wall here
            if (Main.tile[x, y].wall <= 0) return;

            Player player = Main.player[projectile.owner];
            if (player.dead || !player.active) return;

            // Get tile
            int tileId = player.hitTile.HitObject(x, y, 2); // 1 for tiles, 2 for walls

            // Can we break item
            int itemTilePower = 0;
            if (Main.tileNoFail[(int)Main.tile[x, y].type])
            { itemTilePower = 100; }

            // Try hitting
            TileLoader.MineDamage((int)projectile.ai[1], ref itemTilePower);

            #region Huzzah, we can actually mine the darn thing
            AchievementsHelper.CurrentlyMining = true;

            AchievementsHelper.CurrentlyMining = true;

            // Try 3 times to make sure we kill it guaranteed or not at all
            bool killed = false;
            for (int i = 0; i < 3; i++)
            {
                if (player.hitTile.AddDamage(tileId, itemTilePower, true) >= 100)
                {
                    player.hitTile.Clear(tileId);
                    killed = true;
                }
                else
                {
                    killed = false;
                }
            }
            if (killed)
            {
                WorldGen.KillWall(x, y, true);
                if (Main.netMode != 0)
                {
                    NetMessage.SendData(17, -1, -1, null, 0, (float)x, (float)y, 1f, 0, 0, 0);
                }
            }
            else
            {
                WorldGen.KillWall(x, y, false);
                if (Main.netMode != 0)
                {
                    NetMessage.SendData(17, -1, -1, null, 0, (float)x, (float)y, 0f, 0, 0, 0);
                }
            }

            if (itemTilePower > 0) player.hitTile.Prune();

            AchievementsHelper.CurrentlyMining = false;
            #endregion
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.velocity == Vector2.Zero)
            {
                return false;
            }
            Texture2D texture2D19 = Main.projectileTexture[projectile.type];
            Texture2D texture2D20 = Main.extraTexture[21];
            Texture2D texture2D21 = Main.extraTexture[22];
            float num228 = LaserLength;
            Color color44 = new Microsoft.Xna.Framework.Color(255, 255, 255, 0) * 0.9f;
            Texture2D arg_AF99_1 = texture2D19;
            Vector2 arg_AF99_2 = projectile.Center + new Vector2(0, projectile.gfxOffY) - Main.screenPosition;
            Rectangle? sourceRectangle2 = null;
            spriteBatch.Draw(arg_AF99_1, arg_AF99_2, sourceRectangle2, color44, projectile.rotation, texture2D19.Size() / 2f, projectile.scale, SpriteEffects.None, 0f);
            num228 -= (float)(texture2D19.Height / 2 + texture2D21.Height) * projectile.scale;
            Vector2 value20 = projectile.Center + new Vector2(0, projectile.gfxOffY);
            value20 += projectile.velocity * projectile.scale * (float)texture2D19.Height / 2f;
            if (num228 > 0f)
            {
                float num229 = 0f;
                Microsoft.Xna.Framework.Rectangle rectangle7 = new Microsoft.Xna.Framework.Rectangle(0, 16 * (projectile.timeLeft / 3 % 5), texture2D20.Width, 16);
                while (num229 + 1f < num228)
                {
                    if (num228 - num229 < (float)rectangle7.Height)
                    {
                        rectangle7.Height = (int)(num228 - num229);
                    }
                    Main.spriteBatch.Draw(texture2D20, value20 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle7), color44, projectile.rotation, new Vector2((float)(rectangle7.Width / 2), 0f), projectile.scale, SpriteEffects.None, 0f);
                    num229 += (float)rectangle7.Height * projectile.scale;
                    value20 += projectile.velocity * (float)rectangle7.Height * projectile.scale;
                    rectangle7.Y += 16;
                    if (rectangle7.Y + rectangle7.Height > texture2D20.Height)
                    {
                        rectangle7.Y = 0;
                    }
                }
            }
            SpriteBatch arg_B1FF_0 = Main.spriteBatch;
            Texture2D arg_B1FF_1 = texture2D21;
            Vector2 arg_B1FF_2 = value20 - Main.screenPosition;
            sourceRectangle2 = null;
            arg_B1FF_0.Draw(arg_B1FF_1, arg_B1FF_2, sourceRectangle2, color44, projectile.rotation, texture2D21.Frame(1, 1, 0, 0).Top(), projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
