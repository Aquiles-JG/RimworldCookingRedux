using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace CookingRedux
{
    public class PlaceWorker_OnOceanWater : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 c in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size))
            {
                var terrain = map.terrainGrid.TerrainAt(c);
                if (terrain != TerrainDefOf.WaterOceanDeep && terrain != TerrainDefOf.WaterOceanShallow)
                {
                    return new AcceptanceReport("Aq_NeedsOceanWater".Translate());
                }
            }



            return true;
        }






    }
    internal class Thought_Hediff : Thought_Memory
    {
        public bool added = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref added, "added", defaultValue: false);
        }

        public override float MoodOffset()
        {
            if (!added)
            {
                if (def.hediff != null)
                {
                    pawn.health.AddHediff(def.hediff);
                }
                added = true;
            }
            return base.MoodOffset();
        }
    }

    public class IngredientValueGetter_NutritionWithExtraIngredient : IngredientValueGetter_Nutrition
    {
        public override float ValuePerUnitOf(ThingDef t)
        {
            if (!t.IsNutritionGivingIngestible) return 1f;

            if (t.ingredient != null && t.ingredient.mergeCompatibilityTags.Contains("Condiments")) return 1f;
            return t.GetStatValueAbstract(StatDefOf.Nutrition);
        }
    }
}
