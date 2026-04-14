using System.Collections.Generic;
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
            List<Transform> tempBoneTransforms = new List<Transform>();

            for (int i = 0; i < eventSub.boneTransformations.Length; i++) {
                Transform bone = SkinEvents.GetModelFromEventBody(gameObject).transform.Find(eventSub.boneTransformations[i].armaturePath);
                if (bone != null) {
                    tempBoneTransforms.Add(SkinEvents.GetModelFromEventBody(gameObject).transform.Find(eventSub.boneTransformations[i].armaturePath));
                } else {
                    Log.Error($"Received invalid bone path: {eventSub.boneTransformations[i].armaturePath}");
                }
            }

            boneTransforms = tempBoneTransforms.ToArray();
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
