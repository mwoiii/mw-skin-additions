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
            animator = SkinEvents.GetModelFromEventBody(gameObject).GetComponent<Animator>();
            AssignTransformations();
        }

        private void Start() {
            characterBody = GetComponent<CharacterBody>();
        }

        private void AssignTransformations() {
            transformInstances = new List<TransformationInstance>();

            if (transformReversions == null) {
                transformReversions = new Dictionary<Transform, TransformationReversion>();
            }

            for (int i = 0; i < boneTransformations.Length; i++) {
                GameObject model = SkinEvents.GetModelFromEventBody(gameObject);
                Transform bone = model.transform.Find(boneTransformations[i].armaturePath);
                if (bone) {
                    TransformContexts boneContext = boneTransformations[i].contexts;
                    TransformationInstance transformInstance = new TransformationInstance() {
                        model = model,
                        boneTransform = bone,
                        shouldApply = (
                            // in a run
                            (Run.instance && ((boneContext & TransformContexts.Run) == TransformContexts.Run)) ||
                            // in css
                            (!Run.instance && ((boneContext & TransformContexts.CSS) == TransformContexts.CSS))
                        ),
                        animator = animator
                    };
                    transformInstance.SetRelativeBoneByPath(boneTransformations[i].relativeBonePath);
                    transformInstances.Add(transformInstance);

                    if (!transformReversions.ContainsKey(bone)) {
                        transformReversions.Add(bone, new TransformationReversion() {
                            boneTransform = bone,
                            initialLocalScale = bone.localScale,
                            initialLocalPosition = bone.localPosition,
                        });
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
                TransformationInstance transformationInstance = transformInstances[i];
                if (transformationInstance == null || !transformationInstance.shouldApply) {
                    continue;
                }

                Transform boneTransform = transformationInstance.boneTransform;
                BoneTransformation transformationBase = boneTransformations[i];

                // apply position
                if ((!characterBody || (characterBody && !characterBody.currentVehicle)) && animator && animator.enabled) {
                    Transform relativeBone = transformationInstance.relativeBoneOverride ? transformationInstance.relativeBoneOverride : transformationInstance.relativeBone;
                    bool localHasMoved = transformationInstance.prevLocalPosition != boneTransform.localPosition;
                    if (boneTransform && relativeBone && (localHasMoved || forcePositionUpdate)) {
                        Vector3 positionOffset = transformationInstance.positionOverride != null ? (Vector3)transformationInstance.positionOverride : transformationBase.position;
                        if (!localHasMoved) {
                            boneTransform.position = relativeBone.TransformPoint(positionOffset - transformationInstance.prevPositionOffset);
                        } else {
                            boneTransform.position = relativeBone.TransformPoint(positionOffset);
                        }
                        transformationInstance.prevLocalPosition = boneTransform.localPosition;
                        transformationInstance.prevPositionOffset = positionOffset;
                    }
                }

                // apply scale
                if (transformationInstance.boneTransform) {
                    boneTransform.localScale = transformationInstance.localScaleOverride != null ? (Vector3)transformationInstance.localScaleOverride : transformationBase.localScale;
                }

                // run callbacks
                transformationBase.OnUpdate?.Invoke(transformationInstance);
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
