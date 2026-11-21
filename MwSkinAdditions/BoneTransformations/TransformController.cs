using HG;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MwSkinAdditions {
    public class TransformController : MwBaseComponent {

        private List<TransformationInstance> transformInstances;

        public Dictionary<Transform, TransformationReversion> transformReversions;

        private CharacterBody characterBody;

        private Animator animator;

        private GameObject model;

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

        public override void Init(EventSub eventSub) {
            boneTransformations = eventSub.options.boneTransformations;
            rtiBoneTransformation = boneTransformations[rtiIndex];
            _rtiPositionVector = rtiBoneTransformation.position;
            _rtiLocalScaleVector = rtiBoneTransformation.localScale;
            model = SkinEvents.GetModelFromEventBody(gameObject);
            animator = model.AsValidOrNull()?.GetComponent<Animator>();
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
                TransformationInstance transformInstance = new TransformationInstance(model, animator, boneTransformations[i]);

                if (transformInstance.resolved) {
                    transformInstances.Add(transformInstance);
                    if (!transformReversions.ContainsKey(transformInstance.boneTransform)) {
                        transformReversions.Add(transformInstance.boneTransform, new TransformationReversion(transformInstance.boneTransform));
                    }
                } else {
                    Log.Error($"Failed to resolve TransformationInstance with target bone path \"{transformInstance.source.targetBonePath}\"");
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
            foreach (var instance in transformInstances) {
                if (instance == null || !instance.shouldApply) {
                    continue;
                }

                Transform boneTransform = instance.boneTransform;
                BoneTransformation source = instance.source;

                // apply position
                if ((!characterBody || (characterBody && !characterBody.currentVehicle)) && animator && animator.enabled) {
                    Transform relativeBone = instance.relativeBoneOverride ? instance.relativeBoneOverride : instance.relativeBone;
                    bool localHasMoved = instance.prevLocalPosition != boneTransform.localPosition;
                    if (boneTransform && relativeBone && (localHasMoved || forcePositionUpdate)) {
                        Vector3 positionOffset = instance.positionOverride != null ? (Vector3)instance.positionOverride : source.position;
                        if (!localHasMoved) {
                            boneTransform.position = relativeBone.TransformPoint(positionOffset - instance.prevPositionOffset);
                        } else {
                            boneTransform.position = relativeBone.TransformPoint(positionOffset);
                        }
                        instance.prevLocalPosition = boneTransform.localPosition;
                        instance.prevPositionOffset = positionOffset;
                    }
                }

                // apply scale
                if (instance.boneTransform) {
                    boneTransform.localScale = instance.localScaleOverride != null ? (Vector3)instance.localScaleOverride : source.localScale;
                }

                // run callbacks
                source.OnUpdate?.Invoke(instance);
            }
            forcePositionUpdate = false;
        }

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

        public override void OnSwapWith<T>(T component) {
            if (component is TransformController) {
                var oldController = component as TransformController;
                transformReversions = oldController.transformReversions;
                oldController.RevertPositionAndScale();
            }
        }
    }
}
