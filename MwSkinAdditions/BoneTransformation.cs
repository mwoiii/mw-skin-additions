using UnityEngine;

namespace MwSkinAdditions {
    public class BoneTransformation {

        public string childName;

        public string armaturePath;

        public Vector3 localScale;

        public Vector3 localPosition;

        public BoneTransformation(string armaturePath, Vector3 localScale, Vector3 localPosition) {
            this.armaturePath = armaturePath;
            this.localScale = localScale;
            this.localPosition = localPosition;
        }
    }
}
