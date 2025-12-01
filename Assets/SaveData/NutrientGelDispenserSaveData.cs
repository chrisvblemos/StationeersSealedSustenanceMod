using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using System.Xml.Serialization;
using UnityEngine;

namespace Sealedsustenance.Scripts
{
    [XmlInclude(typeof(NutrientGelDispenserSaveData))]
    public class NutrientGelDispenserSaveData : StructureSaveData
    {
        [XmlElement]
        public float Storage;

        [XmlElement]
        public float Capacity;

        [XmlElement]
        public int Processing;

        [XmlElement]
        public float ProcessingRate;

        [XmlElement]
        public float TimeToRefill;
    }
}
