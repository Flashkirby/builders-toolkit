using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Projectiles
{
    public class GrowRay : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Growth Ray");
        }
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
            projectile.friendly = true;
            projectile.timeLeft = 26;
            projectile.alpha = 255;
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.2f;
            
            if ((int)(projectile.localAI[0] + 2) % 3 == 0)
            {
                CheckTileGrowth();
            }

            if (projectile.localAI[0] > 3)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust d = Dust.NewDustDirect(projectile.Center, 0, 0, 110,
                        projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f);
                    d.velocity *= 1.5f;
                    d.scale = 0.4f;
                    d.fadeIn = 0.9f;
                    d.noGravity = true;
                }
            }

            projectile.localAI[0]++;
        }
        private void CheckTileGrowth()
        {
            int left = (int)((projectile.position.X) / 16);
            int right = (int)((projectile.position.X + projectile.width) / 16);
            int top = (int)((projectile.position.Y) / 16);
            int bot = (int)((projectile.position.Y + projectile.height) / 16);
            for (int tY = bot; tY > top - 2; tY--)
            {// Up to Down
                for (int tX = left - 1; tX < right + 1; tX++)
                { // Left to Right
                    if (!WorldGen.InWorld(tX, tY)) continue;
                    if (!GrowTile(Main.tile[tX, tY], tX, tY, new System.Random((int)projectile.ai[1])))
                    {
                        GrowModTree(Main.tile[tX, tY], tX, tY);
                    }
                    if (!WorldGen.TileEmpty(tX, tY))
                    {
                        Dust d = Dust.NewDustDirect(new Point(tX, tY).ToWorldCoordinates(), 0, 0, 110);
                        d.noGravity = true;
                        d.velocity *= 0.3f;
                        d.scale = 0.3f;
                        d.fadeIn = 1.1f;
                    }
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            float speed = projectile.velocity.Length();
            return speed < 2f;
        }
        public override bool? CanCutTiles() { return false; }
        public override void Kill(int timeLeft)
        { CheckTileGrowth(); }

        public static bool GrowTile(Tile tile, int x, int y, System.Random rand)
        {
            GrowFlowerWall(tile, x, y);
            if (!GrowHerbs(tile, x, y - 1))
            {
                if (!GrowGrass(tile, x, y, rand))
                {
                    if (!GrowFlowers(tile, x, y))
                    {
                        if (!GrowTrees(tile, x, y))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public static bool GrowFlowerWall(Tile tile, int x, int y)
        {
            if (tile.wall == WallID.DirtUnsafe || tile.wall == WallID.GrassUnsafe ||
                (tile.wall >= WallID.DirtUnsafe1 && tile.wall <= WallID.DirtUnsafe4))
            {
                if (Main.netMode != 1)
                {
                    tile.wall = WallID.FlowerUnsafe;
                    NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
                }
                return true;
            }

            if (tile.wall == WallID.MudUnsafe ||
                (tile.wall >= WallID.JungleUnsafe1 && tile.wall <= WallID.JungleUnsafe4))
            {
                if (Main.netMode != 1)
                {
                    tile.wall = WallID.JungleUnsafe;
                    NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
                }
                return true;
            }
            return false;
        }
        public static bool GrowGrass(Tile tile, int x, int y, System.Random rand)
        {
            if (!WorldGen.TileEmpty(x, y))
            {
                int replaceType = -1;
                switch (tile.type)
                {
                    case TileID.Dirt: replaceType = TileID.Grass; break;
                    case TileID.Mud: replaceType = TileID.JungleGrass; break;
                    case TileID.Stone:
                        if (y > Main.maxTilesY * 0.7)
                        { replaceType = TileID.LavaMoss; }
                        else
                        {
                            // Look for nearby first
                            for (int mossY = y - 2; mossY <= y + 2; mossY++)
                            {
                                for (int mossX = x - 2; mossX <= x + 2; mossX++)
                                {
                                    try
                                    {
                                        if (Main.tileMoss[Main.tile[mossX, mossY].type])
                                        { replaceType = Main.tile[mossX, mossY].type; }
                                    }
                                    catch { }
                                }
                            }
                            if (replaceType == -1)
                            {
                                replaceType = rand.Next(179, 185);
                            }
                        }
                        break;

                }

                if (replaceType >= 0)
                {
                    if (Main.netMode != 1)
                    {
                        WorldGen.PlaceTile(x, y, replaceType, false, true);
                        NetMessage.SendData(17, -1, -1, null, 1, x, y, (float)replaceType, 0, 0, 0);
                    }
                    return true;
                }
            }
            return false;
        }
        public static bool GrowFlowers(Tile tile, int x, int y)
        {
            // Empty, dry tile with a solid tile beneath
            if (WorldGen.TileEmpty(x, y) && tile.liquid == 0 && WorldGen.SolidTile(x, y + 1))
            {
                tile.frameY = 0;
                tile.slope(0);
                tile.halfBrick(false);
                int grassType = Main.tile[x, y] == null ? -1 : Main.tile[x, y + 1].type;
                int min = -1, max = -1, exclude = -1;
                if (grassType == TileID.Grass)
                {
                    if (Main.rand.Next(2) == 0) // Short or long flowers
                    {
                        tile.type = TileID.Plants;
                        min = 6; max = 11; exclude = 8;
                    }
                    else
                    {
                        tile.type = TileID.Plants2;
                        min = 6; max = 21; exclude = 8;
                    }
                }
                else if (grassType == TileID.HallowedGrass)
                {
                    if (Main.rand.Next(2) == 0) // Short or long flowers
                    {
                        tile.type = TileID.HallowedPlants;
                        min = 4; max = 7; exclude = 5;
                    }
                    else
                    {
                        tile.type = TileID.HallowedPlants2;
                        min = 2; max = 8; exclude = 5;
                    }
                }
                else if (grassType == TileID.JungleGrass)
                {
                    if (Main.rand.Next(2) == 0) // Short or long flowers
                    {
                        tile.type = TileID.JunglePlants;
                        min = 10; max = 23; exclude = -1;
                    }
                    else
                    {
                        tile.type = TileID.JunglePlants2;
                        min = 9; max = 17; exclude = -1;
                    }
                }

                if (min >= 0 && max > min)
                {
                    Main.PlaySound(SoundID.Item54, new Point(x, y).ToWorldCoordinates());
                    if (Main.netMode != 1)
                    {
                        tile.active(true);
                        tile.frameX = (short)(18 * Main.rand.Next(min, max));
                        // Don't spawn mushrooms
                        while (tile.frameX == exclude * 18)
                        { tile.frameX = (short)(18 * Main.rand.Next(min, max)); }
                        NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
                    }
                    return true;
                }
            }
            return false;
        }
        public static bool GrowTrees(Tile tile, int x, int y)
        {
            // Grow the tree (server auto sends netmessage and FX)
            if (TileLoader.IsSapling(Main.tile[x, y].type))
            {
                Main.PlaySound(SoundID.Item60, new Point(x, y).ToWorldCoordinates());
                if (Main.netMode != 1 && WorldGen.GrowTree(x, y))
                {
                    WorldGen.TreeGrowFXCheck(x, y);
                    return true;
                }
            }
            return false;
        }
        public static bool GrowHerbs(Tile tile, int x, int y)
        {
            if (tile.type == 82)
            {
                Main.PlaySound(SoundID.Grass, new Point(x, y).ToWorldCoordinates());
                tile.type = 83;
                if (Main.netMode != 1)
                {
                    NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
                    WorldGen.SquareTileFrame(x, y, true);
                }
                return true;
            }
            return false;
        }

        public bool GrowModTree(Tile tile, int x, int y)
        {
            if (!WorldGen.SolidTile(x, y + 1) || tile.type != mod.TileType<Tiles.MegaAcorn>()) return false;
            if( CustomLivingTree.GrowLivingTree(x, y))
            {
                Main.PlaySound(SoundID.Item81, new Point(x, y).ToWorldCoordinates());
                return true;
            }
            return false;
        }
    }
}