using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using UnityEngine;

namespace MwSkinAdditions {
    internal class ContentPacks : IContentPackProvider {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => "mwmw.MwSkinAdditions";


        public static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();

        internal static NetworkSoundEventDef CreateAndAddNetworkSoundEventDef(string eventName) {
            NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
            networkSoundEventDef.eventName = eventName;

            networkSoundEventDefs.Add(networkSoundEventDef);

            return networkSoundEventDef;
        }

        public void Initialize() {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider) {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args) {
            this.contentPack.identifier = this.identifier;

            contentPack.networkSoundEventDefs.Add(networkSoundEventDefs.ToArray());

            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args) {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args) {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
