namespace MwSkinAdditions {
    public class BlinkBlendShapeAnimation : BlendShapeAnimation {
        public BlinkBlendShapeAnimation(
            string meshName,
            string blendShapeName,
            int feature = -1,
            int priority = 0,
            float fadeInDuration = 0.1f,
            float holdDuration = 0f,
            float fadeOutDuration = 0.1f,
            bool blockBlinking = false
            ) : base(meshName, blendShapeName, feature, priority, fadeInDuration, holdDuration, fadeOutDuration, blockBlinking) {
        }
    }
}
