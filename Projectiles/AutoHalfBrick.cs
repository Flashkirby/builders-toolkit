using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Projectiles
{
    public class AutoHalfBrick : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Step Tool");
        }
        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 80;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.alpha = 100;
            projectile.timeLeft = 6;
            projectile.tileCollide = false;
        }
        public override bool? CanCutTiles() { return false; }

        public override void AI()
        {
            if (projectile.localAI[0] == 0)
            {
                projectile.position += new Vector2(8, 8);
                projectile.position = projectile.position.ToTileCoordinates().ToWorldCoordinates(0, 0);
                bool hammerSlope = projectile.ai[0] > 0;
                Point tilePos = projectile.Center.ToTileCoordinates();
                for (int y = -2; y < 3; y++)
                {
                    for (int x = -2; x < 3; x++)
                    {
                        ManageTileStep(tilePos.X + x, tilePos.Y + y, projectile.owner, hammerSlope);
                    }
                }

                if (Main.netMode == 2)
                {
                    NetMessage.SendTileSquare(-1, tilePos.X, tilePos.Y, 5);
                }
            }

            projectile.alpha += 20;
            projectile.localAI[0]++;
        }

        public static void ManageTileStep(int x, int y, int client, bool hammerStep = true)
        {
            if (!WorldGen.InWorld(x, y, 1)) return;
            Tile t = Main.tile[x, y];
            if (!WorldGen.SolidOrSlopedTile(x, y)) return; // Ignore non-slopable tiles
            if (t.slope() > 0) return; // Ignore slopes

            // Removing Steps
            if (!hammerStep)
            {
                if (t.halfBrick())
                {
                    WorldGen.PoundTile(x, y);

                    // Direction Dust
                    Dust d;
                    d = Dust.NewDustPerfect(new Point(x, y).ToWorldCoordinates(0, 0),
                        133, new Vector2(1.2f, 0), 0, default(Color), 0.6f);
                    d.noGravity = true;
                    d = Dust.NewDustPerfect(new Point(x, y).ToWorldCoordinates(16, 0),
                        133, new Vector2(-1.2f, 0), 0, default(Color), 0.6f);
                    d.noGravity = true;
                }
                return;
            }

            if (!t.halfBrick())
            {
                // Check adjacent blocks
                if (!WorldGen.SolidOrSlopedTile(x, y - 1) &&
                    !WorldGen.SolidOrSlopedTile(x - 1, y - 1) &&
                    !WorldGen.SolidOrSlopedTile(x + 1, y - 1))
                {
                    bool left = WorldGen.SolidOrSlopedTile(x - 1, y);
                    bool leftDown = WorldGen.SolidOrSlopedTile(x - 1, y + 1);
                    bool right = WorldGen.SolidOrSlopedTile(x + 1, y);
                    bool rightDown = WorldGen.SolidOrSlopedTile(x + 1, y + 1);

                    if ((!left && leftDown) || (!right && rightDown))
                    {
                        WorldGen.PoundTile(x, y);

                        // Direction Dust
                        Dust d;
                        d = Dust.NewDustPerfect(new Point(x, y).ToWorldCoordinates(0, 8),
                            133, new Vector2(1.2f, 0), 0, default(Color), 0.6f);
                        d.noGravity = true;
                        d = Dust.NewDustPerfect(new Point(x, y).ToWorldCoordinates(16, 8),
                            133, new Vector2(-1.2f, 0), 0, default(Color), 0.6f);
                        d.noGravity = true;
                    }

                }
            }
        }
    }
}