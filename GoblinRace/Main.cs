using UnityModManagerNet;
using System;
using System.Reflection;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Localization.Shared;

namespace GoblinRace
{
    public class Main
    {
        public static UnityModManagerNet.UnityModManager.ModEntry.ModLogger logger;
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLog(string msg)
        {
            if (logger != null) logger.Log(msg);
        }
        public static void DebugError(Exception ex)
        {
            if (logger != null) logger.Log(ex.ToString() + "\n" + ex.StackTrace);
        }
        public static bool enabled;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;
                var harmony = Harmony12.HarmonyInstance.Create(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            } catch(Exception ex)
            {
                DebugError(ex);
                throw ex;   
            }
            return true;
        }
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        static class LibraryScriptableObject_LoadDictionary_Patch
        {
            static void Postfix()
            {
                try
                {
                    Main.DebugLog("Adding goblins");
                    ref var races = ref Game.Instance.BlueprintRoot.Progression.CharacterRaces;
                    var goblinRace = ResourcesLibrary.TryGetBlueprint<BlueprintRace>("9d168ca7100e9314385ce66852385451");
                    if (races.Contains(goblinRace)) return;
                    var length = races.Length;
                    Array.Resize(ref races, length + 1);
                    races[length] = goblinRace;

                    var stealthy = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("610652378253d3845bb70f005c084daa"); //Stealthy
                    var statBonus = stealthy.GetComponent<AddStatBonus>();
                    statBonus.Stat = Kingmaker.EntitySystem.Stats.StatType.SkillStealth;
                    statBonus.Value = 4;

                } catch(Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }

        [Harmony12.HarmonyPatch(typeof(LocalizationManager), "LoadPack")]
        static class LocalizationManager_CurrentLocale_SetterPatch
        {
            static void Postfix(Locale locale, ref LocalizationPack __result)
            {
                try
                {
                    if(locale == Locale.enGB && __result != null)
                    {
                        __result.Strings["9774d914-1a01-4f52-824c-ac71d213f271"] = "Sneaky";
                    }
                } catch(Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }
    }
}
