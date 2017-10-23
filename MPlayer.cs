using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner
{
    public class MPlayer : ModPlayer
    {
        public static int ScaffoldType = -1;

        public override void OnEnterWorld(Player player)
        {
            ScaffoldType = mod.GetTile<Tiles.Scaffold>().Type;
        }
        public override bool PreItemCheck()
        {
            // Demolition Hammer removes scaffolding super ez
            if (ScaffoldType > 0 && player.HeldItem.type == ItemID.Rockfish)
            { Main.tileCut[ScaffoldType] = true; }
            return true;
        }
        public override void PostItemCheck()
        {
            if(ScaffoldType > 0 && Main.tileCut[ScaffoldType])
            { Main.tileCut[ScaffoldType] = false; }
        }
    }
}
