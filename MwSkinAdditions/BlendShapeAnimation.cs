namespace MwSkinAdditions {
    public class BlendShapeAnimation {

        public string meshName;

        public string blendShapeName;

        public int feature;

        public int priority;

        public float fadeInDuration;

        public float holdDuration;

        public float fadeOutDuration;

        public bool blockBlinking;

        public BlendShapeAnimation(string meshName, string blendShapeName, int feature, int priority, float fadeInDuration, float holdDuration, float fadeOutDuration, bool blockBlinking = false) {
            this.meshName = meshName;
            this.blendShapeName = blendShapeName;
            this.feature = feature;
            this.priority = priority;
            this.fadeInDuration = fadeInDuration;
            this.holdDuration = holdDuration;
            this.fadeOutDuration = fadeOutDuration;
            this.blockBlinking = blockBlinking;
        }
    }
}
