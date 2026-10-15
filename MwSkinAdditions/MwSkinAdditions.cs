using BepInEx;
using MwSkinAdditions.Networking;
using R2API.Networking;
using R2API.Utils;
using RoR2;

namespace MwSkinAdditions {

    [BepInDependency(NetworkingAPI.PluginGUID)]
    [BepInDependency(R2API.Skins.PluginGUID)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class MwSkinAdditions : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "mwmw";
        public const string PluginName = "MwSkinAdditions";
        public const string PluginVersion = "1.1.4";
        public static PluginInfo pluginInfo;
        public static MwSkinAdditions instance;

        public void Awake() {
            instance = this;
            pluginInfo = Info;
            Log.Init(Logger);
            NetworkMessages.Init();
            SkinEvents.Init();
            RoR2Application.onStart += () => { new ContentPacks().Initialize(); };
        }
    }
}
