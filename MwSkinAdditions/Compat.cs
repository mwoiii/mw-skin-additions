using RoR2;
using System;
using System.Reflection;
using UnityEngine;

namespace MwSkinAdditions {
    public class Compat {
        private static bool? _iconsModEnabled;

        public static bool shouldOverrideIconsMod;

        public static bool iconsModEnabled {
            get {
                if (_iconsModEnabled == null) {
                    _iconsModEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(dteeicons.dteeicons.PluginGUID);
                }
                return (bool)_iconsModEnabled;
            }
        }

        // there is no nice way to interface with it :( so break it!! when needing to apply icons with this mod
        // communicate with the author to try and make a more flexible solution??? Are you out of your mind??????
        public static void EnableIconsModHooks() {
            const BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            try { // in case the class or namespace changes idk
                var method = typeof(dteeicons.dteeicons).GetMethod("ApplyDynamicIcon", allFlags);
                if (method != null) {
                    MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Add(method, OnIconsModApplyLobbyIcons);
                }
            } catch (Exception e) {
                Log.Error($"Error with Icons compat: {e}");
            }
        }

        private static void OnIconsModApplyLobbyIcons(Action<dteeicons.dteeicons, BodyIndex, int, Action<Texture2D>, string> orig, dteeicons.dteeicons self, BodyIndex bodyIndex, int skinIndex, Action<Texture2D> applyAction, string logPrefix) {
            if (shouldOverrideIconsMod) {
                skinIndex = -1; // make it get a null skindef so it returns early
                shouldOverrideIconsMod = false;
            }
            orig(self, bodyIndex, skinIndex, applyAction, logPrefix);
        }
    }
}
