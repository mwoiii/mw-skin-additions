using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace MwSkinAdditions {
    public class TransformController : MonoBehaviour {

        private Transform[] boneTransforms;

        private Dictionary<Transform, Transform> boneRelativeTo = new Dictionary<Transform, Transform>();

        private CharacterBody characterBody;

        public bool beingDeleted = false;

        #region RuntimeInspector stuff for easy testing

        public BoneTransformation[] boneTransformations;

        public BoneTransformation rtiBoneTransformation;

        private int _rtiIndex = 0;

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
            AssignLimbs();
        }

        private void Start() {
            characterBody = GetComponent<CharacterBody>();
        }

        private void AssignLimbs() {
            List<Transform> tempBoneTransforms = new List<Transform>();

            for (int i = 0; i < boneTransformations.Length; i++) {
                Transform bone = SkinEvents.GetModelFromEventBody(gameObject).transform.Find(boneTransformations[i].armaturePath);
                if (bone != null) {
                    tempBoneTransforms.Add(SkinEvents.GetModelFromEventBody(gameObject).transform.Find(boneTransformations[i].armaturePath));
                    if (boneTransformations[i].relativeBonePath != null) {
                        Transform relativeBone = SkinEvents.GetModelFromEventBody(gameObject).transform.Find(boneTransformations[i].relativeBonePath);
                        if (relativeBone != null) {
                            boneRelativeTo[bone] = relativeBone;
                        } else {
                            Log.Error($"Received invalid relative bone path: {boneTransformations[i].relativeBonePath}");
                            boneRelativeTo[bone] = bone;
                        }
                    } else {
                        boneRelativeTo[bone] = bone;
                    }
                } else {
                    Log.Error($"Received invalid bone path: {boneTransformations[i].armaturePath}");
                }
            }

            boneTransforms = tempBoneTransforms.ToArray();
        }

        private void LateUpdate() {
            if (!characterBody || !characterBody.currentVehicle) {
                ApplyScale();
                ApplyPosition();
            }
        }

        private void ApplyScale() {
            for (int i = 0; i < boneTransforms.Length; i++) {
                boneTransforms[i].localScale = boneTransformations[i].localScale;
            }
        }

        private void ApplyPosition() {
            for (int i = 0; i < boneTransforms.Length; i++) {
                Transform relativeBone = boneRelativeTo[boneTransforms[i]];
                boneTransforms[i].position = relativeBone.TransformPoint(boneTransformations[i].position);
            }
        }
    }
}
