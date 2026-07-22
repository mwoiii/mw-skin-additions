using EntityStates.Missions.BrotherEncounter;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MwSkinAdditions.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MwSkinAdditions {
    public static class SkinEvents {

        private static Dictionary<SkinDef, EventSub> skinDefToEventSub = new Dictionary<SkinDef, EventSub>();

        public static HashSet<CharacterBody> holdOffIdleInvocation = new HashSet<CharacterBody>();

        public static void Init() {
            SubscribeGlobalEvents();
            SubscribeGameEvents();
        }

        public static void SubscribeEventSkins(EventSub eventSub) {
            foreach (SkinDef skinDef in eventSub.skinDefs) {
                if (skinDef == null) {
                    Log.Error("Received a null SkinDef! Ignoring...");
                    continue;
                }
                skinDefToEventSub.Add(skinDef, eventSub);
            }
        }

        private static void SubscribeGameEvents() {
            On.RoR2.ModelSkinController.ApplySkinAsync += OnSkinAppliedBody;
            On.RoR2.TeleporterInteraction.OnInteractionBegin += OnTeleporterStart;
            On.RoR2.CharacterBody.OnDeathStart += OnDeath;
            On.RoR2.VehicleSeat.EjectPassenger_GameObject += OnLeavePod;
            On.RoR2.CharacterBody.OnSkillActivated += OnSkillActivated;
            On.RoR2.ShrineChanceBehavior.AddShrineStack += OnChanceShrineInteract;
            IL.RoR2.GenericPickupController.AttemptGrant += OnPickupAttemptGrant;
            IL.RoR2.HealthComponent.TakeDamageProcess += OnBearDamageBlock;
            BossGroup.onBossGroupDefeatedServer += OnDefeatBossGroup;
            TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterEnd;
            GlobalEventManager.onServerDamageDealt += OnTakeDamage;
            GlobalEventManager.onCharacterLevelUp += OnLevelUp;
            On.EntityStates.Missions.BrotherEncounter.EncounterFinished.OnEnter += OnMithrixDefeat;
            EquipmentSlot.onServerEquipmentActivated += OnUseEquipment;
            HealthComponent.onCharacterHealServer += OnHeal;
            On.EntityStates.GenericCharacterMain.ApplyJumpVelocity += OnJump;
            On.RoR2.SceneExitController.Begin += OnLeaveStage;
            On.RoR2.CharacterBody.Update += OnBodyUpdate;
            On.EntityStates.SpawnTeleporterState.OnExit += OnLeaveSpawnTeleporterState;
        }

        public static void SubscribeGlobalEvents() {
            EventSub.DifferentSkinAppliedGlobal += RemoveTransformController;
            EventSub.DifferentSkinAppliedGlobal += RemoveExtraObjects;
            EventSub.DifferentSkinAppliedGlobal += RemoveVoiceController;
            EventSub.DifferentSkinAppliedGlobal += RemoveExpressionController;
        }

        public static EventSub GetEventSubFromBody(GameObject body) {
            try {
                SkinDef skinDef = SkinCatalog.FindCurrentSkinDefForBodyInstance(body);
                return GetEventSubFromSkinDef(skinDef);
            } catch (NullReferenceException) {
                Log.Error("GetEventSubFromBody NRE! Returning null...");
                return null;
            }
        }

        public static EventSub GetEventSubFromSkinDef(SkinDef skinDef) {
            if (skinDef == null || !skinDefToEventSub.ContainsKey(skinDef)) {
                return null;
            }

            return skinDefToEventSub[skinDef];
        }

        private static void ForEachBodySafe(Action<GameObject> action) {
            foreach (CharacterMaster master in CharacterMaster.readOnlyInstancesList) {
                if (!master) {
                    continue;
                }
                GameObject bodyObject = master.GetBodyObject();
                if (bodyObject) {
                    action(bodyObject);
                }
            }
        }

        private static IEnumerator OnSkinAppliedBody(On.RoR2.ModelSkinController.orig_ApplySkinAsync orig, ModelSkinController self, int skinIndex, AsyncReferenceHandleUnloadType unloadType) {
            yield return orig(self, skinIndex, unloadType);

            GameObject bodyObject = null;
            EventSub bodyEventSub = null;

            if (self && self.characterModel & self.characterModel.body) {
                bodyObject = self.characterModel.body.gameObject;
            }

            if (bodyObject) {
                bodyEventSub = GetEventSubFromBody(bodyObject);
            }

            if (bodyObject && bodyEventSub != null) {
                bodyEventSub.SkinAppliedRun?.Invoke(bodyObject);
            } else if (!bodyObject && ArrayUtils.GetSafe(self.skins, self.currentSkinIndex) is SkinDef skinDef && skinDefToEventSub.ContainsKey(skinDef)) {
                bodyEventSub = GetEventSubFromSkinDef(skinDef);
                bodyEventSub.SkinAppliedLobby?.Invoke(self.gameObject);
            } else if (!bodyObject) {
                EventSub.DifferentSkinAppliedGlobal?.Invoke(self.gameObject);
            } else {
                EventSub.DifferentSkinAppliedGlobal?.Invoke(bodyObject);
            }
        }

        private static void OnTeleporterStart(On.RoR2.TeleporterInteraction.orig_OnInteractionBegin orig, TeleporterInteraction self, Interactor activator) {
            orig(self, activator);

            if (TeleporterInteraction.instance && !TeleporterInteraction.instance.isCharged) {
                if (GetEventSubFromBody(activator.gameObject) is EventSub eventSub) {
                    eventSub.TeleporterStart?.Invoke(activator.gameObject);
                }
            }
        }

        private static void OnDeath(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self) {
            orig(self);

            if (GetEventSubFromBody(self.gameObject) is EventSub eventSub) {
                eventSub.Death?.Invoke(self.gameObject);
            }
        }

        private static void OnLeavePod(On.RoR2.VehicleSeat.orig_EjectPassenger_GameObject orig, VehicleSeat self, GameObject body) {
            orig(self, body);

            if (self.isSurvivorPod && GetEventSubFromBody(body) is EventSub eventSub) {
                eventSub.LeavePod?.Invoke(body);
            }
        }

        private static void OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill) {
            orig(self, skill);

            if (GetEventSubFromBody(self.gameObject) is EventSub eventSub) {
                if (skill == self.skillLocator.primary) {
                    eventSub.UsePrimary?.Invoke(self.gameObject);
                } else if (skill == self.skillLocator.secondary) {
                    eventSub.UseSecondary?.Invoke(self.gameObject);
                } else if (skill == self.skillLocator.utility) {
                    eventSub.UseUtility?.Invoke(self.gameObject);
                } else if (skill == self.skillLocator.special) {
                    eventSub.UseSpecial?.Invoke(self.gameObject);
                }
            }
        }

        private static void OnChanceShrineInteract(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, ShrineChanceBehavior self, Interactor activator) {
            int successfulPurchaseCount = self.successfulPurchaseCount;

            orig(self, activator);

            if (GetEventSubFromBody(self.gameObject) is EventSub eventSub) {
                bool success = successfulPurchaseCount < self.successfulPurchaseCount;
                if (success) {
                    eventSub.ShrineSuccess?.Invoke(activator.gameObject);
                } else {
                    eventSub.ShrineFailure?.Invoke(activator.gameObject);
                }
            }
        }

        private static void OnBearDamageBlock(ILContext il) {
            var invokeBearBlockDelegate = new Action<HealthComponent>((HealthComponent healthComponent) => {
                if (GetEventSubFromBody(healthComponent.body.gameObject) is EventSub eventSub) {
                    eventSub.BearDamageBlock?.Invoke(healthComponent.body.gameObject);
                }
            });

            ILCursor c = new ILCursor(il);
            // tougher times
            if (c.TryGotoNext(x => x.MatchLdsfld("RoR2.HealthComponent+AssetReferences", "bearEffectPrefab")) &&
                c.TryGotoNext(x => x.MatchLdloc(out _)) &&
                c.TryGotoNext(x => x.MatchLdcI4(out _)) &&
                c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt("RoR2.EffectManager", "SpawnEffect"))
                ) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(invokeBearBlockDelegate);

                // safer spaces
                if (c.TryGotoNext(x => x.MatchLdsfld("RoR2.HealthComponent+AssetReferences", "bearVoidEffectPrefab")) &&
                    c.TryGotoNext(x => x.MatchLdloc(out _)) &&
                    c.TryGotoNext(x => x.MatchLdcI4(out _)) &&
                    c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt("RoR2.EffectManager", "SpawnEffect"))
                    ) {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate(invokeBearBlockDelegate);
                }

            } else {
                Log.Error("OnBearDamageBlock ILHook failed. Tougher Times/Safer Spaces block events will not occur");
            }
        }

        private static void OnLevelUp(CharacterBody characterBody) {
            if (characterBody && GetEventSubFromBody(characterBody.gameObject) is EventSub eventSub) {
                eventSub.LevelUp?.Invoke(characterBody.gameObject);
            }
        }

        private static void OnPickupAttemptGrant(ILContext il) {
            var invokeGetItemDelegate = new Action<CharacterBody, PickupDef>((CharacterBody body, PickupDef pickupDef) => {
                if (GetEventSubFromBody(body.gameObject) is EventSub eventSub) {
                    new SyncGetItem(body.master.bodyInstanceId, (int)pickupDef.itemIndex).Send(NetworkDestination.Clients);
                }
            });

            ILCursor c = new ILCursor(il);
            int pickupLoc = 2;
            if (c.TryGotoNext(x => x.MatchCallOrCallvirt(typeof(PickupCatalog).GetMethod("GetPickupDef")),   // grab pickupdef loc from:                                                                                                
                              x => x.MatchStloc(out pickupLoc)) &&                                           // PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupState.pickupIndex);
                c.TryGotoNext(x => x.MatchLdfld<PickupDef.GrantContext>("shouldDestroy") &&              // then match:
                c.TryGotoNext(MoveType.After, x => x.MatchStfld<GenericPickupController>("consumed")))   // consumed = context.shouldDestroy;
                ) {
                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldloc, pickupLoc);
                c.EmitDelegate(invokeGetItemDelegate);
            } else {
                Log.Error("OnPickupAttemptGrant ILHook failed. Pickup interaction related events will not occur");
            }
        }

        private static void OnDefeatBossGroup(BossGroup bossGroup) {
            if (bossGroup.gameObject.name != "BrotherEncounter, Phase 4") {
                ForEachBodySafe((GameObject bodyObject) => {
                    if (GetEventSubFromBody(bodyObject) is EventSub eventSub) {
                        eventSub.DefeatBossGroup?.Invoke(bodyObject);
                    }
                });
            }
        }

        private static void OnTeleporterEnd(TeleporterInteraction teleporterInteraction) {
            ForEachBodySafe((GameObject bodyObject) => {
                if (GetEventSubFromBody(bodyObject) is EventSub eventSub) {
                    eventSub.TeleporterEnd?.Invoke(bodyObject);
                }
            });
        }

        private static void OnTakeDamage(DamageReport damageReport) {
            if (damageReport.victimBody && damageReport.victimBody.healthComponent && damageReport.victimBody.healthComponent.health > 0) {
                if (GetEventSubFromBody(damageReport.victimBody.gameObject) is EventSub eventSub) {
                    eventSub.TakeDamage?.Invoke(damageReport.victimBody.gameObject, damageReport);
                }
            }
        }

        private static void OnMithrixDefeat(On.EntityStates.Missions.BrotherEncounter.EncounterFinished.orig_OnEnter orig, EncounterFinished self) {
            orig(self);
            ForEachBodySafe((GameObject bodyObject) => {
                if (GetEventSubFromBody(bodyObject) is EventSub eventSub) {
                    eventSub.MithrixDefeat?.Invoke(bodyObject);
                }
            });
        }

        private static void OnUseEquipment(EquipmentSlot self, EquipmentIndex index) {
            if (GetEventSubFromBody(self.characterBody.gameObject) is EventSub eventSub) {
                eventSub.UseEquipment?.Invoke(self.characterBody.gameObject);
            }
        }

        private static void OnHeal(HealthComponent self, float amount, ProcChainMask procChainMask) {
            if (self && self.body && GetEventSubFromBody(self.body.gameObject) is EventSub eventSub) {
                eventSub.Heal?.Invoke(self.body.gameObject, amount);
            }
        }

        private static void OnJump(On.EntityStates.GenericCharacterMain.orig_ApplyJumpVelocity orig, CharacterMotor characterMotor, CharacterBody characterBody, float horizontalBonus, float verticalBonus, bool vault) {
            orig(characterMotor, characterBody, horizontalBonus, verticalBonus, vault);

            if (GetEventSubFromBody(characterBody.gameObject) is EventSub eventSub) {
                eventSub.Jump?.Invoke(characterBody.gameObject);
            }
        }

        private static void OnLeaveStage(On.RoR2.SceneExitController.orig_Begin orig, SceneExitController self) {
            orig(self);
            ForEachBodySafe((GameObject bodyObject) => {
                if (GetEventSubFromBody(bodyObject) is EventSub eventSub) {
                    eventSub.LeaveStage?.Invoke(bodyObject);
                }
            });
        }

        private static void OnLeaveSpawnTeleporterState(On.EntityStates.SpawnTeleporterState.orig_OnExit orig, EntityStates.SpawnTeleporterState self) {
            orig(self);
            if (Run.instance && Run.instance.stageClearCount > 0 && self.gameObject && GetEventSubFromBody(self.gameObject) is EventSub eventSub) {
                eventSub.LeaveSpawnTeleporterState?.Invoke(self.gameObject);
            }
        }

        /// <summary>
        /// Gets the model GameObject which houses the armature.
        /// 
        /// Events that fire in the CSS will return the model GameObject, whereas events that fire in a run will return the body GameObject, which is a separate thing.
        /// Given either the model or body GameObject, this method will return the model GameObject by checking if a run is active or not.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static GameObject GetModelFromEventBody(GameObject body) {
            if (Stage.instance && body && body.TryGetComponent(out ModelLocator modelLocator) && modelLocator.modelTransform) {
                return modelLocator.modelTransform.gameObject;
            } else {
                return body;
            }
        }

        public static ExpressionController GetExpressionController(GameObject body) {
            GameObject model = GetModelFromEventBody(body);
            if (model) {
                return model.GetComponent<ExpressionController>();
            }
            return null;
        }

        public static void RemoveExtraObjects(GameObject body) {
            if (body && body.TryGetComponent(out ExtraObjectController extraObjectController)) {
                foreach (GameObject obj in extraObjectController.extraObjs) {
                    UnityEngine.Object.Destroy(obj);
                }

                UnityEngine.Object.Destroy(extraObjectController);
            }
        }

        public static void RemoveTransformController(GameObject body) {
            if (body && body.TryGetComponent(out TransformController transformController)) {
                transformController.beingDeleted = true;
                UnityEngine.Object.Destroy(transformController);
            }
        }

        public static void RemoveVoiceController(GameObject body) {
            if (body && body.TryGetComponent(out VoiceController voiceController)) {
                UnityEngine.Object.Destroy(voiceController);
            }
        }

        public static void RemoveExpressionController(GameObject body) {
            if (body && body.TryGetComponent(out ExpressionController expressionController)) {
                expressionController.CancelCurrentExpressions();
                UnityEngine.Object.Destroy(expressionController);
            }
        }

        private static void OnBodyUpdate(On.RoR2.CharacterBody.orig_Update orig, CharacterBody self) {
            orig(self);

            if (self == null) {
                return;
            }

            if (self.notMovingStopwatch > 10f && GetEventSubFromBody(self.gameObject) is EventSub eventSub) {
                if (self.notMovingStopwatch % 10f < 0.1f && !holdOffIdleInvocation.Contains(self)) {
                    eventSub.Idle?.Invoke(self.gameObject);
                    RoR2Application.instance.StartCoroutine(HoldBodyFromIdleUpdate(self));
                }
            }
        }

        private static IEnumerator HoldBodyFromIdleUpdate(CharacterBody body) {
            holdOffIdleInvocation?.Add(body);
            yield return new WaitForSeconds(1f);
            holdOffIdleInvocation?.Remove(body);
        }

        public static void InvokeGetItem(GameObject body, int itemIndex) {
            EventSub eventSub = GetEventSubFromBody(body.gameObject);
            eventSub.GetItem?.Invoke(body.gameObject, (ItemIndex)itemIndex);
        }

        public static void InvokeUseShrine(GameObject body, bool success) {
            EventSub eventSub = GetEventSubFromBody(body);
            if (success) {
                eventSub.ShrineSuccess?.Invoke(body);
            } else {
                eventSub.ShrineFailure?.Invoke(body);
            }
        }

        public static void InvokeHoldoutZoneCharged() {
            ForEachBodySafe((GameObject bodyObject) => {
                if (GetEventSubFromBody(bodyObject) is EventSub eventSub) {
                    eventSub.HoldoutZoneCharged?.Invoke(bodyObject);
                }
            });
        }
    }
}
