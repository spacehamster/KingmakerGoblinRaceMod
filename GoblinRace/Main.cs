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
using Kingmaker.Visual.CharacterSystem;
using System.Collections.Generic;
using HarmonyLib;
using static HarmonyLib.AccessTools;

namespace GoblinRace
{
    public class Main
    {
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string msg)
        {
            if (logger != null) logger.Log(msg);
        }
        public static void Error(Exception ex)
        {
            if (logger != null) logger.Error(ex.ToString());
        }
        public static void Error(string msg)
        {
            if (logger != null) logger.Error(msg);
        }

        public static UnityModManager.ModEntry.ModLogger logger;
        public static bool enabled;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;
                var harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Error(ex);
                throw ex;
            }
            return true;
        }

        [HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        static class LibraryScriptableObject_LoadDictionary_Patch
        {
            static void Postfix()
            {
                try
                {
                    Main.Log("Adding goblins");
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
                    Main.Error(ex);
                }
            }
        }

        [HarmonyPatch(typeof(LocalizationManager), "LoadPack")]
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
                    Main.Error(ex);
                }
            }
        }

        [HarmonyPatch(typeof(AssetBundle), "LoadFromFile", new Type[] { typeof(string) })]
        static class AssetBundle_LoadFromFile_Patch
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
                    Main.Log("Fixing Goblin texture");
                    var ee = __result.LoadAllAssets<EquipmentEntity>()[0];
                    m_PrimaryRampsRef(ee) = new List<Texture2D>();
                    ee.ColorsProfile = null;
                }
                catch (Exception ex)
                {
                    Main.Error(ex);
                }
            }
        }

        [HarmonyPatch(typeof(AssetBundle), "LoadFromFileAsync", new Type[] { typeof(string) })]
        static class AssetBundle_LoadFromFileAsync_Patch
        {
            static FieldRef<EquipmentEntity, List<Texture2D>> m_PrimaryRampsRef;
            static void Prepare()
            {
                m_PrimaryRampsRef = AccessTools.FieldRefAccess<EquipmentEntity, List<Texture2D>>("m_PrimaryRamps");
            }
            static void Postfix(string path, ref AssetBundleCreateRequest __result)
            {
                try
                {
                    var assetId = Path.GetFileName(path).Replace("resource_", "");
                    if (assetId != "e4b9c88f38026d440a12ae8ea148b8f3") return;
                    Main.Log("Registering goblin texture callback");
                    __result.completed += OnAssetBundleLoad;

                }
                catch (Exception ex)
                {
                    Main.Error(ex);
                }
            }

            private static void OnAssetBundleLoad(AsyncOperation obj)
            {
                if(obj is AssetBundleCreateRequest request)
                {
                    var bundle = request.assetBundle;
                    var ee = bundle.LoadAllAssets<EquipmentEntity>()[0];
                    m_PrimaryRampsRef(ee) = new List<Texture2D>();
                    ee.ColorsProfile = null;
                }
                else
                {
                    Main.Error($"OnAssetBundleLoad Error: AsyncOperation is {obj.GetType()}");
                }
            }
        }
    }
}
