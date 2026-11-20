using HG;
using UnityEngine;

namespace MwSkinAdditions {
    public class CorrectionInstance {
        public Transform boneTransform;

        public int blendShapeIndex;

        public SkinnedMeshRenderer renderer;

        public CorrectiveBlendShape baseData;

        public bool resolved;

        public CorrectionInstance(GameObject body, CorrectiveBlendShape baseData) {
            this.baseData = baseData;
            var model = SkinEvents.GetModelFromEventBody(body).AsValidOrNull();
            boneTransform = model?.transform.Find(baseData.targetBonePath);
            renderer = model?.transform.Find(baseData.meshName).AsValidOrNull()?.GetComponent<SkinnedMeshRenderer>();
            if (boneTransform && renderer && renderer.sharedMesh) {
                blendShapeIndex = renderer.sharedMesh.GetBlendShapeIndex(baseData.blendShapeName);
                if (blendShapeIndex != -1) {
                    resolved = true;
                }
            }
        }
    }
}
