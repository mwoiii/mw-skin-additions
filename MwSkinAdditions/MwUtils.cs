using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MwSkinAdditions {
    public static class MwUtils {
        public static System.Random rand = new System.Random();

        public static object RandomChoices(IEnumerable<object> choices, IEnumerable<float> weights) {
            List<float> cumWeights = new List<float>();
            float currentWeight = 0;

            foreach (float weight in weights) {
                currentWeight += weight;
                cumWeights.Add(currentWeight);
            }

            float choiceWeight = (float)rand.NextDouble() * currentWeight;

            int i = 0;
            foreach (var choice in choices) {
                if (choiceWeight <= cumWeights[i]) {
                    return choice;
                }
                i++;
            }

            Log.Warning("RandomChoices: Couldn't make a choice - returning null!");
            return null;
        }

        public static object RandomChoice(IEnumerable<object> choices) {
            IList<object> list = choices as IList<object> ?? choices.ToList();
            if (list.Count > 0) {
                return list[rand.Next(list.Count)];
            }
            Log.Warning("RandomChoice: Got passed an empty enumerable - returning null!");
            return null;
        }

        public static object RandomChoice(object[] choices) {
            if (choices.Length > 0) {
                return choices[rand.Next(choices.Length)];
            }
            Log.Warning("RandomChoice: Got passed an empty enumerable - returning null!");
            return null;
        }

        public static void SafeSetBlendShapeWeight(this SkinnedMeshRenderer renderer, int index, float value) {
            if (renderer) {
                renderer.SetBlendShapeWeight(index, value);
            }
        }

        public static T SwapWith<T>(this T newComponent, T oldComponent) where T : MwBaseComponent {
            newComponent.OnSwapWith(oldComponent);
            return newComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Mod(float a, float b) {
            var result = a % b;
            if (result < 0) {
                result += b;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleDelta(float a, float b, AngleDirection direction) {
            if (direction == AngleDirection.AntiClockwise) {
                return Mod(b - a, 360f);
            } else {
                return Mod(a - b, 360f);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerpAngle(float a, float b, float value, AngleDirection direction) {
            var deltaB = AngleDelta(a, b, direction);
            var deltaValue = AngleDelta(a, value, direction);
            var result = 0f;
            if (!Mathf.Approximately(deltaB, 0f)) {
                if (deltaValue > 180f + (0.5f * deltaB)) { // exceeded the midpoint - wrap around to a negative value
                    result = (deltaValue - 360f) / deltaB;
                } else {
                    result = deltaValue / deltaB;
                }
            }
            return result;
        }
    }
}
