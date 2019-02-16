using UnityModManagerNet;
using System;
using System.Reflection;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker;
using Kingmaker.Blueprints.Classes;

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

                } catch(Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }
    }
}
