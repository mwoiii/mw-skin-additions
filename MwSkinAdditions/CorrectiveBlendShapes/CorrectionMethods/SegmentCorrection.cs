using UnityEngine;

namespace MwSkinAdditions {
    public class SegmentCorrection : BaseCorrectionMethod {
        // axis held only for debugging
        public Vector3 axis1;

        public Vector3 axis2;

        public Vector3 normal;

        public Vector3 forward;

        public float minSignedAngle;

        public float maxSignedAngle;

        public SegmentCorrection(float minSignedAngle, float maxSignedAngle, Vector3 axis1, Vector3 axis2, Vector3 forward) {
            this.minSignedAngle = minSignedAngle;
            this.maxSignedAngle = maxSignedAngle;
            this.axis1 = axis1.normalized;
            this.axis2 = axis2.normalized;
            normal = Vector3.Cross(axis1.normalized, axis2.normalized).normalized;
            this.forward = forward.normalized;
        }

        public override float Evaluate(CorrectionInstance instance) {
            if (!instance.boneTransform) {
                return -1f;
            }

            var localForward = instance.boneTransform.localRotation * forward;
            var angle = Mathf.Asin(Vector3.Dot(localForward, normal)) * Mathf.Rad2Deg;

            var weight = 0f;
            if (angle > maxSignedAngle) {
                weight = 100f;
            } else if (angle > minSignedAngle) {
                weight = Mathf.InverseLerp(minSignedAngle, maxSignedAngle, angle) * 100f;
            }

            return weight;
        }

        public override void StartDebug(CorrectionInstance instance) {
            instance.boneTransform.gameObject.AddComponent<SegmentCorrectionDebugController>().Init(instance);
        }
    }
}
