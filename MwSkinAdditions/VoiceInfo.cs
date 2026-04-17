using RoR2;

namespace MwSkinAdditions {

    public class VoiceInfo {
        public float approxDuration;
        public NetworkSoundEventDef sound;

        public VoiceInfo(string soundString, float approxDuration) {
            this.approxDuration = approxDuration;
            sound = ContentPacks.CreateAndAddNetworkSoundEventDef(soundString);
        }
    }
}
