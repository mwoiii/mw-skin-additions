using static MwSkinAdditions.MwUtils;

namespace MwSkinAdditions {
    public class CorrectiveBlendShape {
        public string meshName;

        public string targetBonePath;

        public string blendShapeName;

        public float minAngle;

        public float maxAngle;

        public TargetAxis targetAxis;

        public AngleDirection direction;

        public float inverseLerpLowerThreshold;

        public float inverseLerpUpperThreshold;

        public CorrectiveBlendShape(string meshName, string targetBonePath, string blendShapeName, float minAngle, float maxAngle, TargetAxis targetAxis, AngleDirection direction = AngleDirection.AntiClockwise, float anglePadding = -1f) {
            this.meshName = meshName;
            this.targetBonePath = targetBonePath;
            this.blendShapeName = blendShapeName;
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
            Log.Debug($"set lower threshold {inverseLerpLowerThreshold} and upper threshold {inverseLerpUpperThreshold} for \"{targetBonePath}\"");
        }
    }
}
