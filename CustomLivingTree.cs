using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace BuildPlanner
{
    public class CustomLivingTree
    {
        private const bool DEBUG_GROW = false;
        private static ushort TileWoodTree = TileID.LivingWood;
        private static ushort TileLeafTree = TileID.LeafBlock;
        private static byte WallWoodTree = WallID.LivingWood;
        public static bool GrowLivingTree(int tX, int tY)
        {
            /* Treetop with branches at random alternating intervals
             * Top of tree, one branch either side
             * Trunk with width from 4-6, (wall background)
             * Leaves with random sizes (height up to 3 on branch, 5 on treetop), 2 x per 1 y
             *  - start after certain distance from trunk
             * Roots, 10-16 and. and background versions
             */
            Random genRand = new Random(tX + tX + Main.treeX[0]);

            // Height Limit
            // Main.NewText("Tree cannot grow at this height. ", Color.ForestGreen);
            if (tY < 150) { return false; }

            // Make sure the area above is clear
            // Main.NewText("Tree needs unobstructed path to the sun. " + Main.tile[x, y].type, Color.ForestGreen); 
            for (int x = tX - 25; x <= tX + 25; x++)
            {
                for (int y = 5; y < tY - 5; y++)
                {
                    if (Main.tile[x, y].active() && Main.tileSolid[Main.tile[x, y].type])
                    {
                        return false;
                    }
                }
            }
            
            TileWoodTree = TileID.LivingWood;
            TileLeafTree = TileID.LeafBlock;
            WallWoodTree = WallID.LivingWood;
            for (int i = 1; i <= 3; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    if (Main.tile[tX + j, tY + i] == null) continue;
                    if (Main.tile[tX + j, tY + i].type == TileID.JungleGrass)
                    {
                        TileWoodTree = TileID.LivingMahogany;
                        TileLeafTree = TileID.LivingMahoganyLeaves;
                        WallWoodTree = WallID.LivingWood;
                        break;
                    }
                }
            }

            // Set initial trunk width between 3 and 5
            int trunkLeft = tX - genRand.Next(1, 3);
            int trunkRight = tX + genRand.Next(1, 3);

            int growDir = 1 - genRand.Next(2) * 2;
            // 4-6
            if (growDir > 0) trunkRight++;
            else trunkLeft--;

            Tiles.MegaAcorn.noItemDrop = true; // HACK: killtile no itemdrop doesn't trigger on multikill tile

            // Grow the trunk
            Point treeTop = GrowTrunkUp(tY + 1, genRand, trunkLeft, trunkRight, growDir);

            // Add branches
            GrowBranches(tX, tY, genRand, treeTop.Y);

            // Add Tree top
            GrowTreeTop(treeTop.X, treeTop.Y, genRand);

            // Grow Roots
            GrowRoots(trunkLeft, tY + 2, genRand, trunkRight - trunkLeft + 1, false);
            GrowRoots(trunkLeft, tY + 3, genRand, trunkRight - trunkLeft + 1, true);

            // Grow FX and Vines
            GrowVineAndGores(tX, tY, genRand);

            for (int x = trunkLeft; x <= trunkRight; x++)
            {
                for (int i = 0; i < 4; i++)
                {
                    Dust d = Dust.NewDustDirect(new Point(x, tX).ToWorldCoordinates(0, 0), 16, 16, 7, Main.rand.Next(-30, 31) * 0.1f, -4 * i, 150, default(Color), 2f);
                }
            }

            Tiles.MegaAcorn.noItemDrop = false;

            if (Main.netMode != 1)
            {
                int left, top, width, height;
                left = tX - 25;
                top = treeTop.Y - 25;
                width = 50;
                height = tY + 25 - top;

                NetMessage.SendTileRange(-1, left, top, width, height, TileChangeType.None);
            }
            return true;
        }

        /// <summary> Tree trunk grows up a random set number of tiles, shrinking to a width of 4 </summary>
        private static Point GrowTrunkUp(int tY, Random genRand, int trunkLeft, int trunkRight, int growDir)
        {
            int growHeightCounter = genRand.Next(-10, -5);
            int endY = tY;
            for (int i = 0; i < 150; i++)
            {
                growHeightCounter++;
                // Shrink trunk by 1, until width is no longer at least 4
                if (growHeightCounter > genRand.Next(5, 30)) // guaranteed growth up between 10 and 15 tiles
                {
                    growHeightCounter = 0;
                    if (growDir > 0)
                    { trunkLeft++; }
                    else
                    { trunkRight--; }

                    // stop growing?
                    if (trunkRight - trunkLeft + 1 < genRand.Next(3, 5))
                    {
                        // too small now, break
                        endY = tY - i;
                        break;
                    }
                }

                for (int x = trunkLeft; x <= trunkRight; x++)
                {
                    if (x > trunkLeft && x < trunkRight)
                    { PlaceTreeBackground(x, tY - i, WallWoodTree); }
                    PlaceTreeTile(x, tY - i, TileWoodTree);
                }
            }
            return new Point(trunkLeft + (trunkRight - trunkLeft) / 2, endY);
        }
        /// <summary> Adds alternating branches with leaves across the trunk </summary>
        private static void GrowBranches(int tX, int tY, Random genRand, int trunkTopY)
        {
            int branchDistance = 0;
            int branchDirection = 1 - genRand.Next(2) * 2;
            for (int y = trunkTopY; y <= tY - 3; y++)
            {
                // Add branches left and right at intervals
                if (branchDistance == 0)
                {
                    branchDistance = genRand.Next(4, 17);
                    branchDirection *= -1;
                    AddBranch(tX, y, genRand, 1, 2, 6, 8, branchDirection);
                }
                branchDistance--;
            }
        }
        /// <summary> Adds a winding topper with lots of branching </summary>
        private static void GrowTreeTop(int tX, int tY, Random genRand)
        {
            int tileX = tX;
            int tileY = tY + 1;
            int branchDirection = 1 - genRand.Next(2) * 2;
            int branchCount = 1;
            GrowLeavesAroundTile(tileX, tileY, 5);
            for (int i = 0; i <= genRand.Next(12, 17); i++)
            {
                // Place branches
                if (genRand.Next(branchCount) == 0)
                {
                    branchCount += genRand.Next(2, 3);
                    AddBranch(tileX, tileY, genRand, 4, 1, 4, 6, branchDirection, false);
                    branchDirection *= -1;
                }
                branchCount--;

                // Swerve left and right, weighted to the centre
                if (genRand.Next(3) == 0)
                {
                    PlaceTreeTile(tileX, tileY, TileWoodTree);
                    int swerveDir = 1;
                    if (tileX > tX) swerveDir = -1;
                    if (genRand.Next(4) < 3)
                    { tileX += swerveDir; }
                    else
                    { tileX -= swerveDir; }
                }
                tileY--;

                PlaceTreeTile(tileX, tileY, TileWoodTree);
            }
            GrowLeavesAroundTile(tileX, tileY, 4);
            AddBranch(tileX, tileY, genRand, 1, 5, 4, 6, branchDirection, false);
        }
        /// <summary> Generates a number of roots </summary>
        private static void GrowRoots(int trunkLeft, int tY, Random genRand, int width, bool wall)
        {
            float numOfRoots = genRand.Next(6, 9);
            float rootX, rootY, rootDirection;
            int length = 0;
            for (int i = 0; i <= numOfRoots; i++)
            {
                rootX = trunkLeft + 0.5f + (width * i) / numOfRoots;
                rootY = tY;
                length = genRand.Next(8, 17);
                rootDirection = (i * 2 - numOfRoots) / (numOfRoots);
                for (int l = 0; l <= length; l++)
                {
                    rootX += genRand.Next(0, 100) * 0.01f * rootDirection;
                    if (!wall)
                    { PlaceTreeTile((int)rootX, (int)rootY, TileWoodTree); }
                    else
                    { PlaceTreeBackground((int)rootX, (int)rootY, WallWoodTree); }

                    rootY += 0.3f + 0.1f * genRand.Next(8);
                    if (!wall)
                    { PlaceTreeTile((int)rootX, (int)rootY, TileWoodTree); }
                    else
                    { PlaceTreeBackground((int)rootX, (int)rootY, WallWoodTree); }
                }
            }
        }
        /// <summary> Grows vines and drops leaf gores </summary>
        private static void GrowVineAndGores(int tX, int tY, Random genRand)
        {
            int adjacentVineX = -1;
            for (int x = tX - 25; x <= tX + 25; x++)
            {
                for (int y = 5; y < tY; y++)
                {
                    if (Main.tile[x, y].type == TileLeafTree && WorldGen.TileEmpty(x, y + 1))
                    {
                        int gore = GoreID.TreeLeaf_Normal;
                        if (TileLeafTree == TileID.LivingMahoganyLeaves) gore = GoreID.TreeLeaf_Jungle;
                        Gore.NewGore(new Point(x, y + 1).ToWorldCoordinates(4), Utils.RandomVector2(Main.rand, -10, 0), gore, 0.7f + Main.rand.NextFloat() * 0.6f);
                        Gore.NewGore(new Point(x, y + 1).ToWorldCoordinates(8), Utils.RandomVector2(Main.rand, -5, 5), gore, 0.7f + Main.rand.NextFloat() * 0.6f);
                        Gore.NewGore(new Point(x, y + 1).ToWorldCoordinates(12), Utils.RandomVector2(Main.rand, 0, 10), gore, 0.7f + Main.rand.NextFloat() * 0.6f);

                        // Make vines on the bottom of leaves. Stop vines being right next to each other
                        if (TileLeafTree == TileID.LeafBlock)
                        {
                            if (adjacentVineX + 1 < x && genRand.Next(0, 3) == 0)
                            {
                                adjacentVineX = x;
                                for (int vineLength = 0; vineLength <= genRand.Next(1, 11); vineLength++)
                                {
                                    if (WorldGen.TileEmpty(x, y + 1 + vineLength))
                                    { PlaceTreeTile(x, y + 1 + vineLength, TileID.Vines); }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary> Branch extends out, with occasional stubs, moving up a tile or two </summary>
        private static void AddBranch(int tX, int tY, Random genRand, int baseLeafHeight, int addLeafHeight, int baseLength, int addLength, int direction = 0, bool delayLeaves = true)
        {
            if (direction == 0) direction = 1 - genRand.Next(2) * 2;
            int length = baseLength + genRand.Next(addLength + 1);
            int branchX = tX;
            int branchY = tY + 1;
            int stubCounter = 0;
            int growUpCounter = 0;
            int leafCounter = 0;
            if (!delayLeaves)
            { leafCounter = genRand.Next(5, 9); }

            bool emergeFromTrunk = false; // stub when leaving trunk
            for (int i = 0; i < length; i++)
            {
                if (!emergeFromTrunk && Main.tile[branchX, branchY + 1].type != TileWoodTree)
                { PlaceTreeTile(branchX, branchY + 1, TileWoodTree); emergeFromTrunk = true; }

                if (stubCounter <= 0)
                {
                    stubCounter = genRand.Next(4, 12);
                    // Place a stub up or down one
                    PlaceTreeTile(branchX, branchY + 1 - genRand.Next(2) * 2, TileWoodTree);
                }
                if (growUpCounter <= 0) // branch rises
                {
                    growUpCounter = genRand.Next(4, 16);
                    // leave tile to connect better
                    PlaceTreeTile(branchX, branchY, TileWoodTree);
                    branchY--;
                }
                stubCounter--;
                growUpCounter--;
                if (emergeFromTrunk) leafCounter--;

                //Grow branch
                PlaceTreeTile(branchX, branchY, TileWoodTree);
                branchX += direction;

                if (leafCounter < 0 || !delayLeaves)
                { GrowLeavesAroundTile(branchX, branchY, genRand.Next(baseLeafHeight, baseLeafHeight + addLeafHeight + 1)); }
            }
        }
        /// <summary> Grows leaves in a diamond shape around the tile </summary>
        private static void GrowLeavesAroundTile(int x, int y, int radius)
        {
            for (int tY = -radius; tY <= radius; tY++)
            {
                for (int tX = -radius * 2; tX <= radius * 2; tX++)
                {
                    if (Math.Abs(tY) * 2 + Math.Abs(tX) > radius * 2) continue; // make a stretched diamond shape
                    PlaceTreeTile(x + tX, y + tY, TileLeafTree);
                }
            }
        }

        private static bool PlaceTreeTile(int x, int y, ushort type, int style = 0)
        {
            if (!WorldGen.InWorld(x, y, 1)) return false;
            if (Main.tile[x, y] != null)
            {
                Tile t = Main.tile[x, y];
                if (WorldGen.SolidTile(x, y))
                {
                    // Leaf blocks can only be on empty air
                    if (type == TileLeafTree && t.type == TileWoodTree) return false;
                    if (type == TileLeafTree && t.type == TileLeafTree) return false;

                    // If not in these lists, cannot be replaced
                    if (!TileID.Sets.CanBeClearedDuringOreRunner[t.type] &&
                        !TileID.Sets.Grass[t.type] &&
                        t.type != TileLeafTree && t.type != TileID.Silt &&
                        // Not a low value ore
                        t.type != TileID.Copper && t.type != TileID.Tin &&
                        t.type != TileID.Iron && t.type != TileID.Lead &&
                        t.type != TileID.Silver && t.type != TileID.Tungsten &&
                        // Not killable by lava
                        (t.type < Main.tileLavaDeath.Length && !Main.tileLavaDeath[t.type])
                        ) return false;
                }
            }
            else { Main.tile[x, y] = new Tile(); }

            if (DEBUG_GROW)
            {
                if(type == TileLeafTree)
                {
                    Dust d = Dust.NewDustPerfect(new Point(x, y).ToWorldCoordinates(), DustID.Grass);
                    d.noGravity = true; d.scale = 2f;
                }
                else
                {
                    Dust d = Dust.NewDustPerfect(new Point(x, y).ToWorldCoordinates(), DustID.t_LivingWood);
                    d.noGravity = true; d.scale = 3f;
                }
            }
            else
            {
                WorldGen.KillTile(x, y);
                WorldGen.PlaceTile(x, y, type, false, true, -1, style);
                Main.tile[x, y].slope(0);
            }
            return true;
        }
        private static bool PlaceTreeBackground(int x, int y, byte type)
        {
            if (!WorldGen.InWorld(x, y, 1)) return false;
            if (Main.tile[x, y] != null)
            {
                Tile t = Main.tile[x, y];
                if (t.wall < Main.wallHouse.Length && Main.wallHouse[t.wall]) return false;
            }
            else { Main.tile[x, y] = new Tile(); }

            if (!DEBUG_GROW)
            {
                Main.tile[x, y].wall = 0;
                Main.tile[x, y].wallColor(0);
                WorldGen.PlaceWall(x, y, type, false);
            }
            return true;
        }
    }
}