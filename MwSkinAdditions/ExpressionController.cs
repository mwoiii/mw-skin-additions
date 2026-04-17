using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MwSkinAdditions {
    public class ExpressionController : MonoBehaviour {

        private EventSub eventSub;

        private CharacterBody cachedCharacterBody;

        public CharacterBody characterBody {
            get {
                if (cachedCharacterBody != null) {
                    return cachedCharacterBody;
                }
                cachedCharacterBody = GetComponent<CharacterBody>();
                return cachedCharacterBody;
            }
        }

        private Dictionary<string, SkinnedMeshRenderer> cachedRenderers = new Dictionary<string, SkinnedMeshRenderer>();

        private Dictionary<string, int> cachedBlendShapes = new Dictionary<string, int>();

        private Dictionary<int, ExpressionState> featureStates = new Dictionary<int, ExpressionState>();

        private Dictionary<int, bool> featureActive = new Dictionary<int, bool>();

        public bool inDeathState;

        private float blinkInterval;

        private float blinkStopwatch;

        private bool doubleBlink;

        private int blinkStoppers;

        private class ExpressionState {
            public Coroutine coroutine;
            public BlendShapeAnimation animation;
            public SkinnedMeshRenderer skinnedMeshRenderer;
            public bool finished;
        }
        public void Init(EventSub eventSub) {
            this.eventSub = eventSub;
            eventSub.Death += (GameObject _) => { inDeathState = true; };
        }

        public void TryPlayAnimation(BlendShapeAnimation animation) {
            SkinnedMeshRenderer renderer = GetSkinnedMeshRenderer(animation.meshName);

            if (renderer != null) {
                TrySetExpressionRoutine(animation, renderer);
            }
        }

        private SkinnedMeshRenderer GetSkinnedMeshRenderer(string meshName) {
            if (cachedRenderers.ContainsKey(meshName) && cachedRenderers[meshName] is SkinnedMeshRenderer cachedRenderer) {
                return cachedRenderer;
            }

            SkinnedMeshRenderer renderer = SkinEvents.GetModelFromEventBody(gameObject)?.transform.Find(meshName)?.GetComponent<SkinnedMeshRenderer>();
            cachedRenderers[meshName] = renderer;
            return renderer;
        }

        private void TrySetExpressionRoutine(BlendShapeAnimation animation, SkinnedMeshRenderer renderer) {
            ExpressionState currentExpressionState = GetFeatureState(animation.feature);

            if (currentExpressionState != null && animation.priority > currentExpressionState.animation.priority) {
                CancelExpression(currentExpressionState);
            }

            if (GetFeatureActive(animation.feature) == false) {
                ExpressionState expressionState = new ExpressionState {
                    animation = animation,
                    skinnedMeshRenderer = renderer,
                };
                expressionState.coroutine = StartCoroutine(ExpressionRoutine(animation, renderer, expressionState));
                featureStates[animation.feature] = expressionState;

                if (animation.blockBlinking) {
                    blinkStoppers += 1;
                }
            }
        }

        private IEnumerator ExpressionRoutine(BlendShapeAnimation animation, SkinnedMeshRenderer renderer, ExpressionState expressionState) {

            float stopwatch = 0f;

            featureActive[animation.feature] = true;

            int index = GetBlendShapeIndex(animation, renderer);

            while (stopwatch < animation.fadeInDuration) {
                stopwatch += Time.deltaTime;
                renderer.SetBlendShapeWeight(index, Mathf.Lerp(0f, 100f, stopwatch / animation.fadeInDuration));
                yield return null;
            }

            renderer.SetBlendShapeWeight(index, 100f);

            yield return new WaitForSeconds(animation.holdDuration);
            stopwatch = 0;

            while (stopwatch < animation.fadeOutDuration) {
                stopwatch += Time.deltaTime;
                renderer.SetBlendShapeWeight(index, Mathf.Lerp(100f, 0f, stopwatch / animation.fadeOutDuration));
                yield return null;
            }

            renderer.SetBlendShapeWeight(index, 0f);
            featureActive[animation.feature] = false;
            if (animation.blockBlinking) {
                blinkStoppers -= 1;
            }

            expressionState.finished = true;
        }

        public void CancelCurrentExpressions() {
            foreach (ExpressionState expressionState in featureStates.Values) {
                if (expressionState != null) {
                    CancelExpression(expressionState);
                }
            }
        }

        private void CancelExpression(ExpressionState expressionState) {
            StopCoroutine(expressionState.coroutine);
            featureActive[expressionState.animation.feature] = false;
            StartCoroutine(BlendToZero(expressionState));
            if (!expressionState.finished && expressionState.animation.blockBlinking) {
                blinkStoppers -= 1;
                expressionState.finished = true;
            }
        }

        private IEnumerator BlendToZero(ExpressionState expressionState) {
            int index = GetBlendShapeIndex(expressionState.animation, expressionState.skinnedMeshRenderer);
            float startWeight = expressionState.skinnedMeshRenderer.GetBlendShapeWeight(index);

            float stopwatch = 0f;

            while (stopwatch < expressionState.animation.fadeOutDuration) {
                stopwatch += Time.deltaTime;
                expressionState.skinnedMeshRenderer.SetBlendShapeWeight(index, Mathf.Lerp(startWeight, 0f, stopwatch / expressionState.animation.fadeOutDuration));
                yield return null;
            }
        }

        private ExpressionState GetFeatureState(int feature) {
            if (featureStates.ContainsKey(feature)) {
                return featureStates[feature];
            }
            return null;
        }

        private bool GetFeatureActive(int feature) {
            if (featureActive.ContainsKey(feature)) {
                return featureActive[feature];
            }
            return false;
        }

        private int GetBlendShapeIndex(BlendShapeAnimation animation, SkinnedMeshRenderer skinnedMeshRenderer) {
            if (cachedBlendShapes.ContainsKey(animation.blendShapeName)) {
                return cachedBlendShapes[animation.blendShapeName];
            }
            int index = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(animation.blendShapeName);
            cachedBlendShapes[animation.blendShapeName] = index;
            return index;
        }

        private void Update() {
            if (eventSub.blinkAnimations != null && blinkStoppers <= 0) {
                BlinkUpdate();
            }

            if (eventSub.conditionalIdleAnimations != null) {
                IdleUpdate();
            }
        }

        private void BlinkUpdate() {
            blinkStopwatch += Time.deltaTime;

            // try blinking if surpassed the previously specified interval
            if (blinkStopwatch >= blinkInterval) {
                if (!inDeathState) {
                    foreach (BlendShapeAnimation animation in eventSub.blinkAnimations) {
                        TryPlayAnimation(animation);
                    }

                    // if this was a double blink, toggle off and don't roll, else roll for one
                    if (doubleBlink) {
                        doubleBlink = false;
                    } else if (UnityEngine.Random.value <= 0.1f) {
                        doubleBlink = true;
                    }

                } else {
                    doubleBlink = false;
                }

                // reset the stopwatch and create a new interval depending on if it's a double blink or not
                blinkStopwatch = 0f;

                if (doubleBlink) {
                    blinkInterval = 0.3f;
                } else {
                    blinkInterval = UnityEngine.Random.Range(3f, 8f);
                }
            }
        }

        private void IdleUpdate() {
            foreach (IdleAnimation idleAnimation in eventSub.conditionalIdleAnimations) {
                if (idleAnimation != null && idleAnimation.condition != null) {
                    bool check = idleAnimation.condition(this);
                    if (check) {
                        foreach (BlendShapeAnimation animation in idleAnimation.animations) {
                            TryPlayAnimation(animation);
                        }
                    } else if (idleAnimation.cancelOnConditionFalse) {
                        foreach (BlendShapeAnimation animation in idleAnimation.animations) {
                            ExpressionState currentExpressionState = GetFeatureState(animation.feature);
                            if (GetFeatureActive(animation.feature) && currentExpressionState?.animation == animation) {
                                CancelExpression(currentExpressionState);
                            }
                        }
                    }
                }
            }
        }
    }
}
