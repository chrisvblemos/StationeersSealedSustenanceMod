using Assets.Scripts.Objects.Items;
using SealedSustenance.Interfaces;

namespace SealedSustenance.Items
{
    public class SealedWater : HydrationBase, IEVAConsumable
    {
        public override void Awake()
        {
            base.Awake();

            MaxQuantity = SealedSustenance.configSealedWaterMaxStorage.Value;
        }
    }
}
