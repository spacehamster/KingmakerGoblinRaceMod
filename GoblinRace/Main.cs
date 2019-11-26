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
using UnityEngine;
using System.IO;
using Harmony12;
using Kingmaker.Visual.CharacterSystem;
using System.Collections.Generic;
using static Harmony12.AccessTools;

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
            }
            catch (Exception ex)
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

                }
                catch (Exception ex)
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
                    if (locale == Locale.enGB && __result != null)
                    {
                        __result.Strings["9774d914-1a01-4f52-824c-ac71d213f271"] = "Sneaky";
                    }
                }
                catch (Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }

        [Harmony12.HarmonyPatch(typeof(AssetBundle), "LoadFromFile", new Type[] { typeof(string) })]
        static class AssetBundle_LoadFromFilePatch
        {
            static FieldRef<EquipmentEntity, List<Texture2D>> m_PrimaryRampsRef;
            static void Prepare()
            {
                m_PrimaryRampsRef = AccessTools.FieldRefAccess<EquipmentEntity, List<Texture2D>>("m_PrimaryRamps");
            }
            static void Postfix(string path, ref AssetBundle __result)
            {
                try
                {
                    var assetId = Path.GetFileName(path).Replace("resource_", "");
                    if (assetId != "e4b9c88f38026d440a12ae8ea148b8f3") return;
                    var ee = __result.LoadAllAssets<EquipmentEntity>()[0];
                    m_PrimaryRampsRef(ee) = new List<Texture2D>();
                    ee.ColorsProfile = null;
                }
                catch (Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }
    }
}
