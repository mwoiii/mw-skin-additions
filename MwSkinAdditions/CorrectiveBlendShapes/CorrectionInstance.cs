using HG;
using UnityEngine;

namespace MwSkinAdditions {
    public class CorrectionInstance {
        public GameObject body;

        public Transform boneTransform;

        public int blendShapeIndex;

        public SkinnedMeshRenderer renderer;

        public CorrectiveBlendShape source;

        public bool resolved;

        public CorrectionInstance(GameObject body, CorrectiveBlendShape source) {
            this.body = body;
            this.source = source;
            var model = SkinEvents.GetModelFromEventBody(body).AsValidOrNull();
            renderer = model?.transform.Find(source.meshName).AsValidOrNull()?.GetComponent<SkinnedMeshRenderer>();
            if (renderer && renderer.sharedMesh) {
                if (source.targetBonePath != null) {
                    boneTransform = model.transform.Find(source.targetBonePath);
                }
                blendShapeIndex = renderer.sharedMesh.GetBlendShapeIndex(source.blendShapeName);
                if ((source.targetBonePath == null || (source.targetBonePath != null && boneTransform)) && blendShapeIndex != -1) {
                    resolved = true;
                    source.correctionMethod.Init(this);
                }
            }
        }
    }
}
