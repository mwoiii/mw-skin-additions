using EntityStates.Missions.BrotherEncounter;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MwSkinAdditions {
    public static class SkinEvents {

        private static Dictionary<SkinDef, EventSub> skinDefToEventSub = new Dictionary<SkinDef, EventSub>();

        public static HashSet<CharacterBody> holdOffIdleInvocation = new HashSet<CharacterBody>();

        public static void Init() {
            SubscribeGameEvents();
            SubscribeGlobalEvents();
        }

        public static void SubscribeEventSkin(EventSub eventSkin) {
            skinDefToEventSub.Add(eventSkin.skinDef, eventSkin);
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
        }

        public static void SubscribeGlobalEvents() {
            EventSub.DifferentSkinAppliedGlobal += RemoveTransformController;
            EventSub.DifferentSkinAppliedGlobal += RemoveExtraObjects;
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

        private static IEnumerator OnSkinAppliedBody(On.RoR2.ModelSkinController.orig_ApplySkinAsync orig, ModelSkinController self, int skinIndex, AsyncReferenceHandleUnloadType unloadType) {
            yield return orig(self, skinIndex, unloadType);

            GameObject bodyObject = self?.characterModel?.body?.gameObject;
            EventSub bodyEventSub = null;
            if (bodyObject != null) {
                bodyEventSub = GetEventSubFromBody(bodyObject);
            }

            if (bodyObject != null && bodyEventSub != null) {
                bodyEventSub.SkinAppliedRun?.Invoke(bodyObject);
            } else if (bodyObject == null && ArrayUtils.GetSafe(self.skins, self.currentSkinIndex) is SkinDef skinDef && skinDefToEventSub.ContainsKey(skinDef)) {
                bodyEventSub = GetEventSubFromSkinDef(skinDef);
                bodyEventSub.SkinAppliedLobby?.Invoke(self.gameObject);
            } else if (bodyObject != null) {
                EventSub.DifferentSkinAppliedGlobal?.Invoke(bodyObject);
            } else {
                EventSub.DifferentSkinAppliedGlobal?.Invoke(self.gameObject);
            }
        }

        private static void OnTeleporterStart(On.RoR2.TeleporterInteraction.orig_OnInteractionBegin orig, TeleporterInteraction self, Interactor activator) {
            orig(self, activator);

            if (TeleporterInteraction.instance?.isCharged == false) {
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
            GameObject body = characterBody?.gameObject;

            if (GetEventSubFromBody(body) is EventSub eventSub) {
                eventSub.LevelUp?.Invoke(body);
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
                foreach (CharacterMaster master in CharacterMaster.readOnlyInstancesList) {
                    GameObject bodyObject = master?.GetBodyObject();
                    if (bodyObject != null) {
                        if (GetEventSubFromBody(bodyObject) is EventSub eventSub) {
                            eventSub.DefeatBossGroup?.Invoke(bodyObject);
                        }
                    }
                }
            }
        }

        private static void OnTeleporterEnd(TeleporterInteraction teleporterInteraction) {
            foreach (CharacterMaster master in CharacterMaster.readOnlyInstancesList) {
                GameObject bodyObject = master?.GetBodyObject();
                if (bodyObject != null) {
                    if (GetEventSubFromBody(bodyObject) is EventSub eventSub) {
                        eventSub.TeleporterEnd?.Invoke(bodyObject);
                    }
                }
            }
        }

        private static void OnTakeDamage(DamageReport damageReport) {
            if (damageReport?.victimBody != null) {
                if (GetEventSubFromBody(damageReport?.victimBody?.gameObject) is EventSub eventSub) {
                    eventSub.TakeDamage?.Invoke(damageReport?.victimBody?.gameObject, damageReport);
                }
            }
        }

        private static void OnMithrixDefeat(On.EntityStates.Missions.BrotherEncounter.EncounterFinished.orig_OnEnter orig, EncounterFinished self) {
            orig(self);

            foreach (CharacterMaster master in CharacterMaster.readOnlyInstancesList) {
                GameObject bodyObject = master?.GetBodyObject();
                if (bodyObject != null) {
                    if (GetEventSubFromBody(bodyObject) is EventSub eventSub) {
                        eventSub.MithrixDefeat?.Invoke(bodyObject);
                    }
                }
            }
        }

        private static void OnUseEquipment(EquipmentSlot self, EquipmentIndex index) {
            if (GetEventSubFromBody(self.characterBody.gameObject) is EventSub eventSub) {
                eventSub.UseEquipment?.Invoke(self.characterBody.gameObject);
            }
        }

        private static void OnHeal(HealthComponent self, float amount, ProcChainMask procChainMaskn) {
            GameObject body = self?.body?.gameObject;

            if (GetEventSubFromBody(body) is EventSub eventSub) {
                eventSub.Heal?.Invoke(body, amount);
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

            foreach (CharacterMaster master in CharacterMaster.readOnlyInstancesList) {
                GameObject bodyObject = master?.GetBodyObject();
                if (bodyObject != null) {
                    if (GetEventSubFromBody(bodyObject) is EventSub eventSub) {
                        eventSub.LeaveStage?.Invoke(bodyObject);
                    }
                }
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
            if (Run.instance != null && body) {
                return body.GetComponent<ModelLocator>()?.modelTransform?.gameObject;
            } else {
                return body;
            }
        }

        public static ExpressionController GetExpressionController(GameObject body) {
            return GetModelFromEventBody(body)?.GetComponent<ExpressionController>();
        }

        public static void RemoveExtraObjects(GameObject body) {
            ExtraObjectController extraObjectController = body.GetComponent<ExtraObjectController>();

            if (extraObjectController != null) {
                foreach (GameObject obj in extraObjectController.extraObjs) {
                    UnityEngine.Object.Destroy(obj);
                }

                UnityEngine.Object.Destroy(extraObjectController);
            }
        }

        public static void RemoveTransformController(GameObject body) {
            TransformController transformController = body?.GetComponent<TransformController>();
            if (transformController != null) {
                UnityEngine.Object.Destroy(transformController);
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
            foreach (CharacterMaster master in CharacterMaster.readOnlyInstancesList) {
                GameObject bodyObject = master?.GetBodyObject();
                if (bodyObject != null) {
                    if (GetEventSubFromBody(bodyObject) is EventSub eventSub) {
                        eventSub.HoldoutZoneCharged?.Invoke(bodyObject);
                    }
                }
            }
        }

        public class SyncGetItem : INetMessage {

            NetworkInstanceId netInstanceId;
            int itemIndex;

            public SyncGetItem() {
            }

            public SyncGetItem(NetworkInstanceId netInstanceId, int itemIndex) {
                this.netInstanceId = netInstanceId;
                this.itemIndex = itemIndex;
            }

            public void Serialize(NetworkWriter writer) {
                writer.Write(netInstanceId);
                writer.Write(itemIndex);
            }

            public void Deserialize(NetworkReader reader) {
                netInstanceId = reader.ReadNetworkId();
                itemIndex = reader.ReadInt32();
            }

            public void OnReceived() {
                GameObject body = Util.FindNetworkObject(netInstanceId);
                if (body != null) {
                    InvokeGetItem(body, itemIndex);
                }
            }
        }

        public class SyncUseShrine : INetMessage {

            NetworkInstanceId netInstanceId;
            bool success;

            public SyncUseShrine() {
            }

            public SyncUseShrine(NetworkInstanceId netInstanceId, bool success) {
                this.netInstanceId = netInstanceId;
                this.success = success;
            }

            public void Serialize(NetworkWriter writer) {
                writer.Write(netInstanceId);
                writer.Write(success);
            }

            public void Deserialize(NetworkReader reader) {
                netInstanceId = reader.ReadNetworkId();
                success = reader.ReadBoolean();
            }

            public void OnReceived() {
                GameObject body = Util.FindNetworkObject(netInstanceId);
                if (body != null) {
                    InvokeUseShrine(body, success);
                }
            }
        }

        public class SyncHoldoutZoneCharged : INetMessage {

            public void Serialize(NetworkWriter writer) {
            }

            public void Deserialize(NetworkReader reader) {
            }

            public void OnReceived() {
                InvokeHoldoutZoneCharged();
            }
        }
    }
}
