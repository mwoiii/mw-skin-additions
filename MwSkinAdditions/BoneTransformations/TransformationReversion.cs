using UnityEngine;

namespace MwSkinAdditions {
    // for reverting changes when the skin changes in css
    // assumes that bone local scale and positions aren't really messed with
    // position won't matter for most bones as it'll be fixed by the animator anyway (?)
    public class TransformationReversion {
        public Transform boneTransform;

        public Vector3 initialLocalScale = Vector3.one;

        public Vector3 initialLocalPosition = Vector3.zero;

        public void Apply() {
            if (boneTransform) {
                boneTransform.localScale = initialLocalScale;
                boneTransform.localPosition = initialLocalPosition;
            }
        }
    }
}
