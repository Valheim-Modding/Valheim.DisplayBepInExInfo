using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Valheim.DisplayBepInExInfo
{
    [BepInPlugin("org.bepinex.valheim.displayinfo", "Display BepInEx Info In-Game", "1.0.1")]
    public class DisplayInfoPlugin : BaseUnityPlugin
    {
        internal static ConfigEntry<LogLevel> LogLevels;
        internal static ConfigEntry<bool> DisplayLogsInConsole;

        internal static string BepInExVersion => typeof(BaseUnityPlugin).Assembly.GetName().Version.ToString();

        private void Awake()
        {
            LogLevels = Config.Bind("Console", "LogLevels",
                LogLevel.Message | LogLevel.Info | LogLevel.Warning | LogLevel.Error,
                "Log levels to log to Valheim console.");
            DisplayLogsInConsole = Config.Bind("Console", "DisplayBepInExLogs",
                true,
                "If true, will display BepInEx logs in Valheim console.");

            if (DisplayLogsInConsole.Value)
                BepInEx.Logging.Logger.Listeners.Add(new ValheimConsoleListener());

            Harmony.CreateAndPatchAll(typeof(DisplayInfoPlugin));
        }

        [HarmonyPatch(typeof(FejdStartup), "Start")]
        [HarmonyPostfix]
        private static void OnFejdStartup(FejdStartup __instance)
        {
            var parent = __instance.m_versionLabel.transform.parent.gameObject;
            var bepinGo = new GameObject("BepInEx Version");
            bepinGo.transform.parent = parent.transform;
            bepinGo.AddComponent<CanvasRenderer>();
            bepinGo.transform.localPosition = Vector3.zero;
            var rt = bepinGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.03f, 0.95f);
            rt.anchorMax = new Vector2(0.3f, 0.95f);
            var text = bepinGo.AddComponent<Text>();
            text.font = Font.CreateDynamicFontFromOSFont("Arial", 20);
            text.text =
                $"Running BepInEx {BepInExVersion}\n{Chainloader.PluginInfos.Count} plugins loaded\nPress F5 to open console";
            text.color = Color.white;
            text.fontSize = 20;
        }

        [HarmonyPatch(typeof(ZNet), nameof(ZNet.SetServer))]
        [HarmonyPostfix]
        private static void OnServerStart(string serverName)
        {
            var cm = typeof(BaseUnityPlugin).Assembly.GetType("BepInEx.ConsoleManager", false);
            if (cm == null)
                return;
            var setTitle = AccessTools.MethodDelegate<Action<string>>(AccessTools.Method(cm, "SetConsoleTitle"));
            setTitle($"BepInEx {BepInExVersion} - Valheim Server - {serverName}");
        }

        [HarmonyPatch(typeof(Console), nameof(Console.Awake))]
        [HarmonyPostfix]
        private static void FixConsoleMesh()
        {
            if (Console.instance && Console.instance.m_chatWindow.gameObject)
            {
                foreach (var outline in Console.instance.m_chatWindow.gameObject.GetComponentsInChildren<Outline>())
                {
                    outline.enabled = false;
                }
            }
        }

        [HarmonyPatch(typeof(Console), nameof(Console.UpdateChat))]
        [HarmonyPostfix]
        private static void FixConsoleMesh2()
        {
            if (Console.instance.m_output.text.Length > 6000)
            {
                Console.instance.m_chatBuffer.Clear();
            }
        }
    }
}
