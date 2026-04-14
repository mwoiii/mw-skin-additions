using UnityEngine;

namespace MwSkinAdditions {
    public class TransformController : MonoBehaviour {

        private EventSub eventSub;

        private Transform[] boneTransforms;

        public void Init(EventSub eventSub) {
            this.eventSub = eventSub;
        }

        private void Start() {
            AssignLimbs();
        }
        private void AssignLimbs() {
            boneTransforms = new Transform[eventSub.boneTransformations.Length];

            for (int i = 0; i < boneTransforms.Length; i++) {
                boneTransforms[i] = SkinEvents.GetModelFromEventBody(gameObject).transform.Find(eventSub.boneTransformations[i].armaturePath);
            }
        }

        private void LateUpdate() {
            ApplyScale();
            ApplyPosition();
        }

        private void ApplyScale() {
            for (int i = 0; i < boneTransforms.Length; i++) {
                boneTransforms[i].localScale = eventSub.boneTransformations[i].localScale;
            }
        }

        private void ApplyPosition() {
            for (int i = 0; i < boneTransforms.Length; i++) {
                boneTransforms[i].localPosition += eventSub.boneTransformations[i].localPosition;
            }
        }
    }
}
