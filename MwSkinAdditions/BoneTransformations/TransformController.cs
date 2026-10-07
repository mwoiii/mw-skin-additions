using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MwSkinAdditions {
    public class TransformController : MonoBehaviour {

        private List<TransformationInstance> transformInstances;

        private CharacterBody characterBody;

        private Animator animator;

        public bool beingDeleted = false;

        #region RuntimeInspector stuff for easy testing

        public BoneTransformation[] boneTransformations;

        public BoneTransformation rtiBoneTransformation;

        private int _rtiIndex = 0;

        private bool forcePositionUpdate;

        private Vector3 _rtiPositionVector;

        private Vector3 _rtiLocalScaleVector;

        public int rtiIndex {
            get { return _rtiIndex; }
            set {
                _rtiIndex = value;
                rtiBoneTransformation = boneTransformations[_rtiIndex];
                _rtiPositionVector = rtiBoneTransformation.position;
                _rtiLocalScaleVector = rtiBoneTransformation.localScale;
            }
        }

        public Vector3 rtiPositionVector {
            get { return _rtiPositionVector; }
            set {
                _rtiPositionVector = value;
                rtiBoneTransformation.position = _rtiPositionVector;
                forcePositionUpdate = true;
            }
        }
        public Vector3 rtiLocalScaleVector {
            get { return _rtiLocalScaleVector; }
            set {
                _rtiLocalScaleVector = value;
                rtiBoneTransformation.localScale = _rtiLocalScaleVector;
            }
        }

        #endregion

        public void Init(EventSub eventSub) {
            boneTransformations = eventSub.boneTransformations;
            rtiBoneTransformation = boneTransformations[rtiIndex];
            _rtiPositionVector = rtiBoneTransformation.position;
            _rtiLocalScaleVector = rtiBoneTransformation.localScale;
            AssignTransformations();
        }

        private void Start() {
            characterBody = GetComponent<CharacterBody>();
            animator = SkinEvents.GetModelFromEventBody(gameObject).GetComponent<Animator>();
        }

        private void AssignTransformations() {
            transformInstances = new List<TransformationInstance>();

            for (int i = 0; i < boneTransformations.Length; i++) {
                Transform bone = SkinEvents.GetModelFromEventBody(gameObject).transform.Find(boneTransformations[i].armaturePath);
                if (bone) {
                    TransformationInstance transformInstance = new TransformationInstance() {
                        boneTransform = bone
                    };
                    transformInstances.Add(transformInstance);
                    if (boneTransformations[i].relativeBonePath != null) {
                        Transform relativeBone = SkinEvents.GetModelFromEventBody(gameObject).transform.Find(boneTransformations[i].relativeBonePath);
                        if (relativeBone) {
                            transformInstance.relativeBone = relativeBone;
                        } else {
                            Log.Error($"Received invalid relative bone path: {boneTransformations[i].relativeBonePath}");
                            transformInstance.relativeBone = bone;
                        }
                    } else {
                        transformInstance.relativeBone = bone;
                    }
                } else {
                    Log.Error($"Received invalid bone path: {boneTransformations[i].armaturePath}");
                }
            }
        }

        private void LateUpdate() {
            // void death
            if (characterBody && !characterBody.modelLocator.modelTransform) {
                return;
            }

            if ((!characterBody || (characterBody && !characterBody.currentVehicle)) && animator && animator.enabled) {
                ApplyPosition();
            }

            ApplyScale();
        }

        private void ApplyScale() {
            for (int i = 0; i < transformInstances.Count; i++) {
                Transform boneTransform = transformInstances[i].boneTransform;
                if (boneTransform) {
                    boneTransform.localScale = boneTransformations[i].localScale;
                }
            }
        }

        private void ApplyPosition() {
            for (int i = 0; i < transformInstances.Count; i++) {
                TransformationInstance transformInstance = transformInstances[i];
                if (transformInstance == null || (!Run.instance && !boneTransformations[i].applyInLobby)) {
                    continue;
                }
                Transform boneTransform = transformInstance.boneTransform;
                Transform relativeBone = transformInstance.relativeBone;
                if (boneTransform && relativeBone && (transformInstance.prevLocalPosition != boneTransform.localPosition || forcePositionUpdate)) {
                    boneTransform.position = relativeBone.TransformPoint(boneTransformations[i].position);
                    transformInstance.prevLocalPosition = boneTransform.localPosition;
                }
            }
        }
    }
}
