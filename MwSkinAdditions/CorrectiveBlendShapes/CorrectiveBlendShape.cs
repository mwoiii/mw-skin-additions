using MwSkinAdditions.CorrectiveBlendShapes;

namespace MwSkinAdditions {
    public class CorrectiveBlendShape {
        public string meshName;

        public string targetBonePath;

        public string blendShapeName;

        public BaseCorrectionMethod correctionMethod;

        public CorrectiveBlendShape(string meshName, string targetBonePath, string blendShapeName, AxisCorrection axisCorrection) {
            this.meshName = meshName;
            this.targetBonePath = targetBonePath;
            this.blendShapeName = blendShapeName;
            correctionMethod = axisCorrection;
        }

        public CorrectiveBlendShape(string meshName, string targetBonePath, string blendShapeName, LineCorrection lineCorrection) {
            this.meshName = meshName;
            this.targetBonePath = targetBonePath;
            this.blendShapeName = blendShapeName;
            correctionMethod = lineCorrection;
        }

        public CorrectiveBlendShape(string meshName, string blendShapeName, DuoCorrection duoCorrection) {
            this.meshName = meshName;
            this.blendShapeName = blendShapeName;
            correctionMethod = duoCorrection;
        }
    }
}
