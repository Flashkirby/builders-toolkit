using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using BuildPlanner.Items;

namespace BuildPlanner
{
    public class MNpc : GlobalNPC
    {
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if(type == NPCID.Demolitionist)
            { shop.item[nextSlot].SetDefaults(mod.ItemType<SledgeHammer>()); nextSlot++; }

            if (type == NPCID.Dryad)
            {
                shop.item[nextSlot].SetDefaults(mod.ItemType<WateringCan>()); nextSlot++;
                if(NPC.downedPlantBoss)
                { shop.item[nextSlot].SetDefaults(mod.ItemType<TreeCycler>()); nextSlot++; }
            }

            if (type == NPCID.GoblinTinkerer)
            { shop.item[nextSlot].SetDefaults(mod.ItemType<TileWand>()); nextSlot++; }

            if (type == NPCID.Steampunker)
            { shop.item[nextSlot].SetDefaults(mod.ItemType<AutoSloper>()); nextSlot++; }
        }
    }
}
