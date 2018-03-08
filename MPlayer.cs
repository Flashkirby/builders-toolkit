using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using BuildPlanner.Items;

namespace BuildPlanner
{
    public class MPlayer : ModPlayer
    {
        public static int ScaffoldType = -1;
        public static int PlatformType = -1;

        public override void OnEnterWorld(Player player)
        {
            ScaffoldType = mod.GetTile<Tiles.Scaffold>().Type;
            PlatformType = mod.GetTile<Tiles.ScaffoldPlatform>().Type;
            TreeCycler.localForestCooldown = 0;
            TreeCycler.localSnowCooldown = 0;
            TreeCycler.localJungleCooldown = 0;
        }
        private bool storedHalfBrick = false;
        private byte storedSlope = 0;
        private int storedType = -1;
        public override bool PreItemCheck()
        {
            if (player.whoAmI != Main.myPlayer) return true;

            // TreeCycler internal cooldown
            if (TreeCycler.localForestCooldown > 0) TreeCycler.localForestCooldown--;
            if (TreeCycler.localSnowCooldown > 0) TreeCycler.localSnowCooldown--;
            if (TreeCycler.localJungleCooldown > 0) TreeCycler.localJungleCooldown--;

            AddScaffoldCuttable();

            CheckRenovatorTile();

            ScaffoldSlopeSetup();

            return true;
        }

        public override void PostItemCheck()
        {
            if (player.whoAmI != Main.myPlayer) return;

            RemoveScaffoldCuttable();

            ScaffoldSlopeReplace();
            
            FireMossPlacement(player);
        }

        internal void FireMossPlacement(Player player)
        {
            if (player.HeldItem.type == mod.ItemType<FireMossSeeds>())
            {
                if (player.toolTime == 0 && player.controlUseItem)
                {
                    Item item = player.HeldItem;
                    if (player.whoAmI == Main.myPlayer &&
                        FireMossSeeds.ValidTarget(Player.tileTargetX, Player.tileTargetY))
                    {
                        if (WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, item.createTile, false, true, player.whoAmI))
                        {
                            player.ConsumeItem(mod.ItemType<FireMossSeeds>());
                            NetMessage.SendData(17, -1, -1, null, 1,
                                (float)Player.tileTargetX, (float)Player.tileTargetY,
                                (float)item.createTile, 0);
                            player.toolTime = (int)((float)item.useTime * player.tileSpeed / PlayerHooks.TotalUseTimeMultiplier(player, item));
                            player.itemTime = player.toolTime;
                        }
                    }
                }
            }
        }

        private void AddScaffoldCuttable()
        {
            // Demolition Hammer removes scaffolding super ez
            if (ScaffoldType > 0 && player.HeldItem.type == mod.ItemType<Items.SledgeHammer>())
            {
                Main.tileCut[ScaffoldType] = true;
                Main.tileCut[PlatformType] = true;
            }
        }
        private void RemoveScaffoldCuttable()
        {
            if (ScaffoldType > 0 && Main.tileCut[ScaffoldType])
            {
                Main.tileCut[ScaffoldType] = false;
                Main.tileCut[PlatformType] = false;
            }
        }
        
        private void ScaffoldSlopeSetup()
        {
            if (player.HeldItem.createTile >= 0 &&
                player.HeldItem.createTile != ScaffoldType && player.HeldItem.createTile != PlatformType)
            {
                storedHalfBrick = false;
                storedSlope = 0;
                storedType = -1;

                if (
                    (Main.tile[Player.tileTargetX, Player.tileTargetY].type == ScaffoldType
                    && !TileID.Sets.Platforms[player.HeldItem.createTile])
                    ||
                    (Main.tile[Player.tileTargetX, Player.tileTargetY].type == PlatformType
                    && TileID.Sets.Platforms[player.HeldItem.createTile]))
                {
                    storedType = player.HeldItem.createTile;
                    storedHalfBrick = Main.tile[Player.tileTargetX, Player.tileTargetY].halfBrick();
                    storedSlope = Main.tile[Player.tileTargetX, Player.tileTargetY].slope();
                }
            }
        }
        private void ScaffoldSlopeReplace()
        {
            if (player.HeldItem.createTile >= 0)
            {
                if (Main.tile[Player.tileTargetX, Player.tileTargetY].type == storedType)
                {
                    if (storedHalfBrick)
                    {
                        WorldGen.PoundTile(Player.tileTargetX, Player.tileTargetY);
                        if (Main.netMode == 1)
                        { NetMessage.SendData(17, -1, -1, null, 7, (float)Player.tileTargetX, (float)Player.tileTargetY, 1f); }
                    }
                    if (storedSlope > 0)
                    {
                        WorldGen.SlopeTile(Player.tileTargetX, Player.tileTargetY, storedSlope);
                        if (Main.netMode == 1)
                        { NetMessage.SendData(17, -1, -1, null, 14, (float)Player.tileTargetX, (float)Player.tileTargetY, storedSlope); }
                    }
                }
            }
        }

        #region Renovator
        private void CheckRenovatorTile()
        {
            // Renovator
            if ((player.HeldItem.createTile >= 0 || player.HeldItem.createWall >= 0) &&
                Main.tile[Player.tileTargetX, Player.tileTargetY].active() &&
                Main.tile[Player.tileTargetX, Player.tileTargetY].type == mod.TileType<Tiles.Renovator>())
            {
                if (TileTargetInRange()) // can actually place
                {
                    player.noBuilding = true; // prevent placing. unfortunately doesn't stop wall placement
                    if (player.itemTime == 0 && player.itemAnimation > 0 && player.controlUseItem)
                    {
                        UseRenovator(player.HeldItem, Main.tile[Player.tileTargetX, Player.tileTargetY].color());
                    }
                }
            }
        }

        /// <summary> Maing logic for using the renovator. </summary>
        private bool UseRenovator(Item item, byte paintColour)
        {
            // Check to consume the appropriate item
            if (!CheckUseTile(item)) return false; // Can't even use the item anyway

            Point? target = null;
            byte mode = 255;
            mode = RenovatorUseItemMode(item, mode);
            target = FindTileTarget(mode, paintColour);

            // nothing
            if (target == null) return false;

            // Place twice as fast
            float newItemTime = (player.HeldItem.useTime / PlayerHooks.TotalUseTimeMultiplier(player, player.HeldItem));
            if (mode == 1)
            { player.itemTime = (int)(newItemTime * (player.wallSpeed) * 0.5f); }
            else
            { player.itemTime = (int)(newItemTime * (player.tileSpeed) * 0.5f); }
            player.itemTime = Math.Max(1, player.itemTime);

            Main.PlaySound(7, -1, -1, 1, 1f, 0f);

            int tileX = target.Value.X;
            int tileY = target.Value.Y;
            if (item.createTile >= 0)
            {
                byte slope = Main.tile[tileX, tileY].slope();
                bool half = Main.tile[tileX, tileY].halfBrick();

                WorldGen.PlaceTile(tileX, tileY, item.createTile, false, true, player.whoAmI, item.placeStyle);
                WorldGen.paintTile(tileX, tileY, 0, true);
                if (Main.netMode == 1)
                { NetMessage.SendData(17, -1, -1, null, 1, tileX, tileY, (float)item.createTile, item.placeStyle); }

                WorldGen.SlopeTile(tileX, tileY, slope);
                if (Main.netMode == 1)
                { NetMessage.SendData(17, -1, -1, null, 14, tileX, tileY, 0f); }

                if (half)
                {
                    WorldGen.PoundTile(tileX, tileY);
                    if (Main.netMode == 1)
                    { NetMessage.SendData(17, -1, -1, null, 7, tileX, tileY, 1f); }
                }
            }
            else
            {
                Main.tile[tileX, tileY].wall = 0;
                WorldGen.PlaceWall(tileX, tileY, item.createWall, false);
                WorldGen.paintWall(tileX, tileY, 0, true);
                WallLoader.PlaceInWorld(tileX, tileY, item);
                if (Main.netMode == 1)
                {
                    NetMessage.SendData(17, -1, -1, null, 3, tileX, tileY, (float)item.createWall);
                }
            }

            // Consume the item
            ConsumeTile(item);
            return true;
        }

        internal static byte RenovatorUseItemMode(Item item, byte mode)
        {
            if (item.createTile >= 0)
            {
                if (Main.tileSolid[item.createTile] && !Main.tileSolidTop[item.createTile]) mode = 0;
                if (TileID.Sets.Platforms[item.createTile]) mode = 2;
            }
            else if (item.createWall >= 0) mode = 1;
            return mode;
        }

        /// <summary> Check if player tile range is within block range </summary>
        private bool TileTargetInRange()
        {
            return player.position.X / 16f - Player.tileRangeX - player.HeldItem.tileBoost - player.blockRange <= Player.tileTargetX &&
                (player.position.X + player.width) / 16f + Player.tileRangeX + player.HeldItem.tileBoost - 1f + player.blockRange >= Player.tileTargetX &&
                player.position.Y / 16f - Player.tileRangeY - player.HeldItem.tileBoost - player.blockRange <= Player.tileTargetY &&
                (player.position.Y + player.height) / 16f + Player.tileRangeY + player.HeldItem.tileBoost - 2f + player.blockRange >= Player.tileTargetY;
        }
        /// <summary> Search for scaffolding blocks, returns null if none found. </summary>
        private Point? FindTileTarget(byte mode, byte paintColour)
        {
            if (mode == 255) return null;

            int tileRange = 64 + player.blockRange * 8;
            int tileX, tileY;
            for (int y = 0; y < 1 + tileRange * 2; y++)  //bottom up
            {
                if (y % 2 == 0)
                { tileY = Player.tileTargetY + y / 2; }
                else
                { tileY = Player.tileTargetY + (y + 1) / -2; }
                for (int x = 0; x < 1 + tileRange * 2; x++)// L2R
                {
                    if (x % 2 == 0)
                    { tileX = Player.tileTargetX + x / 2; }
                    else
                    { tileX = Player.tileTargetX + (x + 1) / -2; }
                    if (!WorldGen.InWorld(tileX, tileY, 1)) { continue; }
                    Tile t = Main.tile[tileX, tileY];

                    // Filter by painted tiles
                    if(mode == 1)
                    {
                        if (t.wallColor() != paintColour) { continue; }
                    }
                    else
                    {
                        if (t.color() != paintColour) { continue; }
                    }
                    

                    // Filter scaffolding
                    if (mode == 0 && t.type != mod.TileType<Tiles.Scaffold>()) continue;
                    else if (mode == 1 && t.wall != mod.WallType<Tiles.ScaffoldWall>()) continue;
                    else if (mode == 2 && t.type != mod.TileType<Tiles.ScaffoldPlatform>()) continue;

                    // Otherwise yeah let's do Main.LocalPlayer
                    Vector2 source = new Point(Player.tileTargetX, Player.tileTargetY).ToWorldCoordinates();
                    Vector2 target = new Point(tileX, tileY).ToWorldCoordinates();
                    Dust d = Dust.NewDustPerfect(source, 170, (target - source) * new Vector2(0.086f, 0.08f), 0, default(Color), 1.5f);
                    d.noGravity = true;

                    // Square visual
                    for (int i = -1; i < 2; i += 2)
                    {
                        d = Dust.NewDustPerfect(new Point(tileX, tileY).ToWorldCoordinates(8 - 8 * i, 8 - 8 * i),
                            DustID.t_Martian, new Vector2(2 * i, 0), 0, default(Color), 0.6f);
                        d.noGravity = true;
                        d = Dust.NewDustPerfect(new Point(tileX, tileY).ToWorldCoordinates(8 - 8 * i, 8 - 8 * i),
                            DustID.t_Martian, new Vector2(0, 2 * i), 0, default(Color), 0.6f);
                        d.noGravity = true;
                    }

                    return new Point(tileX, tileY);
                }
            }
            return null;
        }
        /// <summary> Check if the tile placing item can be used (stacks etc.) </summary>
        private bool CheckUseTile(Item item)
        {
            if (item.createTile < 0 && item.createWall < 0) return false;
            if (item.createTile == mod.TileType<Tiles.Scaffold>() ||
                item.createTile == mod.TileType<Tiles.ScaffoldPlatform>() ||
                item.createWall == mod.WallType<Tiles.ScaffoldWall>()) return false;
            // No point adding scaffolding to scaffolding

            if (item.tileWand >= 0)
            {
                foreach (Item i in player.inventory)
                {
                    if (i.active && i.stack > 0 && i.type == item.tileWand)
                    { return true; }
                }
                return false;
            }
            else if (item.stack <= 0) return false;
            return true;
        }
        /// <summary> Consumes a unit of the item, or its ammo for tile wands </summary>
        private void ConsumeTile(Item item)
        {
            Item consumable = item;
            if (item.tileWand >= 0)
            {
                consumable = null;
                foreach (Item i in player.inventory)
                {
                    if(i.active && i.stack > 0 && i.type == item.tileWand)
                    {
                        consumable = i;
                        break;
                    }
                }
            }
            if (consumable == null) return;

            bool noConsume = false;
            if (!PlayerHooks.ConsumeAmmo(player, item, consumable)) noConsume = true;
            if (!ItemLoader.ConsumeAmmo(item, consumable, player)) noConsume = true;
            if (!noConsume)
            {
                consumable.stack--;
                if (consumable.stack <= 0)
                {
                    consumable.active = false;
                    consumable.TurnToAir();
                }
            }
        }
        #endregion

    }
}
