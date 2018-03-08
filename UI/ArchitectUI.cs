using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace BuildPlanner.UI
{
    public class ArchitectUI : UIState
    {
        public static class Settings
        {
            public static bool MineTiles = false;
            public enum ToolMode { TileLine, TileSquare, TileEllipse, PlatformLine, PlatformStairs, WallFill }
            public static ToolMode Mode = ToolMode.TileSquare;
        }
        #region Textures
        internal static void LoadTextures(Texture2D[] textures)
        {
            if (textures.Length != 12) return;
            TexBorderBlack = textures[0];
            TexBorderBlackOutline = textures[1];
            TexBorderRed = textures[2];
            TexBorderRedOutline = textures[3];

            TexModePlace = textures[10];
            TexModeBreak = textures[11];

            TexToolLine = textures[4];
            TexToolSquare = textures[5];
            TexToolEllipse = textures[6];
            TexPlatformLine = textures[7];
            TexPlatformStairs = textures[8];
            TexWallFill = textures[9];
        }
        public static Texture2D TexBorderBlack;
        public static Texture2D TexBorderBlackOutline;
        public static Texture2D TexBorderRed;
        public static Texture2D TexBorderRedOutline;
        public static Texture2D TexModePlace;
        public static Texture2D TexModeBreak;
        public static Texture2D TexToolLine;
        public static Texture2D TexToolSquare;
        public static Texture2D TexToolEllipse;
        public static Texture2D TexPlatformLine;
        public static Texture2D TexPlatformStairs;
        public static Texture2D TexWallFill;
        #endregion

        private bool visible = false;
        private Vector2 MouseNoZoom
        {
            get
            {
                return new Vector2(
                    (Main.mouseX - Main.screenWidth / 2f) * Main.GameViewMatrix.Zoom.X + Main.screenWidth / 2f,
                    (Main.mouseY - Main.screenHeight / 2f) * Main.GameViewMatrix.Zoom.Y + Main.screenHeight / 2f);
            }
        }
        public void SetVisibility(bool set)
        {
            visible = set;
            if(set)
            {
                Anchor.Left.Set(MouseNoZoom.X, 0f);
                Anchor.Top.Set(MouseNoZoom.Y, 0f);
                Recalculate();
            }
        }
        public void ToggleVisibility()
        { SetVisibility(!visible); }

        private UIElement Anchor;
        private UICircleOption BorderCentre, BorderTL, BorderTR, BorderML, BorderMR, BorderBL, BorderBR;
        private UICircleOption[] BorderArray = new UICircleOption[7];
        public override void OnInitialize()
        {
            Anchor = new UIElement();

            BorderCentre = new UICircleOption(TexBorderBlack, TexModePlace);
            BorderML = new UICircleOption(TexBorderBlack, TexToolLine);
            BorderTL = new UICircleOption(TexBorderBlack, TexToolSquare);
            BorderTR = new UICircleOption(TexBorderBlack, TexToolEllipse);
            BorderBL = new UICircleOption(TexBorderBlack, TexPlatformLine);
            BorderBR = new UICircleOption(TexBorderBlack, TexPlatformStairs);
            BorderMR = new UICircleOption(TexBorderBlack, TexWallFill);
            BorderArray[0] = BorderCentre;
            BorderArray[1] = BorderML; BorderArray[2] = BorderTL; BorderArray[3] = BorderTR;
            BorderArray[4] = BorderBL; BorderArray[5] = BorderBR; BorderArray[6] = BorderMR;

            BorderCentre.Centre = new Vector2(0, 0);
            BorderTL.Centre = new Vector2(-23, -40); BorderTR.Centre = new Vector2(23, -40);
            BorderML.Centre = new Vector2(-46, 0); BorderMR.Centre = new Vector2(46, 0);
            BorderBL.Centre = new Vector2(-23, 40); BorderBR.Centre = new Vector2(23, 40);

            BorderCentre.OnMouseDown += PlaceMineToggle;
            BorderML.OnMouseDown += SetToolLine;
            BorderTL.OnMouseDown += SetToolSquare;
            BorderTR.OnMouseDown += SetToolEllipse;
            BorderBL.OnMouseDown += SetPlatformLine;
            BorderBR.OnMouseDown += SetPlatformStairs;
            BorderMR.OnMouseDown += SetWallFill;

            foreach (UICircleOption border in BorderArray)
            { Anchor.Append(border); }
            base.Append(Anchor);
        }

        private void PlaceMineToggle(UIMouseEvent evt, UIElement listeningElement)
        {
            Settings.MineTiles = !Settings.MineTiles;
            if (Settings.MineTiles)
            { BorderCentre.SetOverlay(TexModeBreak); }
            else
            { BorderCentre.SetOverlay(TexModePlace); }
        }

        private void SetToolLine(UIMouseEvent evt, UIElement listeningElement)
        { Settings.Mode = Settings.ToolMode.TileLine; }

        private void SetToolSquare(UIMouseEvent evt, UIElement listeningElement)
        { Settings.Mode = Settings.ToolMode.TileSquare; }

        private void SetToolEllipse(UIMouseEvent evt, UIElement listeningElement)
        { Settings.Mode = Settings.ToolMode.TileEllipse; }

        private void SetPlatformLine(UIMouseEvent evt, UIElement listeningElement)
        { Settings.Mode = Settings.ToolMode.PlatformLine; }

        private void SetPlatformStairs(UIMouseEvent evt, UIElement listeningElement)
        { Settings.Mode = Settings.ToolMode.PlatformStairs; }

        private void SetWallFill(UIMouseEvent evt, UIElement listeningElement)
        { Settings.Mode = Settings.ToolMode.WallFill; }

        private void SetBaseFrames(Texture2D texture)
        {
            foreach (UICircleOption border in BorderArray)
            { border.SetImage(texture); }
        }
        public override void Update(GameTime gameTime)
        {
            if (Main.ingameOptionsWindow ||
                Main.InGameUI.IsVisible ||
                Main.LocalPlayer.showItemIcon ||
                Main.LocalPlayer.mouseInterface || Main.LocalPlayer.lastMouseInterface ||
                Main.LocalPlayer.controlUseItem ||
                (Main.LocalPlayer.HeldItem.type != Items.Architect.ID) ||
                !visible
                )
            { visible = false; }
            else
            {
                Texture2D baseBorder = TexBorderBlack;
                Texture2D outline = TexBorderBlackOutline;
                if (Settings.MineTiles)
                {
                    baseBorder = TexBorderRed;
                    outline = TexBorderRedOutline;
                }
                foreach (UICircleOption border in BorderArray)
                { border.SetImage(baseBorder); }

                foreach (UICircleOption border in BorderArray)
                {
                    border.ImageColour = Color.Gray;
                    if (Vector2.Distance(MouseNoZoom, border.Centre +
                        new Vector2(Anchor.Left.Pixels, Anchor.Top.Pixels)) < 19)
                    {
                        UIMouseEvent evt = new UIMouseEvent(this, MouseNoZoom);
                        Main.blockMouse = true;
                        border.SetImage(outline);
                        if (Main.mouseLeft && Main.mouseLeftRelease)
                        { border.MouseDown(evt); }
                        else if (!Main.mouseLeft && !Main.mouseLeftRelease)
                        { border.MouseUp(evt); }
                    }
                }
                BorderArray[0].ImageColour = Color.White;
                if (Settings.Mode == Settings.ToolMode.TileLine) { BorderArray[1].ImageColour = Color.White; }
                if (Settings.Mode == Settings.ToolMode.TileSquare) { BorderArray[2].ImageColour = Color.White; }
                if (Settings.Mode == Settings.ToolMode.TileEllipse) { BorderArray[3].ImageColour = Color.White; }
                if (Settings.Mode == Settings.ToolMode.PlatformLine) { BorderArray[4].ImageColour = Color.White; }
                if (Settings.Mode == Settings.ToolMode.PlatformStairs) { BorderArray[5].ImageColour = Color.White; }
                if (Settings.Mode == Settings.ToolMode.WallFill) { BorderArray[6].ImageColour = Color.White; }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!visible) return;

            base.Draw(spriteBatch);
        }


    }

    public class UICircleOption : UIElement
    {
        protected Texture2D _texture;
        protected Texture2D _textureOverlay;

        public float ImageScale = 1f;
        public Color ImageColour = new Color();
        public Vector2 Centre
        {
            get { return new Vector2(Left.Pixels + Width.Pixels / 2f, Top.Pixels + Width.Pixels / 2f); }
            set { Left.Set(value.X - Width.Pixels / 2f, 0f); Top.Set(value.Y - Height.Pixels / 2f, 0f); }
        }

        public UICircleOption(Texture2D texture, Texture2D overlay = null)
        {
            this._texture = texture;
            this._textureOverlay = overlay;
            this.Width.Set((float)this._texture.Width, 0f);
            this.Height.Set((float)this._texture.Height, 0f);
        }

        public void SetImage(Texture2D texture)
        {
            this._texture = texture;
            this.Width.Set((float)this._texture.Width, 0f);
            this.Height.Set((float)this._texture.Height, 0f);
        }
        public void SetOverlay(Texture2D texture)
        {
            this._textureOverlay = texture;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = base.GetDimensions();
            spriteBatch.Draw(this._texture, dimensions.Position() + this._texture.Size() * (1f - this.ImageScale) / 2f, null, ImageColour, 0f, Vector2.Zero, this.ImageScale, SpriteEffects.None, 0f);
            if (_textureOverlay != null)
            {
                spriteBatch.Draw(this._textureOverlay, dimensions.Position() + (this._texture.Size() - this._textureOverlay.Size()) / 2 + this._textureOverlay.Size() * (1f - this.ImageScale) / 2f, null, ImageColour, 0f, Vector2.Zero, this.ImageScale, SpriteEffects.None, 0f);
            }
        }
    }
}
