using Assets.Scripts.Localization2;

namespace SealedSustenance.Localization
{
    public static class MyGameStrings
    {
        public static readonly GameString CantProcessNutrient = GameString.Create("CantProcessNutrient", "Can't process nutrients from this item.");
        public static readonly GameString NutrientDispenserIsFull = GameString.Create("NutrientDispenserIsFull", "Nutrient dispenser is currently full.");
        public static readonly GameString NutrientDispenserStorage = GameString.Create("NutrientDispenserStorage", "Nutrients Stored");
    }
}