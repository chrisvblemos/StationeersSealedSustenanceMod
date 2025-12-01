using Assets.Scripts.Objects.Items;
using SealedSustenance.Interfaces;

namespace SealedSustenance.Items
{
    public class NutrientGel : CannedFood, IEVAConsumable
    {
        public override void DestroyItemAtZero()
        {

        }
        
        public override void Awake()
        {
            base.Awake();

            NutritionValue = SealedSustenance.configNutrientGelNutritionValue.Value;
        }
    }
    
}
