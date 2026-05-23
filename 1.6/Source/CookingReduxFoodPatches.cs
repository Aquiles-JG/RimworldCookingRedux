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

        public static ThingDef GetReplacementMeal(ThingDef original)
        {
            if (original == DefsOf.MealSimple)
            {
                return DefsOf.Aq_HotPot;
            }
            if (original == DefsOf.MealFine || original == DefsOf.MealFine_Meat)
            {
                return DefsOf.Aq_Meatloaf;
            }
            if (original == DefsOf.MealFine_Veg)
            {
                return DefsOf.Aq_CarrotPuree;
            }
            if (original == DefsOf.MealLavish || original == DefsOf.MealLavish_Meat)
            {
                return DefsOf.Aq_Bowlstew;
            }
            if (original == DefsOf.MealLavish_Veg)
            {
                return DefsOf.Aq_CarrotPuree;
            }
            return original;
        }

        public static string GetReplacementMealName(string original)
        {
            if (original == "MealSimple")
            {
                return "Aq_HotPot";
            }
            if (original == "MealFine" || original == "MealFine_Meat")
            {
                return "Aq_Meatloaf";
            }
            if (original == "MealFine_Veg")
            {
                return "Aq_CarrotPuree";
            }
            if (original == "MealLavish" || original == "MealLavish_Meat")
            {
                return "Aq_Bowlstew";
            }
            if (original == "MealLavish_Veg")
            {
                return "Aq_CarrotPuree";
            }
            return original;
        }

        private static void RedirectThingDefOfReferences()
        {
            ThingDefOf.MealSimple = DefsOf.Aq_HotPot;
            ThingDefOf.MealFine = DefsOf.Aq_Meatloaf;
        }
        private static void RedirectPawnKindFoodDefs()
        {
            foreach (var pawnKind in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (pawnKind.invFoodDef != null)
                {
                    pawnKind.invFoodDef = CookingReduxFoodPatches.GetReplacementMeal(pawnKind.invFoodDef);
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
                        thingCount.thingDef = CookingReduxFoodPatches.GetReplacementMeal(thingCount.thingDef);
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
                if (def != null)
                {
                    def = CookingReduxFoodPatches.GetReplacementMeal(def);
                }
                else
                {
                    var value = Rand.Value;
                    if (value < 0.5f)
                    {
                        def = DefsOf.Aq_SaltedMeat;
                    }
                    else if (value < 0.75f)
                    {
                        def = DefsOf.Aq_Sausages;
                    }
                    else
                    {
                        def = ThingDefOf.MealSurvivalPack;
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
                def = CookingReduxFoodPatches.GetReplacementMeal(def);
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
                defName = CookingReduxFoodPatches.GetReplacementMealName(defName);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
    public static class BackCompatibility_BackCompatibleDefName_Patch
    {
        public static void Postfix(Type defType, string defName, ref string __result)
        {
            if (defType == typeof(ThingDef))
            {
                __result = CookingReduxFoodPatches.GetReplacementMealName(defName);
            }
        }
    }
}
