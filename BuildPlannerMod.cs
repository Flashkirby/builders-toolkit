using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace BuildPlanner
{
	class BuildPlanner : Mod
	{
		public BuildPlanner()
		{
            Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}
        public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
        {
            if (messageType == 17)
            {
                try
                {
                    //NetMessage.SendData(17, -1, -1, null, 3, tileX, tileY, (float)item.createWall, 0, 0, 0);
                    byte mode = reader.ReadByte();
                    short tileX = reader.ReadInt16();
                    short tileY = reader.ReadInt16();
                    short createWall = reader.ReadInt16();
                    if (mode == 3)
                    {
                        // Soft remove walls when trying to place walls
                        Main.tile[tileX, tileY].wall = 0;
                        Main.tile[tileX, tileY].wallColor(0);
                    }
                }
                catch { }
            }
            return false;
        }
    }
}
