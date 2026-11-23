using UnityEngine;
using static MwSkinAdditions.MwUtils;

namespace MwSkinAdditions {
    public class AxisCorrection : BaseCorrectionMethod {
        public float minAngle;

        public float maxAngle;

        public TargetAxis targetAxis;

        public AngleDirection direction;

        public float inverseLerpLowerThreshold;

        public float inverseLerpUpperThreshold;

        public AxisCorrection(float minAngle, float maxAngle, TargetAxis targetAxis, AngleDirection direction = AngleDirection.AntiClockwise, float anglePadding = -1f) {
            this.minAngle = Mod(minAngle, 360f);
            this.maxAngle = Mod(maxAngle, 360f);
            this.targetAxis = targetAxis;
            this.direction = direction;

            // minus an arbitrary small value to ensure InverseLerpAngle returns +ve or -ve values correctly.
            // the position exactly 180 degrees from the middle is always +ve I think. don't want lower threshold to be +ve
            var maxAnglePadding = (360f - AngleDelta(minAngle, maxAngle, direction)) * 0.5f - 0.01f;
            if (anglePadding > maxAnglePadding || anglePadding < 0f) {
                anglePadding = maxAnglePadding;
            }

            if (direction == AngleDirection.AntiClockwise) {
                inverseLerpLowerThreshold = InverseLerpAngle(minAngle, maxAngle, Mod(minAngle - anglePadding, 360f), direction);
                inverseLerpUpperThreshold = InverseLerpAngle(minAngle, maxAngle, Mod(maxAngle + anglePadding, 360f), direction);
            } else {
                inverseLerpLowerThreshold = InverseLerpAngle(minAngle, maxAngle, Mod(minAngle + anglePadding, 360f), direction);
                inverseLerpUpperThreshold = InverseLerpAngle(minAngle, maxAngle, Mod(maxAngle - anglePadding, 360f), direction);
            }
        }

        public override float Evaluate(CorrectionInstance instance) {
            if (!instance.boneTransform) {
                return -1f;
            }

            var currentAngle = instance.boneTransform.localEulerAngles[(int)targetAxis];
            var pos = InverseLerpAngle(minAngle, maxAngle, currentAngle, direction);
            float weight = -1f;
            if (pos > inverseLerpLowerThreshold && pos < inverseLerpUpperThreshold) {
                if (pos < 0) { // in lower padding
                    weight = 0f;
                } else if (pos <= 1) { // between 0 and 1
                    weight = Mathf.Clamp01(InverseLerpAngle(minAngle, maxAngle, currentAngle, direction)) * 100f;
                } else { // in upper padding
                    weight = 100f;
                }
            }
            return weight;
        }
    }
}
