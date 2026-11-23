using RoR2BepInExPack.Utilities;

namespace MwSkinAdditions {
    public class DuoCorrection : BaseCorrectionMethod {

        public CorrectiveBlendShape correction1;

        public CorrectiveBlendShape correction2;

        public static FixedConditionalWeakTable<CorrectionInstance, CorrectionInstance[]> duoInstances = new();

        public DuoCorrection(CorrectiveBlendShape correction1, CorrectiveBlendShape correction2) {
            this.correction1 = correction1;
            this.correction2 = correction2;
        }

        public override void Init(CorrectionInstance instance) {
            base.Init(instance);
            duoInstances[instance] = [
                new CorrectionInstance(instance.body, correction1),
                new CorrectionInstance(instance.body, correction2),
            ];
        }

        public override float Evaluate(CorrectionInstance instance) {
            if (duoInstances.ContainsKey(instance)) {
                var corrections = duoInstances[instance];
                var weight1 = corrections[0].source.correctionMethod.Evaluate(corrections[0]);
                var weight2 = corrections[1].source.correctionMethod.Evaluate(corrections[1]);
                if (weight1 <= weight2 && weight1 > 0f) {
                    corrections[0].renderer.SetBlendShapeWeight(corrections[0].blendShapeIndex, 0f);
                    corrections[1].renderer.SetBlendShapeWeight(corrections[1].blendShapeIndex, weight2 - weight1);
                    return weight1;
                } else if (weight2 < weight1 && weight2 > 0f) {
                    corrections[0].renderer.SetBlendShapeWeight(corrections[0].blendShapeIndex, weight1 - weight2);
                    corrections[1].renderer.SetBlendShapeWeight(corrections[1].blendShapeIndex, 0f);
                    return weight2;
                } else {
                    corrections[0].renderer.SetBlendShapeWeight(corrections[0].blendShapeIndex, weight1);
                    corrections[1].renderer.SetBlendShapeWeight(corrections[1].blendShapeIndex, weight2);
                    return 0f;
                }
            }
            return -1f;
        }
    }
}
