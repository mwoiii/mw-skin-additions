using RoR2;
using UnityEngine;

namespace MwSkinAdditions {

    public class VoiceInfo {
        public float approxDuration;
        public NetworkSoundEventDef sound;

        public VoiceInfo(string soundString, float approxDuration) {
            this.approxDuration = approxDuration;
            sound = ContentPacks.CreateAndAddNetworkSoundEventDef(soundString);
        }

        private static NetworkSoundEventDef CreateNetworkSoundEventDef(string eventName) {
            NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
            networkSoundEventDef.eventName = eventName;

            return networkSoundEventDef;
        }
    }
}
