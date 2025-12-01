using UnityEngine;
using LaunchPadBooster;
using System.Collections.Generic;
using HarmonyLib;
using SealedSustenance.Structures;
using LaunchPadBooster.Utils;
using ThingImport.Thumbnails;
using Assets.Scripts.AssetCreation;
using Assets.Scripts.UI;
using Assets.Scripts.Objects;
using BepInEx.Configuration;
using BepInEx;
using UnityEditor.VersionControl;
using Util;
using Sealedsustenance.Scripts;


namespace SealedSustenance
{
    [BepInPlugin("lemos.mods.sealedsustenance", "Sealed Sustenance", "1.0.0.0")]
    public class SealedSustenance : BaseUnityPlugin
    {
        public static readonly Mod MOD = new("Sealedsustenance", "1.0");

        internal static ConfigEntry<float> configNutrientGelNutritionValue;
        internal static ConfigEntry<float> configNutrientGelDispenserMaxNutrientStorage;
        internal static ConfigEntry<float> configNutrientGelDispenserProcessRate;
        internal static ConfigEntry<float> configNutrientGelDispenserRefillTime;
        internal static ConfigEntry<float> configSealedWaterMaxStorage;

        public void OnLoaded(List<GameObject> prefabs)
        {
            MOD.AddPrefabs(prefabs);
        
            configNutrientGelNutritionValue = Config.Bind("Nutrient Gel - Balancing", "NutrientGelNutritionValue", 2000.0f, "Total nutrition value of the Nutrient Gel when full (100%).");
            configSealedWaterMaxStorage = Config.Bind("Sealed Water - Balancing", "SaledWaterMaxStorage", 2.0f, "Max amount of water that the Sealed Water can store (in liters).");
            configNutrientGelDispenserMaxNutrientStorage = Config.Bind("Nutrient Gel - Balancing", "NutrientGelDispenserMaxNutrientStorage", 10000.0f, "Max amount of nutrition value that the Nutrient Gel Dispenser can store (Potato has 40 Nutrition Value, for reference, dispenser will generate 20 - half from it).");
            configNutrientGelDispenserProcessRate = Config.Bind("Nutrient Gel - Balancing", "NutrientGelDispenserProcessRate", 400.0f, "How many nutrients the Nutrient Gel Dispenser will process per second.");
            configNutrientGelDispenserRefillTime = Config.Bind("Nutrient Gel - Balancing", "NutrientGelDispenserRefillTime", 8.0f, "How long it will take for the dispenser to fully refill a Nutrient Gel (in seconds).");
            
            MOD.SetupPrefabs<SealedWaterDispenser>().SetExitTool(PrefabNames.Drill);
            MOD.SetupPrefabs<NutrientGelDispenser>().SetExitTool(PrefabNames.Drill);

            MOD.SetupPrefabs().SetBlueprintMaterials();

            Harmony harmony = new Harmony("SealedSustenance");
            harmony.PatchAll();

            MOD.AddSaveDataType<NutrientGelDispenserSaveData>();

            MOD.SetMultiplayerRequired();

#if DEVELOPMENT_BUILD
            Debug.Log($"Loaded {prefabs.Count} prefabs");
#endif

        }
    }
}
