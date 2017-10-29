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

        public override void OnEnterWorld(Player player)
        {
            ScaffoldType = mod.GetTile<Tiles.Scaffold>().Type;
            TreeCycler.localForestCooldown = 0;
            TreeCycler.localSnowCooldown = 0;
            TreeCycler.localJungleCooldown = 0;
        }
        public override bool PreItemCheck()
        {
            if (player.whoAmI != Main.myPlayer) return true;

            // TreeCycler internal cooldown
            if (TreeCycler.localForestCooldown > 0) TreeCycler.localForestCooldown--;
            if (TreeCycler.localSnowCooldown > 0) TreeCycler.localSnowCooldown--;
            if (TreeCycler.localJungleCooldown > 0) TreeCycler.localJungleCooldown--;

            // Demolition Hammer removes scaffolding super ez
            if (ScaffoldType > 0 && player.HeldItem.type == ItemID.Rockfish)
            { Main.tileCut[ScaffoldType] = true; }
            
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

            return true;
        }
        public override void PostItemCheck()
        {
            if (player.whoAmI != Main.myPlayer) return;

            if (ScaffoldType > 0 && Main.tileCut[ScaffoldType])
            { Main.tileCut[ScaffoldType] = false; }
        }

        /// <summary> Maing logic for using the renovator. </summary>
        private bool UseRenovator(Item item, byte paintColour)
        {
            // Check to consume the appropriate item
            if (!CheckUseTile(item)) return false; // Can't even use the item anyway

            Point? target = null;
            byte mode = 255;
            if (item.createTile >= 0)
            {
                if (Main.tileSolid[item.createTile] && !Main.tileSolidTop[item.createTile]) mode = 0;
                if (TileID.Sets.Platforms[item.createTile]) mode = 2;
            }
            else if (item.createWall >= 0) mode = 1;
            target = FindTileTarget(mode, paintColour);

            // nothing
            if (target == null) return false;

            player.itemTime = (int)(player.HeldItem.useTime / PlayerHooks.TotalUseTimeMultiplier(player, player.HeldItem)) / 2; // Place twice as fast
            Main.PlaySound(7, -1, -1, 1, 1f, 0f);

            int tileX = target.Value.X;
            int tileY = target.Value.Y;
            if (item.createTile >= 0)
            {
                byte slope = Main.tile[tileX, tileY].slope();

                WorldGen.PlaceTile(tileX, tileY, item.createTile, false, true, player.whoAmI, item.placeStyle);
                WorldGen.SlopeTile(tileX, tileY, slope);
                if (Main.netMode == 1)
                {
                    NetMessage.SendData(17, -1, -1, null, 1, tileX, tileY, (float)item.createTile, item.placeStyle, 0, 0);
                    NetMessage.SendData(17, -1, -1, null, 14, tileX, tileY, 0f, 0, 0, 0);
                }
            }
            else
            {
                Main.tile[tileX,tileY].wall = 0;
                Main.tile[tileX, tileY].wallColor(0);
                WorldGen.PlaceWall(tileX, tileY, item.createWall, false);
                WallLoader.PlaceInWorld(tileX, tileY, item);
                if (Main.netMode == 1)
                {
                    NetMessage.SendData(17, -1, -1, null, 3, tileX, tileY, (float)item.createWall, 0, 0, 0);
                }
            }

            // Consume the item
            ConsumeTile(item);
            return true;
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
                    if (t.color() != paintColour) { continue; }

                    // Filter scaffolding
                    if (mode == 0 && t.type != mod.TileType<Tiles.Scaffold>()) continue;
                    else if (mode == 1 && t.wall != mod.WallType<Tiles.ScaffoldWall>()) continue;
                    else if (mode == 2 && t.type != mod.TileType<Tiles.ScaffoldPlatform>()) continue;

                    // Otherwise yeah let's do this
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
    }
}
