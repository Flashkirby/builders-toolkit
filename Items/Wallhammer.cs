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
            Tooltip.SetDefault("'Renovation made easy!'");
        }
        public override void SetDefaults()
        {
            item.rare = 10;
            item.mana = 3;
            item.UseSound = SoundID.Item67;
            item.noMelee = true;
            item.useStyle = 5;
            item.damage = 35;
            item.useAnimation = 25;
            item.useTime = 25;
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
            r.AddIngredient(ItemID.LaserDrill);
            r.AddIngredient(ItemID.LunarBar, 6);
            r.SetResult(item.type);
            r.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, 0f, item.hammer);
            return false;
        }
    }
}
