﻿using BepInEx;
using UnboundLib;
using UnboundLib.Cards;
using HarmonyLib;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using MoodMods.Cards;
 
namespace MoodMods
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class MoodMods : BaseUnityPlugin
    {
        private const string ModId = "com.aww.rounds.MoodMods";
        private const string ModName = "MoodMods";
        public const string Version = "1.0.0"; // What version are we on (major.minor.patch)?
 
        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            //CustomCard.BuildCard<myCard>();
            CustomCard.BuildCard<TheCloudOfHorror>();


        }
    }
}
