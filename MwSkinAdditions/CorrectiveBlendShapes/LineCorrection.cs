using System.Runtime.CompilerServices;
using UnityEngine;

namespace MwSkinAdditions.CorrectiveBlendShapes {
    public class LineCorrection : BaseCorrectionMethod {

        public Vector3 point1;

        public Vector3 point2;

        public Vector3 forward;

        public Vector3 normal;

        public bool equalPoints;

        public float startAngle;

        public LineCorrection(float startAngle, Vector3 point1, Vector3 point2, Vector3 forward) {
            this.startAngle = startAngle;
            this.point1 = point1.normalized;
            this.point2 = point2.normalized;
            this.forward = forward.normalized;

            if (this.point1 == -this.point2) {
                Log.Warning(
                    $"LineCorrection instance has point vectors that are directly opposite ({point1}, {point2}). " +
                    $"Cannot determine the intended path between the points, as there are infinite equidistant possibilities. " +
                    $"Changing point2 to a compatible unit vector."
                );
                float minAbs = float.PositiveInfinity;
                int minIndex = 0;
                for (int i = 0; i < 2; i++) {
                    var abs = Mathf.Abs(this.point2[i]);
                    if (abs < minAbs) {
                        minAbs = abs;
                        minIndex = i;
                    }
                }
                this.point2 = Vector3.zero;
                point2[minIndex] = 1f;
            }

            equalPoints = this.point1 == this.point2;
            if (!equalPoints) {
                normal = Vector3.Cross(this.point1, this.point2).normalized;
            }
        }

        public override float Evaluate(CorrectionInstance instance) {
            if (!instance.boneTransform) {
                return -1f;
            }

            var localForward = instance.boneTransform.localRotation * forward;

            float angle;
            if (equalPoints) {
                angle = Mathf.Acos(Vector3.Dot(localForward, point1));
            } else if (OccursBefore(point1, localForward) && OccursBefore(localForward, point2)) {
                angle = Mathf.Asin(Mathf.Abs(Vector3.Dot(localForward, normal)));
            } else {
                angle = Mathf.Acos(Mathf.Max(Vector3.Dot(localForward, point1), Vector3.Dot(localForward, point2)));
            }

            var weight = Mathf.InverseLerp(startAngle, 0f, angle * Mathf.Rad2Deg) * 100f;
            instance.renderer.SetBlendShapeWeight(instance.blendShapeIndex, weight);
            return weight;
        }

        public override void StartDebug(CorrectionInstance instance) {
            instance.boneTransform.gameObject.AddComponent<LineCorrectionDebugController>().Init(instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool OccursBefore(Vector3 v1, Vector3 v2) {
            return Vector3.Dot(Vector3.Cross(v1, v2), normal) >= 0f;
        }
    }
}
