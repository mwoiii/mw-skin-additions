using BepInEx;
using MwSkinAdditions.Networking;
using R2API.Networking;
using R2API.Utils;

namespace MwSkinAdditions {

    [BepInDependency(NetworkingAPI.PluginGUID)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class MwSkinAdditions : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "mwmw";
        public const string PluginName = "MwSkinAdditions";
        public const string PluginVersion = "1.1.0";
        public static PluginInfo pluginInfo;
        public static MwSkinAdditions instance;

        public void Awake() {
            instance = this;
            pluginInfo = Info;
            Log.Init(Logger);
            NetworkMessages.Init();
            SkinEvents.Init();
            RoR2.RoR2Application.onStart += () => { new ContentPacks().Initialize(); };
        }
    }
}
