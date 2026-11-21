using RoR2;
using UnityEngine;

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

        public BoneTransformation source;

        public bool resolved;

        public TransformationInstance(GameObject model, Animator animator, BoneTransformation source) {
            this.model = model;
            this.animator = animator;
            this.source = source;
            if (model) {
                boneTransform = model.transform.Find(source.targetBonePath);
                shouldApply = (
                    // in a run
                    (Run.instance && ((source.contexts & TransformContexts.Run) == TransformContexts.Run)) ||
                    // in css
                    (!Run.instance && ((source.contexts & TransformContexts.CSS) == TransformContexts.CSS))
                );
                SetRelativeBoneByPath(source.relativeBonePath);
                if (boneTransform) {
                    resolved = true;
                }
            }
        }

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
