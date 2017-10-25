using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Projectiles
{
    public class AutoSloper : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Slope Tool");
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
                        ManageTileSlope(tilePos.X + x, tilePos.Y + y, hammerSlope);
                    }
                }

            }

            projectile.alpha += 20;
            projectile.localAI[0]++;
        }

        public static void ManageTileSlope(int x, int y, bool hammerSlope = true)
        {
            if (!WorldGen.InWorld(x, y, 1)) return;
            Tile t = Main.tile[x, y];
            if (!WorldGen.SolidOrSlopedTile(x, y)) return; // Ignore non-slopable tiles
            if (t.halfBrick()) return; // Ignore half tiles

            // Removing Slopes
            if (!hammerSlope)
            {
                if (t.slope() != Tile.Type_Solid)
                {
                    WorldGen.SlopeTile(x, y, Tile.Type_Solid);
                    if (Main.netMode == 2)
                    { NetMessage.SendData(MessageID.TileChange, -1, -1, null, 14, x, y, 0f, 0, 0, 0); }

                    // Direction Dust
                    Dust d;
                    for (int i = -1; i < 2; i += 2)
                    {
                        d = Dust.NewDustPerfect(new Point(x, y).ToWorldCoordinates(8 - 8 * i, 8 - 8 * i),
                            DustID.t_Martian, new Vector2(2 * i, 0), 0, default(Color), 0.6f);
                        d.noGravity = true;
                        d = Dust.NewDustPerfect(new Point(x, y).ToWorldCoordinates(8 - 8 * i, 8 - 8 * i),
                            DustID.t_Martian, new Vector2(0, 2 * i), 0, default(Color), 0.6f);
                        d.noGravity = true;
                    }
                }
                return;
            }

            if (t.slope() == Tile.Type_Solid)
            {
                // Check adjacent blocks
                bool left = WorldGen.SolidOrSlopedTile(x - 1, y);
                bool right = WorldGen.SolidOrSlopedTile(x + 1, y);
                bool top = WorldGen.SolidOrSlopedTile(x, y - 1);
                bool bottom = WorldGen.SolidOrSlopedTile(x, y + 1);
                int slope = 0;
                // Find slope orientation
                if (bottom && !top)
                {
                    if (left && !right)
                    { slope = Tile.Type_Halfbrick; } // not slopeupright because???
                    else if (right && !left)
                    { slope = Tile.Type_SlopeDownRight; }
                }
                else if (top && !bottom)
                {
                    if (left && !right)
                    { slope = Tile.Type_SlopeDownLeft; }
                    else if (right && !left)
                    { slope = Tile.Type_SlopeUpRight; }
                }
                if (slope == 0) return;

                // Apply sloping
                WorldGen.SlopeTile(x, y, slope);
                if (Main.netMode == 2)
                { NetMessage.SendData(MessageID.TileChange, -1, -1, null, 14, x, y, 0f, 0, 0, 0); }

                // Direction Dust
                for (int i = -1; i < 2; i += 2)
                {
                    Dust d = Dust.NewDustPerfect(new Point(x, y).ToWorldCoordinates(), DustID.t_Martian,
                        default(Vector2), 0, default(Color), 0.6f);
                    Vector2 velocity = default(Vector2);
                    if (slope == Tile.Type_Halfbrick || slope == Tile.Type_SlopeUpRight)
                    { velocity = new Vector2(1, 1); }
                    else if (slope == Tile.Type_SlopeDownRight || slope == Tile.Type_SlopeDownLeft)
                    { velocity = new Vector2(-1, 1); }
                    d.position -= velocity * 8 * i;
                    d.velocity = velocity * 2 * i;
                    d.noGravity = true;
                }
            }
        }
    }
}