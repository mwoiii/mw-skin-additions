namespace MwSkinAdditions {
    public abstract class BaseCorrectionMethod {
        public bool debug;

        public abstract float Evaluate(CorrectionInstance instance);

        public virtual void Init(CorrectionInstance instance) {
            if (debug) {
                StartDebug(instance);
            }
        }

        public virtual void StartDebug(CorrectionInstance instance) {
        }

        public virtual void EvaluateAndApply(CorrectionInstance instance) {
            float weight = Evaluate(instance);
            if (weight >= 0f) {
                instance.renderer.SetBlendShapeWeight(instance.blendShapeIndex, weight);
            }
        }
    }
}
