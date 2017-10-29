using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class TreeCycler : ModItem
    {

        internal static int localJungleCooldown = 0;
        internal static int localSnowCooldown = 0;
        internal static int localForestCooldown = 0;
        private static int[] jungleBGs = new int[] { 0, 1 };
        private static string[] jungleDesc = new string[] { "Canopy Jungle", "Wild Jungle" };
        private static int[] snowBGs = new int[] { 0, 1, 2, 21, 22, 3, 31, 32, 4, 41, 42 };
        private static string[] snowDesc = new string[]
        { "Snowy Forest", "Dormant Hills", "Boreal Glacier West", "Dormant Glacier", "Dormant Taiga", "Boreal Mountain East", "Dormant Mountain", "Boreal Forest West", "Evergreen West", "Dormant Evergreen", "Boreal Forest West" };
        private static int[] forestTreeStyles = new int[] { 0, 1, 2, 3, 4, 5 };
        private static string[] forestTreeDesc = new string[]
        { "Classic", "Bonsai", "Weeping", "Tropical", "Waxy", "Budding",  };

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Titanic Terraformer");
            Tooltip.SetDefault("Cycles the biome style of the current area\n" +
                "<right> to cycle in reverse\n" +
                "Can be used in the Forest, Snow and Jungle");
        }
        public override void SetDefaults()
        {
            item.rare = 7;
            item.UseSound = SoundID.Item82;
            item.useStyle = 5;
            item.useTurn = true;
            item.useAnimation = 60;
            item.useTime = 60;
            item.autoReuse = true;
            item.width = 20;
            item.height = 20;
            item.value = Item.buyPrice(0, 15, 0, 0);
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.netMode != 0)
            { tooltips.Add(new TooltipLine(mod, "TooltipNoMP", "Cannot be used in Multiplayer")); }
        }

        public override bool CanUseItem(Player player)
        {
            if (Main.netMode != 0) return false;
            return player.ZoneSkyHeight || player.ZoneOverworldHeight || player.ZoneDirtLayerHeight;
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool UseItem(Player player)
        {
            int tileX = (int)player.Center.X / 16;
            int tileY = (int)player.Center.Y / 16;
            int screenTileLeft = tileX - 8 - Main.screenWidth / 16;
            int screenTileRight = tileX + 8 + Main.screenWidth / 16;
            int screenTileTop = tileY - 8 - Main.screenHeight / 16;
            int screenTileBottom = tileY + 8 + Main.screenHeight / 16;
            int cycleDir = player.altFunctionUse > 0 ? -1 : 1;

            if (player.ZoneJungle)
            {
                CycleBackground(ref WorldGen.jungleBG, jungleBGs, cycleDir);
                WorldGen.setBG(2, WorldGen.jungleBG);
                if (localJungleCooldown <= 480)
                {
                    GrowScreenTreeFX(screenTileLeft, screenTileRight, screenTileTop, screenTileBottom, TileID.JungleGrass);
                    localJungleCooldown += 480;
                }
            }
            else if (player.ZoneSnow)
            {
                CycleBackground(ref WorldGen.snowBG, snowBGs, cycleDir);
                WorldGen.setBG(3, WorldGen.snowBG);
                if (localSnowCooldown <= 480)
                {
                    GrowScreenTreeFX(screenTileLeft, screenTileRight, screenTileTop, screenTileBottom, TileID.SnowBlock);
                    localSnowCooldown += 480;
                }
            }
            else // Forest Biome
            {
                for (int i = 0; i < Main.treeX.Length; i++)
                {
                    int treeIndex = -1; // TreeX content is 1, 2, or 3 depending on world size
                    if (i > 2) { treeIndex = 3; }
                    else if (tileX <= Main.treeX[i]) { treeIndex = i; }
                    if (treeIndex < 0) continue;

                    CycleBackground(ref Main.treeStyle[i], forestTreeStyles, cycleDir);
                    if (localForestCooldown <= 480)
                    {
                        GrowScreenTreeFX(screenTileLeft, screenTileRight, screenTileTop, screenTileBottom, TileID.Grass);
                        localForestCooldown += 480;
                    }
                    break;
                }

                #region Randomise Background
                // Forest BG can be any of styles 0, 1, 2, 3, 31, 4, 5, 51, 6, 7, 71, 72, 73, 8
                //RandomiseForestBG();
                #endregion
            }
            return true;
        }

        private static void CycleBackground(ref int currentBG, int[] backgrounds, int direction, bool recur = true)
        {
            for (int i = 0; i < backgrounds.Length; i++)
            {
                if (currentBG == backgrounds[i])
                {
                    i += direction;
                    if (i >= backgrounds.Length) i = 0;
                    if (i < 0) i = backgrounds.Length - 1;
                    currentBG = backgrounds[i];
                    Main.BlackFadeIn = 255;
                    return;
                }
            }
            // Npt found eh? Recurseively try once
            if (recur)
            {
                currentBG = backgrounds[0];
                CycleBackground(ref currentBG, backgrounds, direction, false);
            }
        }

        private static void GrowScreenTreeFX(int screenTileLeft, int screenTileRight, int screenTileTop, int screenTileBottom, int groundTile)
        {
            int middle = (screenTileLeft + screenTileRight) / 2;
            for (int y = screenTileTop; y < screenTileBottom; y++)
            {
                if (!WorldGen.InWorld(middle, y)) continue; // skip whole y layer if not applicable
                for (int x = screenTileLeft; x < screenTileRight; x++)
                {
                    if (!WorldGen.InWorld(x, y)) continue;
                    // Make sure it is the main trunk of the tree since branches cannot be closer than 2 tiles
                    if (Main.tile[x, y].type == TileID.Trees &&
                        Main.tile[x, y - 1].type == TileID.Trees &&
                        Main.tile[x, y - 2].type == TileID.Trees &&
                        Main.tile[x, y + 1].type == groundTile)
                    {
                        WorldGen.TreeGrowFXCheck(x, y);
                    }
                }
            }
        }
        private static void RandomiseForestBG()
        {
            WorldGen.treeBG = WorldGen.genRand.Next(9);
            if ((WorldGen.treeBG == 1 || WorldGen.treeBG == 2) && WorldGen.genRand.Next(2) == 0)
            {
                WorldGen.treeBG = WorldGen.genRand.Next(7);
            }
            if (WorldGen.treeBG == 0)
            {
                WorldGen.treeBG = WorldGen.genRand.Next(7);
            }
            if (WorldGen.treeBG == 3 && WorldGen.genRand.Next(3) == 0)
            {
                WorldGen.treeBG = 31;
            }
            if (WorldGen.treeBG == 5 && WorldGen.genRand.Next(2) == 0)
            {
                WorldGen.treeBG = 51;
            }
            if (WorldGen.treeBG == 7 && WorldGen.genRand.Next(4) == 0)
            {
                WorldGen.treeBG = WorldGen.genRand.Next(71, 74);
            }
            WorldGen.setBG(0, WorldGen.treeBG);
        }

        #region Draws
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.mouseText || Main.playerInventory || Main.HoveringOverAnNPC) return;
            if (!CanUseItem(Main.LocalPlayer)) return;
            if (Main.LocalPlayer.HeldItem.type != item.type) return;
            string current = "";
            string next = "";
            string prev = "";
            if (Main.LocalPlayer.ZoneJungle)
            {
                GetDescription(WorldGen.jungleBG, jungleBGs, jungleDesc, ref current, ref next, ref prev);
            }
            else if (Main.LocalPlayer.ZoneSnow)
            {
                GetDescription(WorldGen.snowBG, snowBGs, snowDesc, ref current, ref next, ref prev);
            }
            else
            {
                for (int i = 0; i < Main.treeX.Length; i++)
                {
                    int treeIndex = -1; // TreeX content is 1, 2, or 3 depending on world size
                    if (i > 2) { treeIndex = 3; }
                    else if ((int)Main.LocalPlayer.Center.X / 16 <= Main.treeX[i]) { treeIndex = i; }
                    if (treeIndex < 0) continue;

                    GetDescription(Main.treeStyle[i], forestTreeStyles, forestTreeDesc, ref current, ref next, ref prev);
                    break;
                }
            }
            Main.instance.MouseText("<¬ " + next + "\nStyle: " + current + "\n-> " + prev, 2);
        }

        private void GetDescription(int currentBG, int[] backgrounds, string[] descriptions, ref string current, ref string next, ref string prev)
        {
            int posCur = 0;
            for (int i = 0; i < backgrounds.Length; i++)
            { if (currentBG == backgrounds[i]) { posCur = i; break; } }

            int posNext = posCur + 1;
            if (posNext >= backgrounds.Length) posNext = 0;
            int posPrev = posCur - 1;
            if (posPrev < 0) posPrev = backgrounds.Length - 1;

            current = descriptions[posCur];
            next = descriptions[posNext];
            prev = descriptions[posPrev];
        }
        #endregion
    }
}