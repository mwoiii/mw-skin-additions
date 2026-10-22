using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MwSkinAdditions {
    public class TransformController : MonoBehaviour {

        private List<TransformationInstance> transformInstances;

        public Dictionary<Transform, TransformationReversion> transformReversions;

        private CharacterBody characterBody;

        private Animator animator;

        public bool beingDeleted = false;

        #region RuntimeInspector stuff for easy testing

        public BoneTransformation[] boneTransformations;

        public BoneTransformation rtiBoneTransformation;

        private int _rtiIndex = 0;

        private bool forcePositionUpdate;

        private bool reverted;

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
            boneTransformations = eventSub.options.boneTransformations;
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

            if (transformReversions == null) {
                transformReversions = new Dictionary<Transform, TransformationReversion>();
            }

            for (int i = 0; i < boneTransformations.Length; i++) {
                Transform bone = SkinEvents.GetModelFromEventBody(gameObject).transform.Find(boneTransformations[i].armaturePath);
                if (bone) {
                    TransformContexts boneContext = boneTransformations[i].contexts;
                    TransformationInstance transformInstance = new TransformationInstance() {
                        boneTransform = bone,
                        shouldApply = (
                            // in a run
                            (Run.instance && ((boneContext & TransformContexts.Run) == TransformContexts.Run)) ||
                            // in css
                            (!Run.instance && ((boneContext & TransformContexts.CSS) == TransformContexts.CSS))
                        ),
                    };
                    transformInstances.Add(transformInstance);

                    if (!transformReversions.ContainsKey(bone)) {
                        transformReversions.Add(bone, new TransformationReversion() {
                            boneTransform = bone,
                            initialLocalScale = bone.localScale,
                            initialLocalPosition = bone.localPosition,
                        });
                    }

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

            ApplyPostionAndScale();
        }

        private void LateUpdate() {
            // void death
            if (characterBody && !characterBody.modelLocator.modelTransform) {
                return;
            }

            ApplyPostionAndScale();
        }

        private void ApplyPostionAndScale() {
            for (int i = 0; i < transformInstances.Count; i++) {
                TransformationInstance transformInstance = transformInstances[i];
                if (transformInstance == null || !transformInstance.shouldApply) {
                    continue;
                }
                Transform boneTransform = transformInstance.boneTransform;

                // apply position
                if ((!characterBody || (characterBody && !characterBody.currentVehicle)) && animator && animator.enabled) {
                    Transform relativeBone = transformInstance.relativeBone;
                    bool localHasMoved = transformInstance.prevLocalPosition != boneTransform.localPosition;
                    if (boneTransform && relativeBone && (localHasMoved || forcePositionUpdate)) {
                        Vector3 positionOffset = boneTransformations[i].position;
                        if (!localHasMoved) {
                            boneTransform.position = relativeBone.TransformPoint(positionOffset - transformInstance.prevPositionOffset);
                        } else {
                            boneTransform.position = relativeBone.TransformPoint(positionOffset);
                        }
                        transformInstance.prevLocalPosition = boneTransform.localPosition;
                        transformInstance.prevPositionOffset = positionOffset;
                    }
                }

                // apply scale
                if (transformInstance.boneTransform) {
                    boneTransform.localScale = boneTransformations[i].localScale;
                }
            }
            forcePositionUpdate = false;
        }

        //private void ApplyPosition() {
        //    for (int i = 0; i < transformInstances.Count; i++) {
        //        TransformationInstance transformInstance = transformInstances[i];
        //        if (transformInstance == null || !transformInstance.shouldApply) {
        //            continue;
        //        }
        //        Transform boneTransform = transformInstance.boneTransform;
        //        Transform relativeBone = transformInstance.relativeBone;
        //        bool localHasMoved = transformInstance.prevLocalPosition != boneTransform.localPosition;
        //        if (boneTransform && relativeBone && (localHasMoved || forcePositionUpdate)) {
        //            Vector3 positionOffset = boneTransformations[i].position;
        //            if (!localHasMoved) {
        //                boneTransform.position = relativeBone.TransformPoint(positionOffset - transformInstance.prevPositionOffset);
        //            } else {
        //                boneTransform.position = relativeBone.TransformPoint(positionOffset);
        //            }
        //            transformInstance.prevLocalPosition = boneTransform.localPosition;
        //            transformInstance.prevPositionOffset = positionOffset;
        //        }
        //    }
        //    forcePositionUpdate = false;
        //}

        private void OnDestroy() {
            if (!reverted) {
                RevertPositionAndScale();
            }
        }

        public void RevertPositionAndScale() {
            foreach (TransformationReversion reversion in transformReversions.Values) {
                reversion.Apply();
            }
            reverted = true;
        }
    }
}
