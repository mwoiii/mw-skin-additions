using UnityEngine;

namespace MwSkinAdditions {
    public class TransformationInstance {
        public Transform boneTransform;

        public Transform relativeBone;

        // there are instances where the bone transforms aren't reset between offsets and the changes compound unless accounted for
        // so that's what these vectors are for
        public Vector3 prevLocalPosition = new Vector3(float.NaN, float.NaN, float.NaN);

        public Vector3 prevPositionOffset;
    }
}
