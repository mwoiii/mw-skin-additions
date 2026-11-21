namespace MwSkinAdditions.CorrectiveBlendShapes {
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
    }
}
