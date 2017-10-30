using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace BuildPlanner.Tiles
{
    public class MegaAcorn : ModTile
    {
        private static List<ushort> allowedTiles;
        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Living Sapling");
            AddMapEntry(new Color(80, 70, 40), name);
            disableSmartCursor = true;
            dustType = DustID.t_LivingWood;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.DrawYOffset = 2; //offset into ground
            TileObjectData.addTile(Type);

            allowedTiles = new List<ushort>(new ushort[] { TileID.Grass, TileID.SnowBlock, TileID.JungleGrass, TileID.HallowedGrass });
        }
        internal static bool noItemDrop = false;
        public override void KillMultiTile(int i, int j, int frameX, int frameY) { if (!noItemDrop) Item.NewItem(i * 16, j * 16, 32, 16, mod.ItemType(this.GetType().Name)); }

        public override bool CanPlace(int i, int j)
        {
            // Only placeable on certain tiles, when fully solid (not sloped)
            for (int x = i; x <= i + 1; x++)
            {
                if (!allowedTiles.Contains(Main.tile[x, j + 1].type)) return false;
                if (!WorldGen.SolidTile(x, j + 1)) return false;
            }
            return true;
        }
    }
}
