using RoR2;
using RoR2.Audio;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace MwSkinAdditions {

    public class VoiceController : MonoBehaviour {

        private float timeSinceLastRolledLine;

        private const float soundWaitPadding = 0.5f;

        // time to wait between lines to avoid overlap, set by the VoiceInfo's approxDuration
        private float currentSoundWait;

        // stopwatches adding deltatime every frame for sound probability
        private Dictionary<VoiceGroup, float> voiceGroupStopwatches = new Dictionary<VoiceGroup, float>();

        // the previous index of the played sound, stored so that unique voicelines can play sequentially
        private Dictionary<VoiceArray, int> lastPlayedIndex = new Dictionary<VoiceArray, int>();

        public void Init(EventSub eventSub) {
            foreach (VoiceGroup voiceGroup in eventSub.voiceGroups) {
                voiceGroupStopwatches.Add(voiceGroup, 0f);
            }
        }

        private void Awake() {
            if (!NetworkServer.active) {
                Destroy(this);
            }
        }

        private void Update() {
            float time = Time.deltaTime;
            foreach (VoiceGroup key in voiceGroupStopwatches.Keys.ToArray()) {
                voiceGroupStopwatches[key] += time;
            }
            timeSinceLastRolledLine += time;
        }

        public bool RollForSoundEvent(VoiceArray soundArray, float maxProbability = 1f) {
            if (timeSinceLastRolledLine < currentSoundWait) {
                return false;
            }

            float time = voiceGroupStopwatches[soundArray.group];

            float diff = soundArray.group.maxWait - soundArray.group.minWait;

            // Not Today!!!!
            float chance;
            if (diff == 0f) {
                chance = 1f;
            } else {
                chance = Mathf.Min(maxProbability, (time - soundArray.group.minWait) / diff);
            }

            // final roll; resetting elapsed time if successful
            if (time > soundArray.group.minWait && Random.value <= chance) {
                voiceGroupStopwatches[soundArray.group] = 0f;
                timeSinceLastRolledLine = 0f;
                return true;
            }

            return false;
        }

        public void TryPlayRandomUniqueSoundServer(VoiceArray soundArray, GameObject source, float maxProbability = 1f) {
            if (!NetworkServer.active) {
                return;
            }

            if (RollForSoundEvent(soundArray, maxProbability)) {
                int indexRoll;
                if (lastPlayedIndex.ContainsKey(soundArray)) {
                    indexRoll = MwUtils.rand.Next(soundArray.voiceLines.Length - 1);
                    if (indexRoll >= lastPlayedIndex[soundArray]) {
                        indexRoll++;
                        indexRoll %= soundArray.voiceLines.Length;
                    }
                } else {
                    indexRoll = MwUtils.rand.Next(soundArray.voiceLines.Length);
                    lastPlayedIndex.Add(soundArray, indexRoll);
                }
                lastPlayedIndex[soundArray] = indexRoll;
                PlaySoundServer(soundArray.voiceLines[indexRoll], source, this);
            }
        }

        public void TryPlayRandomSoundServer(VoiceArray soundArray, GameObject source, float maxProbability = 1f) {
            if (!NetworkServer.active) {
                return;
            }

            if (RollForSoundEvent(soundArray, maxProbability)) {
                PlayRandomSoundServer(soundArray, source, this);
            }
        }

        public void TryPlayRandomSoundServer(VoiceArray soundArray, Vector3 position, float maxProbability = 1f) {
            if (!NetworkServer.active) {
                return;
            }

            if (RollForSoundEvent(soundArray, maxProbability)) {
                PlayRandomSoundServer(soundArray, position, this);
            }
        }

        public void TryPlayRandomSound(VoiceArray soundArray, GameObject source, float maxProbability = 1f) {
            if (RollForSoundEvent(soundArray, maxProbability)) {
                PlayRandomSound(soundArray, source, this);
            }
        }

        public void TryPlayRandomSound(VoiceArray soundArray, Vector3 position, float maxProbability = 1f) {
            if (RollForSoundEvent(soundArray, maxProbability)) {
                PlayRandomSound(soundArray, position, this);
            }
        }

        public static void PlayRandomSoundServer(VoiceArray soundArray, GameObject source, VoiceController voiceController = null) {
            if (!NetworkServer.active) {
                return;
            }

            VoiceInfo voiceInfo = (VoiceInfo)MwUtils.RandomChoice(soundArray.voiceLines);
            PlaySoundServer(voiceInfo, source, voiceController);
        }

        public static void PlayRandomSoundServer(VoiceArray soundArray, Vector3 position, VoiceController voiceController = null) {
            if (!NetworkServer.active) {
                return;
            }

            VoiceInfo voiceInfo = (VoiceInfo)MwUtils.RandomChoice(soundArray.voiceLines);
            PlaySoundServer(voiceInfo, position, voiceController);
        }

        public static void PlayRandomSound(VoiceArray soundArray, GameObject source, VoiceController voiceController = null) {
            VoiceInfo voiceInfo = (VoiceInfo)MwUtils.RandomChoice(soundArray.voiceLines);
            PlaySound(voiceInfo, source, voiceController);
        }

        public static void PlayRandomSound(VoiceArray soundArray, Vector3 position, VoiceController voiceController = null) {
            VoiceInfo voiceInfo = (VoiceInfo)MwUtils.RandomChoice(soundArray.voiceLines);
            PlaySound(voiceInfo, position, voiceController);
        }

        public static void PlaySoundServer(VoiceInfo voiceInfo, GameObject source, VoiceController voiceController = null) {
            if (!NetworkServer.active) {
                return;
            }

            EntitySoundManager.EmitSoundServer(voiceInfo.sound.akId, source);
            if (voiceController != null) {
                voiceController.currentSoundWait = voiceInfo.approxDuration + soundWaitPadding;
            }
        }

        public static void PlaySoundServer(VoiceInfo voiceInfo, Vector3 position, VoiceController voiceController = null) {
            if (!NetworkServer.active) {
                return;
            }

            EffectManager.SimpleSoundEffect(voiceInfo.sound.index, position, true);
            if (voiceController != null) {
                voiceController.currentSoundWait = voiceInfo.approxDuration + soundWaitPadding;
            }
        }

        public static void PlaySound(VoiceInfo voiceInfo, GameObject source, VoiceController voiceController = null) {
            EntitySoundManager.EmitSoundLocal(voiceInfo.sound.akId, source);
            if (voiceController != null) {
                voiceController.currentSoundWait = voiceInfo.approxDuration + soundWaitPadding;
            }
        }

        public static void PlaySound(VoiceInfo voiceInfo, Vector3 position, VoiceController voiceController = null) {
            EffectManager.SimpleSoundEffect(voiceInfo.sound.index, position, false);
            if (voiceController != null) {
                voiceController.currentSoundWait = voiceInfo.approxDuration + soundWaitPadding;
            }
        }
    }
}
