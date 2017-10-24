using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace BuildPlanner.Tiles
{
    public class Scaffold : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false; // Doesn't block lights
            Main.tileLavaDeath[Type] = true; // Burrrrn
            Main.tileBrick[Type] = true; // Merges with other "brick" tiles aka most house tiles
            TileID.Sets.BreakableWhenPlacing[Type] = true; // Can place other tiles over this
            mineResist = 0.01f; // Can be broken basically instantly
            dustType = 7; // Wood
            AddMapEntry(new Color(185, 122, 87)); // Sort of wood-ish colour
        }
    }
    public class ScaffoldPlatform : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true; // is a platform
            Main.tileBlockLight[Type] = false; // Doesn't block lights
            Main.tileSolid[Type] = true; // platforms are totally solid
            Main.tileSolidTop[Type] = true; // But only on top
            Main.tileNoAttach[Type] = true; // Doesn't "merge" with other block types
            Main.tileTable[Type] = true; // Can be used to place table things like candles
            Main.tileLavaDeath[Type] = true; // Also burrrrns
            TileID.Sets.Platforms[Type] = true;
            TileID.Sets.BreakableWhenPlacing[Type] = true; // Can place other tiles over this
            mineResist = 0.01f; // Can be broken basically instantly
            dustType = 7; // Wood
            AddMapEntry(new Color(185, 122, 87)); // Sort of wood-ish colour
            disableSmartCursor = true; // Smartcursor doesn't target platforms as a block
            adjTiles = new int[] { TileID.Platforms }; // Yes this is a platform tile thankyou

            #region Platform sprite data (From Example Mod)
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleMultiplier = 27;
            TileObjectData.newTile.StyleWrapLimit = 27;
            TileObjectData.newTile.UsesCustomCanPlace = false;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.addTile(Type);
            #endregion
        }
    }
    public class ScaffoldWall : ModWall
    {
        public override void SetDefaults()
        {
            Main.wallHouse[Type] = true; // housing wall can be destroyed anywhere
            Main.wallLight[Type] = true; // Lets all light though (as opposed to WallID.Sets.Transparent)
            AddMapEntry(new Color(110, 80, 60)); // Sort of dark wood-ish colour
        }
    }
}
