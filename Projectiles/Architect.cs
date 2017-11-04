using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Sprite by San#1917
/// </summary>
namespace BuildPlanner.Projectiles
{
    public class Architect : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 2;
        }
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
        public override bool? CanCutTiles() { return false; }

        public Point TargetTile
        { get { return new Point((int)projectile.ai[0], (int)projectile.ai[1]); } }
        public List<Point> SelectedTiles;
        public override void AI()
        {
            if (projectile.velocity.X != 0)
            {
                projectile.spriteDirection = Math.Sign(projectile.velocity.X);
                projectile.velocity.X = 0;
            }

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

            projectile.ai[0] = Main.MouseWorld.X / 16; projectile.ai[1] = Main.MouseWorld.Y / 16;
            Point StartTile = projectile.position.ToTileCoordinates();

            TileSelection ts = new TileSelection(StartTile, TargetTile);
            ts.SetFillOff();
            switch (UI.ArchitectUI.Settings.Mode)
            {
                case UI.ArchitectUI.Settings.ToolMode.TileLine:
                    ts.SetSelectLine();
                    break;
                case UI.ArchitectUI.Settings.ToolMode.TileSquare:
                    ts.SetSelectRuler();
                    break;
                case UI.ArchitectUI.Settings.ToolMode.TileEllipse:
                    ts.SetSelectCircle();
                    break;
                case UI.ArchitectUI.Settings.ToolMode.PlatformLine:
                    ts.SetSelectRuler();
                    break;
                case UI.ArchitectUI.Settings.ToolMode.PlatformStairs:
                    ts.SetSelectRulerDiag();
                    break;
                case UI.ArchitectUI.Settings.ToolMode.WallFill:
                    ts.SetSelectRuler();
                    ts.SetFillOn();
                    break;
            }
            SelectedTiles = ts.MakeSelection(player.direction != projectile.spriteDirection);

            if (player.channel) return;
            PlaceTiles(player, StartTile, TargetTile, SelectedTiles);
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

        private void PlaceTiles(Player player, Point StartTile, Point EndTile, List<Point> SelectedTiles)
        {

            bool sendNetMessage =
                (Main.netMode == 2 && projectile.owner == 255) ||
                (Main.netMode == 1 && projectile.owner == Main.myPlayer);

            if (UI.ArchitectUI.Settings.MineTiles)
            {
                if (UI.ArchitectUI.Settings.Mode == UI.ArchitectUI.Settings.ToolMode.WallFill)
                {
                    foreach (Point p in SelectedTiles)
                    {
                        if (Main.tile[p.X, p.Y] == null) Main.tile[p.X, p.Y] = new Tile();
                        Tile t = Main.tile[p.X, p.Y];
                        AttemptBreakWall(player, sendNetMessage, t, p.X, p.Y);

                        DropDustAtTile(p);
                    }
                }
                else
                {
                    foreach (Point p in SelectedTiles)
                    {
                        if (Main.tile[p.X, p.Y] == null) Main.tile[p.X, p.Y] = new Tile();
                        Tile t = Main.tile[p.X, p.Y];
                        AttemptBreakTile(player, sendNetMessage, t, p.X, p.Y);

                        DropDustAtTile(p);
                    }
                }
            }
            else
            {
                switch (UI.ArchitectUI.Settings.Mode)
                {
                    case UI.ArchitectUI.Settings.ToolMode.PlatformLine:
                        foreach (Point p in SelectedTiles)
                        {
                            if (Main.tile[p.X, p.Y] == null) Main.tile[p.X, p.Y] = new Tile();
                            Tile t = Main.tile[p.X, p.Y];
                            AttemptPlaceTile(player, sendNetMessage, t, p.X, p.Y,
                                Tiles.ScaffoldPlatform.ID, 0, false);
                        }
                        break;
                    case UI.ArchitectUI.Settings.ToolMode.PlatformStairs:
                        break;
                    case UI.ArchitectUI.Settings.ToolMode.WallFill:
                        foreach (Point p in SelectedTiles)
                        {
                            if (Main.tile[p.X, p.Y] == null) Main.tile[p.X, p.Y] = new Tile();
                            Tile t = Main.tile[p.X, p.Y];
                            AttemptPlaceWall(player, sendNetMessage, t, p.X, p.Y,
                                Tiles.ScaffoldWall.ID);
                        }
                        break;
                    default:
                        foreach (Point p in SelectedTiles)
                        {
                            if (Main.tile[p.X, p.Y] == null) Main.tile[p.X, p.Y] = new Tile();
                            Tile t = Main.tile[p.X, p.Y];
                            AttemptPlaceTile(player, sendNetMessage, t, p.X, p.Y,
                                Tiles.Scaffold.ID, 0, true);
                        }
                        break;
                }
            }
        }

        private void DropDustAtTile(Point p)
        {
            Dust d = Dust.NewDustPerfect(p.ToWorldCoordinates(), 175);
            d.fadeIn = 3f;
            d.velocity = default(Vector2);
            d.noGravity = true;
        }

        private static void AttemptPlaceTile(Player player, bool sendNetMessage, Tile t, int x, int y, int Type, int Style, bool replaceChestBottom = false)
        {
            bool placed = false;

            if(replaceChestBottom)
            {
                // Chests are annoying, let's fix that.
                if (Main.tile[x, y - 1] == null) Main.tile[x, y - 1] = new Tile();
                if (TileID.Sets.BasicChest[Main.tile[x, y - 1].type] && t.type != Type)
                {
                    placed = WorldGen.PlaceTile(x, y, Type, false, true, player.whoAmI, Style);
                }
                else
                {
                    placed = WorldGen.PlaceTile(x, y, Type, false, false, player.whoAmI, Style);
                }
            }
            else
            {
                placed = WorldGen.PlaceTile(x, y, Type, false, false, player.whoAmI, Style);
            }

            if (placed && sendNetMessage)
            { NetMessage.SendData(17, -1, -1, null, 1, (float)x, (float)y, (float)Type, Style, 0, 0); }
        }
        private static void AttemptPlaceWall(Player player, bool sendNetMessage, Tile t, int x, int y, int Wall)
        {
            if ((int)t.wall != Wall)
            {
                WorldGen.PlaceWall(x, y, Wall, false);
                if(sendNetMessage)
                { NetMessage.SendData(17, -1, -1, null, 3, (float)x, (float)y, (float)Wall, 0, 0, 0); }
            }
        }
        private static void AttemptBreakTile(Player player, bool sendNetMessage, Tile t, int x, int y)
        {
            int tileId = player.hitTile.HitObject(x, y, 1); // 1 for tiles, 2 for walls
            bool failed = player.hitTile.AddDamage(tileId, 200, true) <= 100;
            player.hitTile.Clear(tileId);

            if (failed)
            {
                WorldGen.KillTile(x, y, true);
                if (sendNetMessage)
                {
                    NetMessage.SendData(17, -1, -1, null, 0, (float)x, (float)y, 1f, 0, 0, 0);
                }
            }
            else
            {
                WorldGen.KillTile(x, y, false);
                if (sendNetMessage)
                {
                    NetMessage.SendData(17, -1, -1, null, 0, (float)x, (float)y, 0f, 0, 0, 0);
                }
            }
        }
        private static void AttemptBreakWall(Player player, bool sendNetMessage, Tile t, int x, int y)
        {
            int tileId = player.hitTile.HitObject(x, y, 2); // 1 for tiles, 2 for walls
            bool failed = player.hitTile.AddDamage(tileId, 200, true) <= 100;
            player.hitTile.Clear(tileId);

            if (failed)
            {
                WorldGen.KillWall(x, y, true);
                if (sendNetMessage)
                {
                    NetMessage.SendData(17, -1, -1, null, 2, (float)x, (float)y, 1f, 0, 0, 0);
                }
            }
            else
            {
                WorldGen.KillWall(x, y, false);
                if (sendNetMessage)
                {
                    NetMessage.SendData(17, -1, -1, null, 2, (float)x, (float)y, 0f, 0, 0, 0);
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (SelectedTiles == null && SelectedTiles.Count == 0) return false;
            Texture2D texture = Main.projectileTexture[projectile.type];
            Rectangle frame = texture.Frame(1, Main.projFrames[projectile.type], 0, UI.ArchitectUI.Settings.MineTiles ? 1 : 0);
            foreach (Point point in SelectedTiles)
            {
                spriteBatch.Draw(texture, point.ToWorldCoordinates() - Main.screenPosition, frame,
                    new Color(1f, 1f, 1f, 0.5f), projectile.rotation, frame.Size() / 2,
                    projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }

    public class TileSelection
    {
        private Point startTile;
        private Point endTile;
        private byte selectStyle;
        private bool selectFill;

        public TileSelection(Point startTile, Point endTile)
        {
            this.startTile = startTile;
            this.endTile = endTile;
        }
        public void SetSelectLine() { selectStyle = 0; }
        public void SetSelectRuler() { selectStyle = 1; }
        public void SetSelectCircle() { selectStyle = 2; }
        public void SetSelectRulerDiag() { selectStyle = 3; }
        public void SetFillOn() { selectFill = true; }
        public void SetFillOff() { selectFill = false; }

        public List<Point> MakeSelection(bool alternate)
        {
            List<Point> list = new List<Point>();
            float diffX = endTile.X - startTile.X;
            float diffY = endTile.Y - startTile.Y;
            if (selectStyle == 0)
            {
                LineStyle(list, diffX, diffY);
            }
            else if (selectStyle == 1)
            {
                RulerStyle(alternate, list, diffX, diffY);
            }
            else if (selectStyle == 2)
            {
                EllipseStyle(list, diffX, diffY);
            }
            else if (selectStyle == 3)
            {
                StairStyle(alternate, list, diffX, diffY);
            }
            return list;
        }

        private void StairStyle(bool alternate, List<Point> list, float diffX, float diffY)
        {
            if (alternate) { RulerStyle(false, list, diffX, diffY); return; }

            int dirX = Math.Sign(diffX);
            int dirY = Math.Sign(diffY);
            int w = (int)Math.Abs(diffX);
            int h = (int)Math.Abs(diffY);
            int shortest = h;

            Point endPoint;
            if(w >= h)
            {
                for (int i = 0; i < w - shortest; i++)
                {
                    list.Add(new Point(
                        startTile.X + i * dirX,
                        startTile.Y
                        ));
                }
                endPoint = new Point(startTile.X + (w - shortest) * dirX, startTile.Y);
            }
            else
            {
                shortest = w;
                for (int i = 0; i < h - shortest; i++)
                {
                    list.Add(new Point(
                        startTile.X,
                        startTile.Y + i * dirY
                        ));
                }
                endPoint = new Point(startTile.X, startTile.Y + (h - shortest) * dirY);
            }

            for(int i = 0; i <= shortest; i++)
            {
                list.Add(new Point(
                    endPoint.X + i * dirX,
                    endPoint.Y + i * dirY
                    ));
            }
        }

        private void EllipseStyle(List<Point> list, float diffX, float diffY)
        {
            int left = Math.Min(startTile.X, endTile.X);
            int top = Math.Min(startTile.Y, endTile.Y);
            int width = (int)Math.Abs(diffX);
            int height = (int)Math.Abs(diffY);
            int rX = width / 2;
            int rY = height / 2;
            if (rX < 1 || rY < 1)
            {
                selectFill = true;
                RulerStyle(false, list, diffX, diffY);
                return;
            }

            double angQuart = Math.PI / 2;

            int longSide = rX;
            if (rY > rX)
            { longSide = rY; }
            longSide *= 2;

            // Get the quarter arc
            int listIndex = 0;
            for (int l = 0; l <= longSide; l++)
            {
                int x = (int)(left + 0.5 + rX * Math.Cos(l * angQuart / longSide));
                int y = (int)(top + 0.5 + rY * Math.Sin(l * angQuart / longSide));
                if (listIndex < 1 || (list[listIndex - 1].X != x || list[listIndex - 1].Y != y))
                {
                    list.Add(new Point(x, y));
                    listIndex++;
                }
            }

            // Move down and mirror horizontally
            int popX = width % 2 == 0 ? 0 : 1;
            int popY = height % 2 == 0 ? 0 : 1;
            int origLen = list.Count;
            for (int i = 0; i < listIndex; i++)
            {
                int lx = left * 2 + rX - list[i].X;
                int ly = list[i].Y + rY + popY;
                list[i] = new Point(list[i].X + rX + popX, ly);
                if (popX == 0 && lx == left + rX) continue; // stop overlap where ends meet
                list.Add(new Point(lx, ly));
            }
            // Mirror vertical
            origLen = list.Count;
            for (int i = 0; i < origLen; i++)
            {
                int ly = (top + rY) * 2 - list[i].Y + popY;
                if (popY == 0 && ly == top + rY) continue; // stop overlap where ends meet
                int lx = list[i].X;
                list.Add(new Point(lx, ly));
            }
        }

        private void RulerStyle(bool alternate, List<Point> list, float diffX, float diffY)
        {
            int dirX = Math.Sign(diffX);
            int dirY = Math.Sign(diffY);
            if (selectFill)
            {
                for (int y = 0; y <= Math.Abs(diffY); y++)
                {
                    for (int x = 0; x <= Math.Abs(diffX); x++)
                    {
                        list.Add(new Point(startTile.X + x * dirX, startTile.Y + y * dirY));
                    }
                }
                return;
            }
            if (!alternate)
            {
                for (int x = 0; x <= Math.Abs(diffX); x++)
                {
                    list.Add(new Point(startTile.X + x * dirX, startTile.Y));
                }
                for (int y = 1; y <= Math.Abs(diffY); y++)
                {
                    list.Add(new Point(endTile.X, startTile.Y + y * dirY));
                }
            }
            else
            {
                for (int y = 0; y <= Math.Abs(diffY); y++)
                {
                    list.Add(new Point(startTile.X, startTile.Y + y * dirY));
                }
                for (int x = 1; x <= Math.Abs(diffX); x++)
                {
                    list.Add(new Point(startTile.X + x * dirX, endTile.Y));
                }
            }
        }

        private void LineStyle(List<Point> list, float diffX, float diffY)
        {
            float longest = Math.Max(1, Math.Max(Math.Abs(diffX), Math.Abs(diffY)));
            for (int i = 0; i <= longest; i++)
            {
                list.Add(new Point(
                   (int)(startTile.X + 0.5f + diffX * i / longest),
                   (int)(startTile.Y + 0.5f + diffY * i / longest)
                    ));
            }
        }
    }
}