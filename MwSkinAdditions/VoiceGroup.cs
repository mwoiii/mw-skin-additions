namespace MwSkinAdditions {
    public class VoiceGroup {
        public VoiceArray[] voiceArrays;

        public float minWait;

        public float maxWait;

        public VoiceGroup(VoiceArray[] voiceArrays, float minWait, float maxWait) {
            this.voiceArrays = voiceArrays;

            foreach (VoiceArray voiceArray in voiceArrays) {
                voiceArray.group = this;
            }
        }
    }
}
