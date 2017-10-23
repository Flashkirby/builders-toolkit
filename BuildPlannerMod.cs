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
    }
}
