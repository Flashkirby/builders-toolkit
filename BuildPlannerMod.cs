using System.IO;

using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using BuildPlanner.UI;
using System.Collections.Generic;
using Terraria.GameInput;
using Microsoft.Xna.Framework;

namespace BuildPlanner
{
    public class BuildPlanner : Mod
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
        
        private UserInterface architectUserInterface;
        internal static ArchitectUI architectUI;
        public override void Load()
        {
            Items.Architect.ID = ItemType<Items.Architect>();
            Tiles.Scaffold.ID = TileType<Tiles.Scaffold>();
            Tiles.ScaffoldPlatform.ID = TileType<Tiles.ScaffoldPlatform>();
            Tiles.ScaffoldWall.ID = WallType<Tiles.ScaffoldWall>();

            if (Main.netMode != 2)
            {
                ArchitectUI.LoadTextures(new Texture2D[] {
                    Main.wireUITexture[0],
                    Main.wireUITexture[1],
                    Main.wireUITexture[8],
                    Main.wireUITexture[9],
                    GetTexture("UI/Building_0"),
                    GetTexture("UI/Building_1"),
                    GetTexture("UI/Building_2"),
                    GetTexture("UI/Building_3"),
                    GetTexture("UI/Building_4"),
                    GetTexture("UI/Building_5"),
                    GetTexture("UI/Building_6"),
                    GetTexture("UI/Building_7"),
                    });
                architectUI = new ArchitectUI();
                architectUI.Activate();
                architectUserInterface = new UserInterface();
                architectUserInterface.SetState(architectUI);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            //All this stuff is jankyily adapted from ExampleMod
            //This is getting the wire select layer, and adding the UI just underneath it
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Wire Selection"));
            if (MouseTextIndex != -1)
            {
                layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
                    "BuilderPlanner: ArchitectUILayer",
                    delegate
                    {
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateScale(Main.UIScale, Main.UIScale, 1f));
                        architectUserInterface.Update(Main._drawInterfaceGameTime);
                        architectUI.Draw(Main.spriteBatch);
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);
                        return true;
                    })
                );
            }
        }
    }
}
