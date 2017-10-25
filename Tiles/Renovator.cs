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
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Furious Worldforge");
            AddMapEntry(new Color(0, 80, 255), name);
            animationFrameHeight = 54;
            disableSmartCursor = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3); // A 3x3 tile
            TileObjectData.newTile.DrawYOffset = 2; //offset into ground
            TileObjectData.addTile(Type);
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY) { Item.NewItem(i * 16, j * 16, 48, 48, mod.ItemType<Items.Renovator>()); }
        public override void NumDust(int i, int j, bool fail, ref int num) { num = 0; }
        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (++Main.tileFrameCounter[Type] >= 8) // change every 8 ticks
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
            if (player.HeldItem.createTile < 0 && player.HeldItem.createWall < 0) return;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = player.HeldItem.type;
        }
    }
}
