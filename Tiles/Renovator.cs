using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace BuildPlanner.Tiles
{
    public class Renovator : ModTile
    {
        public override void SetDefaults()
        {
			Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Furious Worldforge");
            AddMapEntry(new Color(0, 80, 255), name);
            animationFrameHeight = 54;
			TileID.Sets.HasOutlines[Type] = true;
            disableSmartCursor = true;

            adjTiles = new int[] { TileID.WorkBenches, TileID.AdamantiteForge, TileID.Sawmill, TileID.HeavyWorkBench };

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3); // A 3x3 tile
            TileObjectData.newTile.DrawYOffset = 2; //offset into ground
            TileObjectData.addTile(Type);
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY) { Item.NewItem(i * 16, j * 16, 48, 48, mod.ItemType<Items.Renovator>()); }
        public override void NumDust(int i, int j, bool fail, ref int num) { num = 0; }
        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (++Main.tileFrameCounter[Type] >= 15) // change every few ticks
            {
                Main.tileFrameCounter[Type] = 0;
                if (++Main.tileFrame[Type] >= 10) // 10 Frames
                {
                    Main.tileFrame[Type] = 0;
                }
            }
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (MPlayer.RenovatorUseItemMode(player.HeldItem, 255) == 255) return;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = player.HeldItem.type;

            for (int num = 0; num < 16; num++)
            {
                try
                {
                    int tileRange = 64 + player.blockRange * 8;
                    Point randomTile = new Point(i + Main.rand.Next(-tileRange, tileRange + 1), j + Main.rand.Next(-tileRange, tileRange + 1));
                    Tile t = Main.tile[randomTile.X, randomTile.Y];

                    // Check matching tile and paint colour
                    bool match = (t.type == mod.TileType<Tiles.Scaffold>() ||
                        t.type == mod.TileType<Tiles.ScaffoldPlatform>()) &&
                        Main.tile[i, j].color() == t.color();
                    if (!match)
                    {
                        match = t.wall == mod.WallType<Tiles.ScaffoldWall>() && Main.tile[i, j].color() == t.wallColor();
                    }

                    if (match)
                    {
                        Dust d = Dust.NewDustPerfect(randomTile.ToWorldCoordinates(), 170, default(Vector2), 0, default(Color), 0.5f);
                        d.noGravity = true;
                        d.fadeIn = 1f;
                    }
                }
                catch { }
            }
        }

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Main.tile[i, j];
			if (tile.frameX == 18 && (tile.frameY - 18) % 54 == 0)
			{	// Only light up in the middle
				r = 0.48f;
				g = 0.32f;
				b = 0f;
			}
		}
    }
}
