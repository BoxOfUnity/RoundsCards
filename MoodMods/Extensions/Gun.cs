using System;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace MoodMods.Extensions
{
    // ADD FIELDS TO GUN
    [Serializable]
    public class GunAdditionalData
    {
        

        public GunAdditionalData()
        {
            
        }
    }
    public static class GunExtension
    {
        public static readonly ConditionalWeakTable<Gun, GunAdditionalData> data =
            new ConditionalWeakTable<Gun, GunAdditionalData>();

        public static GunAdditionalData GetAdditionalData(this Gun gun)
        {
            return data.GetOrCreateValue(gun);
        }

        public static void AddData(this Gun gun, GunAdditionalData value)
        {
            try
            {
                data.Add(gun, value);
            }
            catch (Exception) { }
        }
    }
    // reset extra gun attributes when resetstats is called
    [HarmonyPatch(typeof(Gun), "ResetStats")]
    class GunPatchResetStats
    {
        private static void Prefix(Gun __instance)
        {
            //__instance.GetAdditionalData().shockBlastBaseForce = 0;
            
        }
    }
}