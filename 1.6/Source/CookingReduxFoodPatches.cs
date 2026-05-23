using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;
using System;

namespace CookingRedux
{
    public static class CookingReduxFoodPatches
    {
        private static bool initialized = false;
        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }
            try
            {
                RedirectPawnKindFoodDefs();
                RedirectScenarioStartingMeals();
                RemoveVanillaDefs();
                RedirectThingDefOfReferences();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to remove vanilla food: " + ex.ToString());
            }
            initialized = true;
        }
        private static void RedirectThingDefOfReferences()
        {
            ThingDefOf.MealSimple = DefsOf.Aq_HotPot;
            ThingDefOf.MealFine = DefsOf.Aq_Steak;
        }
        private static void RedirectPawnKindFoodDefs()
        {
            foreach (var pawnKind in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (pawnKind.invFoodDef == null)
                {
                    continue;
                }
                if (pawnKind.invFoodDef == DefsOf.MealSimple ||
                    pawnKind.invFoodDef == DefsOf.MealFine ||
                    pawnKind.invFoodDef == DefsOf.MealFine_Veg ||
                    pawnKind.invFoodDef == DefsOf.MealFine_Meat)
                {
                    pawnKind.invFoodDef = DefsOf.Aq_HotPot;
                }
                else if (pawnKind.invFoodDef == DefsOf.MealLavish ||
                         pawnKind.invFoodDef == DefsOf.MealLavish_Veg ||
                         pawnKind.invFoodDef == DefsOf.MealLavish_Meat)
                {
                    pawnKind.invFoodDef = DefsOf.Aq_Bowlstew;
                }
            }
        }

        private static void RedirectScenarioStartingMeals()
        {
            foreach (var scenarioDef in DefDatabase<ScenarioDef>.AllDefs)
            {
                if (scenarioDef.scenario?.AllParts == null)
                {
                    continue;
                }
                foreach (var part in scenarioDef.scenario.AllParts)
                {
                    if (part is ScenPart_ThingCount thingCount)
                    {
                        if (thingCount.thingDef == DefsOf.MealSimple ||
                            thingCount.thingDef == DefsOf.MealFine ||
                            thingCount.thingDef == DefsOf.MealFine_Veg ||
                            thingCount.thingDef == DefsOf.MealFine_Meat)
                        {
                            thingCount.thingDef = DefsOf.Aq_HotPot;
                        }
                        else if (thingCount.thingDef == DefsOf.MealLavish ||
                                 thingCount.thingDef == DefsOf.MealLavish_Veg ||
                                 thingCount.thingDef == DefsOf.MealLavish_Meat)
                        {
                            thingCount.thingDef = DefsOf.Aq_Bowlstew;
                        }
                    }
                }
            }
        }

        private static void RemoveVanillaDefs()
        {
            var thingsToRemove = new List<ThingDef>
            {
                DefsOf.MealSimple,
                DefsOf.MealFine,
                DefsOf.MealLavish,
                DefsOf.MealFine_Veg,
                DefsOf.MealLavish_Veg,
                DefsOf.MealFine_Meat,
                DefsOf.MealLavish_Meat
            };

            foreach (var thing in thingsToRemove)
            {
                if (thing != null)
                {
                    RemoveThingDef(thing);
                }
            }

            var recipesToRemove = new List<RecipeDef>
            {
                DefsOf.CookMealSimple,
                DefsOf.CookMealSimpleBulk,
                DefsOf.CookMealFine,
                DefsOf.CookMealFineBulk,
                DefsOf.CookMealLavish,
                DefsOf.CookMealLavishBulk,
                DefsOf.CookMealFine_Veg,
                DefsOf.CookMealFine_Meat,
                DefsOf.CookMealFineBulk_Veg,
                DefsOf.CookMealFineBulk_Meat,
                DefsOf.CookMealLavish_Veg,
                DefsOf.CookMealLavish_Meat,
                DefsOf.CookMealLavishBulk_Veg,
                DefsOf.CookMealLavishBulk_Meat,
            };

            foreach (var recipe in recipesToRemove)
            {
                if (recipe != null)
                {
                    RemoveRecipeDef(recipe);
                }
            }

            RemoveRecipesFromBenches(recipesToRemove);
        }

        private static void RemoveThingDef(ThingDef def)
        {
            def.destroyOnDrop = true;
            def.generateCommonality = 0f;
            def.generateAllowChance = 0f;
            def.recipeMaker = null;
            def.scatterableOnMapGen = false;
            def.tradeability = Tradeability.None;
            def.tradeTags?.Clear();
            DefDatabase<ThingDef>.Remove(def);
            DefDatabase<ThingDef>.defsByShortHash.Remove(def.shortHash);
        }

        private static void RemoveRecipeDef(RecipeDef def)
        {
            def.factionPrerequisiteTags = new List<string> { "BANNEDFROMGAME" };
            DefDatabase<RecipeDef>.Remove(def);
            DefDatabase<RecipeDef>.defsByShortHash.Remove(def.shortHash);
        }

        private static void RemoveRecipesFromBenches(List<RecipeDef> recipesToRemove)
        {
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef.recipes != null)
                {
                    thingDef.recipes.RemoveAll(r => recipesToRemove.Contains(r));
                }
            }
        }

        [HarmonyPatch(typeof(StaticConstructorOnStartupUtility), nameof(StaticConstructorOnStartupUtility.CallAll))]
        public static class StaticConstructorOnStartupUtility_CallAll_Patch
        {
            public static void Prefix()
            {
                Initialize();
            }
        }
    }

    [HarmonyPatch(typeof(PawnInventoryGenerator), nameof(PawnInventoryGenerator.GiveRandomFood))]
    public static class PawnInventoryGenerator_GiveRandomFood_Patch
    {
        public static bool Prefix(Pawn p)
        {
            if (p.kindDef.invNutrition > 0.001f)
            {
                ThingDef def = p.kindDef.invFoodDef;
                if (def == null)
                {
                    var value = Rand.Value;
                    if (value < 0.5f)
                    {
                        def = DefsOf.Aq_HotPot;
                    }
                    else if (value < 0.75f)
                    {
                        def = ThingDefOf.MealSurvivalPack;
                    }
                    else
                    {
                        def = DefsOf.Aq_Meatloaf;
                    }
                }
                var thing = ThingMaker.MakeThing(def);
                thing.stackCount = GenMath.RoundRandom(p.kindDef.invNutrition / thing.GetStatValue(StatDefOf.Nutrition));
                p.inventory.TryAddItemNotForSale(thing);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ThingMaker), "MakeThing")]
    public static class ThingMaker_MakeThing_Patch
    {
        public static bool Prefix(ref ThingDef def)
        {
            if (def != null)
            {
                if (def == DefsOf.MealSimple ||
                    def == DefsOf.MealFine ||
                    def == DefsOf.MealFine_Veg ||
                    def == DefsOf.MealFine_Meat)
                {
                    def = DefsOf.Aq_HotPot;
                }
                else if (def == DefsOf.MealLavish ||
                         def == DefsOf.MealLavish_Veg ||
                         def == DefsOf.MealLavish_Meat)
                {
                    def = DefsOf.Aq_Bowlstew;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ThingDef), "Named")]
    public static class ThingDef_Named_Patch
    {
        public static bool Prefix(ref string defName)
        {
            if (defName != null)
            {
                if (defName == DefsOf.MealSimple.defName ||
                    defName == DefsOf.MealFine.defName ||
                    defName == DefsOf.MealFine_Veg.defName ||
                    defName == DefsOf.MealFine_Meat.defName)
                {
                    defName = DefsOf.Aq_HotPot.defName;
                }
                else if (defName == DefsOf.MealLavish.defName ||
                         defName == DefsOf.MealLavish_Veg.defName ||
                         defName == DefsOf.MealLavish_Meat.defName)
                {
                    defName = DefsOf.Aq_Bowlstew.defName;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
    public static class BackCompatibility_BackCompatibleDefName_Patch
    {
        public static void Postfix(System.Type defType, string defName, ref string __result)
        {
            if (defType == typeof(ThingDef))
            {
                if (defName == "MealSimple" || defName == "MealFine" || defName == "MealFine_Veg" || defName == "MealFine_Meat")
                {
                    __result = "Aq_HotPot";
                }
                else if (defName == "MealLavish" || defName == "MealLavish_Veg" || defName == "MealLavish_Meat")
                {
                    __result = "Aq_Bowlstew";
                }
            }
        }
    }
}
