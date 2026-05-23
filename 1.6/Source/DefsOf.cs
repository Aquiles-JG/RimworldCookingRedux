using RimWorld;
using Verse;

namespace CookingRedux
{
    [DefOf]
    public static class DefsOf
    {
        static DefsOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefsOf));
        }
        public static ThingDef MealSimple;
        public static ThingDef MealFine;
        public static ThingDef MealLavish;
        public static ThingDef MealFine_Veg;
        public static ThingDef MealLavish_Veg;
        public static ThingDef MealFine_Meat;
        public static ThingDef MealLavish_Meat;

        public static RecipeDef CookMealSimple;
        public static RecipeDef CookMealSimpleBulk;
        public static RecipeDef CookMealFine;
        public static RecipeDef CookMealFineBulk;
        public static RecipeDef CookMealLavish;
        public static RecipeDef CookMealLavishBulk;
        public static RecipeDef CookMealFine_Veg;
        public static RecipeDef CookMealFine_Meat;
        public static RecipeDef CookMealFineBulk_Veg;
        public static RecipeDef CookMealFineBulk_Meat;
        public static RecipeDef CookMealLavish_Veg;
        public static RecipeDef CookMealLavish_Meat;
        public static RecipeDef CookMealLavishBulk_Veg;
        public static RecipeDef CookMealLavishBulk_Meat;
        public static ThingDef Aq_HotPot;
        public static ThingDef Aq_Steak;
        public static ThingDef Aq_Bowlstew;
        public static ThingDef Aq_Meatloaf;
        public static ThingDef Aq_SaltedMeat;
        public static ThingDef Aq_Sausages;
        public static ThingDef Aq_CarrotPuree;
    }
}
