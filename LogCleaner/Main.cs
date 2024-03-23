using BepInEx;
using HarmonyLib;
using System;
using System.Linq;

namespace LogCleaner
{

    [BepInPlugin("crafterbot.suppresslogs", "LogCleaner", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        private static Main instance;

        private string[] typesToSuppress;

        public void Awake()
        {
            instance = this;

            var targetTypesEntry = Config.Bind("General", "Types to Suppress", "RigContainer", "Format: namespace.typename,namespace.typename,namespace.typename");
            typesToSuppress = targetTypesEntry.Value.Trim().Split(',', ' ');

            Logger.LogInfo($"Found {typesToSuppress.Length} types(s), applying patches");

            if (UnityEngine.Debug.unityLogger.logHandler is UnityEngine.DebugLogHandler)
            {
                Harmony harmony = new Harmony(Info.Metadata.GUID);
                harmony.Patch(
                    original: typeof(UnityEngine.DebugLogHandler).GetMethod("Internal_Log", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic),
                    prefix: new(GetType(), nameof(LogPrefix)));
            }
            else
            {
                Logger.LogFatal("Unkown log handler " + UnityEngine.Debug.unityLogger.logHandler.GetType().FullName);
            }
        }

        private static bool LogPrefix() =>
            !instance.typesToSuppress.Any(Environment.StackTrace.Contains);
    }
}