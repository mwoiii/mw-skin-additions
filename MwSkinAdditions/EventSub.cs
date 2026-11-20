using HG;
using RoR2;
using System;
using UnityEngine;

namespace MwSkinAdditions {
    public class EventSub {

        public SkinDef skinDef {
            get {
                if (skinDefs != null) {
                    return skinDefs[0];
                }
                return null;
            }
            set {
                if (skinDefs != null && skinDefs.Length >= 1) {
                    skinDefs[0] = value;
                } else {
                    skinDefs = [value];
                }
            }
        }

        public SkinDef[] skinDefs;

        public EventSubOptions options;


        public static Action<GameObject> DifferentSkinAppliedGlobal;

        private Action<GameObject> SkinAppliedLobbyEarly; // ensure components are added before any event stuff fires

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

        public EventSub() {
            options ??= new EventSubOptions();
            SkinAppliedLobby += (GameObject body) => {
                SkinAppliedLobbyEarly?.Invoke(body);
            };
        }

        [Obsolete("Provide an EventSubOptions object instead.")]
        public EventSub(SkinDef skinDef, BoneTransformation[] boneTransformations = null, ExtraObject[] extraObjects = null,
            bool useAnimations = false, BlendShapeAnimation[] blinkAnimations = null, IdleAnimation[] conditionalIdleAnimations = null,
            VoiceGroup[] voiceGroups = null) : this() {
            this.skinDef = skinDef;
            options.boneTransformations = boneTransformations;
            options.extraObjects = extraObjects;
            options.blinkAnimations = blinkAnimations;
            options.conditionalIdleAnimations = conditionalIdleAnimations;
            options.voiceGroups = voiceGroups;
        }

        public EventSub(SkinDef skinDef, EventSubOptions eventSubOptions) : this() {
            if (eventSubOptions != null) {
                options = eventSubOptions;
            }
            this.skinDef = skinDef;
        }

        public EventSub(SkinDef[] skinDefs, EventSubOptions eventSubOptions) : this() {
            if (eventSubOptions != null) {
                options = eventSubOptions;
            }
            this.skinDefs = skinDefs;
        }

        public void Init() {
            SkinEvents.Register(this);

            if (options?.boneTransformations != null && (Stage.instance || options.transformInCSS)) {
                SubscribeAddMwComponent<TransformController>();
            }

            if (options?.extraObjects != null) {
                SubscribeAddMwComponent<ExtraObjectController>();
            }

            if (options?.isAnimated == true) {
                SubscribeAddMwComponent<ExpressionController>();
            }

            if (options?.voiceGroups != null) {
                SubscribeAddMwComponent<VoiceController>();
            }

            if (options?.correctiveBlendShapes != null) {
                SubscribeAddMwComponent<CorrectionController>();
            }

            if (options?.customIcons != null) {
                SkinAppliedRun += SwapBodyIconRun;
            }

            # region Debug
            /*
            SkinAppliedLobbyEarly += (GameObject _) => { Log.Debug("SkinAppliedLobbyEarly event invoked!"); };
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

        private void SubscribeAddMwComponent<T>() where T : MwBaseComponent {
            SkinAppliedRun += AddMwComponent<T>;
            SkinAppliedLobbyEarly += AddMwComponent<T>;
        }

        private void AddMwComponent<T>(GameObject body) where T : MwBaseComponent {
            if (!body.TryGetComponent(out T mwComponent) || mwComponent.beingDeleted) {
                mwComponent = body.AddComponent<T>().SwapWith(mwComponent);
            }
            mwComponent.Init(this);
        }

        private static void SwapBodyIconRun(GameObject body) {
            if (body.TryGetComponent(out CharacterBody characterBody) &&
                characterBody.modelLocator &&
                characterBody.modelLocator.modelTransform &&
                characterBody.modelLocator.modelTransform.TryGetComponent(out ModelSkinController skinController) &&
                ArrayUtils.GetSafe(skinController.skins, skinController.currentSkinIndex).TryGetCustomIcon(out Texture icon)
               ) {
                characterBody.portraitIcon = icon;
            }
        }
    }
}
