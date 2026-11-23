namespace MwSkinAdditions {
    public class CorrectiveBlendShape {
        public string meshName;

        public string targetBonePath;

        public string blendShapeName;

        public BaseCorrectionMethod correctionMethod;

        public CorrectiveBlendShape(string meshName, string blendShapeName, BaseCorrectionMethod correctionMethod) {
            this.meshName = meshName;
            this.blendShapeName = blendShapeName;
            this.correctionMethod = correctionMethod;
        }

        public CorrectiveBlendShape(string meshName, string targetBonePath, string blendShapeName, BaseCorrectionMethod correctionMethod) {
            this.meshName = meshName;
            this.targetBonePath = targetBonePath;
            this.blendShapeName = blendShapeName;
            this.correctionMethod = correctionMethod;
        }
    }
}
