using RoR2;
using System;
using UnityEngine;

namespace MwSkinAdditions {
    public class EventSub {

        public SkinDef skinDef;

        public BoneTransformation[] boneTransformations;

        public ExtraObject[] extraObjects;

        public bool useAnimations = false;

        public BlendShapeAnimation[] blinkAnimations;

        public IdleAnimation[] conditionalIdleAnimations;

        public VoiceGroup[] voiceGroups;


        public static Action<GameObject> DifferentSkinAppliedGlobal;

        public Action<GameObject> SkinAppliedLobby;

        public Action<GameObject> SkinAppliedRun;

        public Action<GameObject, DamageReport> TakeDamage;

        public Action<GameObject> Death;

        public Action<GameObject> DefeatBossGroup;

        public Action<GameObject> LeavePod;

        public Action<GameObject> UsePrimary;

        public Action<GameObject> UseSecondary;

        public Action<GameObject> UseUtility;

        public Action<GameObject> UseSpecial;

        public Action<GameObject> ShrineSuccess;

        public Action<GameObject> ShrineFailure;

        public Action<GameObject> TeleporterStart;

        public Action<GameObject> TeleporterEnd;

        public Action<GameObject> BearDamageBlock;

        public Action<GameObject> LevelUp;

        public Action<GameObject> MithrixDefeat;

        public Action<GameObject> UseEquipment;

        public Action<GameObject, float> Heal;

        public Action<GameObject> Jump;

        public Action<GameObject> LeaveStage;

        public Action<GameObject> Idle;

        public Action<GameObject, ItemIndex> GetItem;

        public Action<GameObject> HoldoutZoneCharged;

        public EventSub(SkinDef skinDef, BoneTransformation[] boneTransformations = null, ExtraObject[] extraObjects = null,
                        bool useAnimations = false, BlendShapeAnimation[] blinkAnimations = null, IdleAnimation[] conditionalIdleAnimations = null,
                        VoiceGroup[] voiceGroups = null) {
            this.skinDef = skinDef;
            this.boneTransformations = boneTransformations;
            this.extraObjects = extraObjects;
            this.useAnimations = useAnimations;
            this.blinkAnimations = blinkAnimations;
            this.conditionalIdleAnimations = conditionalIdleAnimations;
            this.voiceGroups = voiceGroups;
        }

        public void Init() {
            SkinEvents.SubscribeEventSkin(this);
            if (boneTransformations != null) {
                SubscribeTransformEvents();
            }

            if (extraObjects != null) {
                SubscribeExtraObjectEvents();
            }

            if (useAnimations) {
                SubscribeAnimationEvents();
            }

            if (voiceGroups != null) {
                SubscribeVoiceEvents();
            }
        }

        private void SubscribeTransformEvents() {
            SkinAppliedRun += AddTransformController;
            SkinAppliedLobby += AddTransformController;
            Death += DisableTransformController;
        }

        private void AddTransformController(GameObject body) {
            TransformController transformController = body.GetComponent<TransformController>();
            if (transformController == null || transformController.beingDeleted) {
                transformController = body.AddComponent<TransformController>();
            }
            transformController.Init(this);
        }

        public void DisableTransformController(GameObject body) {
            TransformController transformController = body?.GetComponent<TransformController>();
            if (transformController != null) {
                transformController.enabled = false;
            }
        }

        private void SubscribeExtraObjectEvents() {
            SkinAppliedRun += AddExtraObjects;
            SkinAppliedLobby += AddExtraObjects;
        }

        private void AddExtraObjects(GameObject body) {
            Transform model = SkinEvents.GetModelFromEventBody(body).transform;
            ExtraObjectController extraObjectController = body.AddComponent<ExtraObjectController>();

            foreach (ExtraObject extraObject in extraObjects) {
                GameObject obj = UnityEngine.Object.Instantiate(extraObject.prefab);
                obj.transform.parent = model.Find(extraObject.armatureParentPath);
                obj.transform.localPosition = extraObject.localPosition;
                obj.transform.localEulerAngles = extraObject.localEulerAngles;
                obj.transform.localScale = extraObject.localScale;
                extraObjectController.extraObjs.Add(obj);
            }
        }

        private void SubscribeAnimationEvents() {
            SkinAppliedRun += AddExpressionController;
            SkinAppliedLobby += AddExpressionController;
        }

        private void AddExpressionController(GameObject body) {
            ExpressionController expressionController = body.GetComponent<ExpressionController>();
            if (expressionController == null) {
                expressionController = body.AddComponent<ExpressionController>();
            }
            expressionController.Init(this);
        }

        private void SubscribeVoiceEvents() {
            SkinAppliedRun += AddVoiceController;
            SkinAppliedLobby += AddVoiceController;
        }

        private void AddVoiceController(GameObject body) {
            VoiceController voiceController = body.GetComponent<VoiceController>();
            if (voiceController == null) {
                voiceController = body.AddComponent<VoiceController>();
            }
            voiceController.Init(this);
        }
    }
}
