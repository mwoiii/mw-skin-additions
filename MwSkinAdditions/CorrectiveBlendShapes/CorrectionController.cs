using System.Collections.Generic;
using UnityEngine;
using static MwSkinAdditions.MwUtils;

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
                var correctionInstance = new CorrectionInstance(gameObject, correctiveBlendShapes[i]);
                if (correctionInstance.resolved) {
                    correctionInstances.Add(correctionInstance);
                } else {
                    Log.Error($"Failed to resolve CorrectionInstance with BlendShape name \"{correctionInstance.baseData.blendShapeName}\"");
                }
            }

            ApplyCorrections();
        }

        private void Update() {
            ApplyCorrections();
        }

        private void ApplyCorrections() {
            foreach (var instance in correctionInstances) {
                if (!instance.boneTransform || !instance.renderer) {
                    continue;
                }

                var currentAngle = instance.boneTransform.localEulerAngles[(int)instance.baseData.targetAxis];
                var pos = InverseLerpAngle(instance.baseData.minAngle, instance.baseData.maxAngle, currentAngle, instance.baseData.direction);

                if (pos > instance.baseData.inverseLerpLowerThreshold && pos < instance.baseData.inverseLerpUpperThreshold) {
                    float weight;
                    if (pos < 0) { // in lower padding
                        weight = 0f;
                    } else if (pos <= 1) { // between 0 and 1
                        weight = Mathf.Clamp01(InverseLerpAngle(instance.baseData.minAngle, instance.baseData.maxAngle, currentAngle, instance.baseData.direction)) * 100f;
                    } else { // in upper padding
                        weight = 100f;
                    }
                    instance.renderer.SetBlendShapeWeight(instance.blendShapeIndex, weight);
                }
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
