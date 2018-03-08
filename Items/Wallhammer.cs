using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuildPlanner.Items
{
    public class Wallhammer : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Wonderhammer");
            Tooltip.SetDefault("<right> to fire a wall smashing beam\n'Innovating renovation!'");
        }
        public override void SetDefaults()
        {
            item.rare = 5;
            item.mana = 3;
            item.UseSound = SoundID.Item67;
            item.noMelee = true;
            item.useStyle = 5;
            item.damage = 30;
            item.autoReuse = true;
            item.useAnimation = 25;
            item.useTime = 24;
            item.width = 24;
            item.height = 28;
            item.shoot = mod.ProjectileType<Projectiles.Wallbeam>();
            item.scale = 1f;
            item.shootSpeed = 10f;
            item.knockBack = 0f;
            item.magic = true;
            item.hammer = 65;
            item.value = Item.sellPrice(0, 0, 80, 0);
        }
        public override void AddRecipes()
        {
            ModRecipe r = new ModRecipe(mod);
            r.AddIngredient(ItemID.HallowedBar, 12);
            r.AddIngredient(ItemID.SoulofFright, 10);
            r.AddIngredient(ItemID.SoulofMight, 10);
            r.AddTile(TileID.MythrilAnvil);
            r.SetResult(item.type);
            r.AddRecipe();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
			int dmg = damage;
			int hammer = 0;
			if (player.altFunctionUse > 0)
			{
				hammer = item.hammer;
			}
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, dmg, knockBack, player.whoAmI, 0f, hammer);
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, 0);
        }
    }
}
