using UnityEngine;
using UnityEngine.XR;

namespace MwSkinAdditions {
    public class TransformationInstance {
        public GameObject model;

        public Transform boneTransform;

        public Transform relativeBone;

        public Transform relativeBoneOverride;

        public Vector3? positionOverride = null;

        public Vector3? localScaleOverride = null;

        public bool shouldApply = true;

        public Animator animator;

        public void SetRelativeBoneByPath(string path) {
            if (path != null) {
                relativeBone = model.transform.Find(path);
                if (!relativeBone) {
                    Log.Error($"Received invalid relative bone path: {path}");
                    relativeBone = boneTransform;
                }
            } else {
                relativeBone = boneTransform;
            }
        }

        public void SetRelativeBoneOverrideByPath(string path) {
            if (path != null) {
                relativeBoneOverride = model.transform.Find(path);
                if (!relativeBoneOverride) {
                    Log.Error($"Received invalid relative bone override path: {path}");
                    relativeBoneOverride = null;
                }
            } else {
                relativeBoneOverride = null;
            }
        }

        public void RemoveAllOverrides() {
            relativeBoneOverride = null;
            positionOverride = null;
            localScaleOverride = null;
        }

        // there are instances where the bone transforms aren't reset between offsets and the changes compound unless accounted for
        // so that's what these vectors are for
        public Vector3 prevLocalPosition = new Vector3(float.NaN, float.NaN, float.NaN);

        public Vector3 prevPositionOffset;
    }
}
