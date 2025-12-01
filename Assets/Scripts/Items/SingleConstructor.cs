using UnityEngine;
using StationeersObjects = Assets.Scripts.Objects;


namespace SealedSustenance.Items
{
#if UNITY_EDITOR
    [AddComponentMenu("Stationeers/Items/SingleConstructor")]
#endif
    public class SingleConstructor : StationeersObjects.Constructor
    {
    }
}