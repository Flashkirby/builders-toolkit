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
        public List<Point> Tiles;
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
            Tiles = ts.MakeSelection(player.direction != projectile.spriteDirection);

            if (player.channel) return;
            PlaceTiles(StartTile, TargetTile, Tiles);
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

        private void PlaceTiles(Point StartTile, Point EndTile, List<Point> Tiles)
        {

            Dust d = Dust.NewDustPerfect(StartTile.ToWorldCoordinates(), DustID.FlameBurst);
            d.fadeIn = 2f;
            d.velocity = default(Vector2);
            d.noGravity = true;
            d = Dust.NewDustPerfect(TargetTile.ToWorldCoordinates(), DustID.FlameBurst);
            d.fadeIn = 2f;
            d.velocity = default(Vector2);
            d.noGravity = true;

        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Tiles == null && Tiles.Count == 0) return false;
            Texture2D texture = Main.projectileTexture[projectile.type];

            foreach (Point point in Tiles)
            {
                spriteBatch.Draw(texture, point.ToWorldCoordinates() - Main.screenPosition,
                    new Rectangle(0, 0, texture.Width, texture.Height),
                    new Color(0.5f, 0.5f, 0.5f, 1f), projectile.rotation, texture.Size() / 2,
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