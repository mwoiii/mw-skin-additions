using RoR2;
using System;
using UnityEngine;

namespace MwSkinAdditions {
    public class EventSub {

        public SkinDef[] skinDefs;

        public BoneTransformation[] boneTransformations;

        public ExtraObject[] extraObjects;

        public bool useAnimations = false;

        public BlendShapeAnimation[] blinkAnimations;

        public IdleAnimation[] conditionalIdleAnimations;

        public VoiceGroup[] voiceGroups;

        public bool transformInCSS = true;


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

        public Action<GameObject> LeaveSpawnTeleporterState; // typical next stage spawn state 

        [Obsolete("Provide an EventSubOptions object instead.")]
        public EventSub(SkinDef skinDef, BoneTransformation[] boneTransformations = null, ExtraObject[] extraObjects = null,
            bool useAnimations = false, BlendShapeAnimation[] blinkAnimations = null, IdleAnimation[] conditionalIdleAnimations = null,
            VoiceGroup[] voiceGroups = null) {
            this.skinDefs = new SkinDef[] { skinDef };
            this.boneTransformations = boneTransformations;
            this.extraObjects = extraObjects;
            this.useAnimations = useAnimations;
            this.blinkAnimations = blinkAnimations;
            this.conditionalIdleAnimations = conditionalIdleAnimations;
            this.voiceGroups = voiceGroups;
        }

        public EventSub(SkinDef skinDef, EventSubOptions eventSubOptions) {
            eventSubOptions ??= new EventSubOptions();

            skinDefs = new SkinDef[] { skinDef };
            UnpackOptions(eventSubOptions);
        }

        public EventSub(SkinDef[] skinDefs, EventSubOptions eventSubOptions) {
            eventSubOptions ??= new EventSubOptions();

            this.skinDefs = skinDefs;
            UnpackOptions(eventSubOptions);
        }

        private void UnpackOptions(EventSubOptions eventSubOptions) {
            boneTransformations = eventSubOptions.boneTransformations;
            extraObjects = eventSubOptions.extraObjects;
            useAnimations = eventSubOptions.useAnimations;
            blinkAnimations = eventSubOptions.blinkAnimations;
            conditionalIdleAnimations = eventSubOptions.conditionalIdleAnimations;
            voiceGroups = eventSubOptions.voiceGroups;
            transformInCSS = eventSubOptions.transformInCSS;
        }

        public void Init() {
            SkinEvents.SubscribeEventSkins(this);
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

            # region Debug
            /*
            SkinAppliedLobby += (GameObject _) => { Log.Debug("SkinAppliedLobby event invoked!"); };
            SkinAppliedRun += (GameObject _) => { Log.Debug("SkinAppliedRun event invoked!"); };
            TakeDamage += (GameObject _, DamageReport _) => { Log.Debug("TakeDamage event invoked!"); };
            Death += (GameObject _) => { Log.Debug("Death event invoked!"); };
            DefeatBossGroup += (GameObject _) => { Log.Debug("DefeatBossGroup event invoked!"); };
            LeavePod += (GameObject _) => { Log.Debug("LeavePod event invoked!"); };
            UsePrimary += (GameObject _) => { Log.Debug("UsePrimary event invoked!"); };
            UseSecondary += (GameObject _) => { Log.Debug("UseSecondary event invoked!"); };
            UseUtility += (GameObject _) => { Log.Debug("UseUtility event invoked!"); };
            UseSpecial += (GameObject _) => { Log.Debug("UseSpecial event invoked!"); };
            ShrineSuccess += (GameObject _) => { Log.Debug("ShrineSuccess event invoked!"); };
            ShrineFailure += (GameObject _) => { Log.Debug("ShrineFailure event invoked!"); };
            TeleporterStart += (GameObject _) => { Log.Debug("TeleporterStart event invoked!"); };
            TeleporterEnd += (GameObject _) => { Log.Debug("TeleporterEnd event invoked!"); };
            BearDamageBlock += (GameObject _) => { Log.Debug("BearDamageBlock event invoked!"); };
            LevelUp += (GameObject _) => { Log.Debug("LevelUp event invoked!"); };
            MithrixDefeat += (GameObject _) => { Log.Debug("MithrixDefeat event invoked!"); };
            UseEquipment += (GameObject _) => { Log.Debug("UseEquipment event invoked!"); };
            Heal += (GameObject _, float _) => { Log.Debug("Heal event invoked!"); };
            Jump += (GameObject _) => { Log.Debug("Jump event invoked!"); };
            LeaveStage += (GameObject _) => { Log.Debug("LeaveStage event invoked!"); };
            Idle += (GameObject _) => { Log.Debug("Idle event invoked!"); };
            GetItem += (GameObject _, ItemIndex _) => { Log.Debug("GetItem event invoked!"); };
            HoldoutZoneCharged += (GameObject _) => { Log.Debug("HoldoutZoneCharged event invoked!"); };
            LeaveSpawnTeleporterState += (GameObject _) => { Log.Debug("LeaveSpawnTeleporterState event invoked!"); };
            */
            #endregion
        }

        private void SubscribeTransformEvents() {
            SkinAppliedRun += AddTransformController;
            SkinAppliedLobby += AddTransformController;
        }

        private void AddTransformController(GameObject body) {
            if (Run.instance == null && !transformInCSS) {
                return;
            }

            TransformController transformController = body.GetComponent<TransformController>();
            if (transformController == null || transformController.beingDeleted) {
                transformController = body.AddComponent<TransformController>();
            }
            transformController.Init(this);
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
