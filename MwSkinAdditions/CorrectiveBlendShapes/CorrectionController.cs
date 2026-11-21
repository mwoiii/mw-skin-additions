using System.Collections.Generic;

namespace MwSkinAdditions {
    public class CorrectionController : MwBaseComponent {

        public CorrectiveBlendShape[] correctiveBlendShapes;

        private List<CorrectionInstance> correctionInstances;

        public override void Init(EventSub eventSub) {
            correctiveBlendShapes = eventSub.options.correctiveBlendShapes;
            AssignCorrections();
        }

        private void AssignCorrections() {
            correctionInstances = new List<CorrectionInstance>();

            for (int i = 0; i < correctiveBlendShapes.Length; i++) {
                var instance = new CorrectionInstance(gameObject, correctiveBlendShapes[i]);
                if (instance.resolved) {
                    correctionInstances.Add(instance);
                } else {
                    Log.Error($"Failed to resolve CorrectionInstance with BlendShape name \"{instance.source.blendShapeName}\"");
                }
            }

            ApplyCorrections();
        }

        private void Update() {
            ApplyCorrections();
        }

        private void ApplyCorrections() {
            foreach (var instance in correctionInstances) {
                if (!instance.renderer) {
                    continue;
                }
                instance.source.correctionMethod.Evaluate(instance);
            }
        }

        //private void OnDestroy() {
        //    foreach (var instance in correctionInstances) {
        //        if (instance.renderer) {
        //            instance.renderer.SetBlendShapeWeight(instance.blendShapeIndex, 0f);
        //        }
        //    }
        //}
    }
}
