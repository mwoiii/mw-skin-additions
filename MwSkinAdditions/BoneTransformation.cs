using UnityEngine;

namespace MwSkinAdditions {
    public class BoneTransformation {

        public string armaturePath;

        public Vector3 localScale;

        public Vector3 position;

        public string relativeBonePath;

        public BoneTransformation(string armaturePath, Vector3 localScale, Vector3 localPosition, string relativeBonePath = null) {
            this.armaturePath = armaturePath;
            this.localScale = localScale;
            this.position = localPosition;
            this.relativeBonePath = relativeBonePath;
        }
    }
}
