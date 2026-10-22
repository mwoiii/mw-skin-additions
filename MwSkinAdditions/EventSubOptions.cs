using System;
using UnityEngine;

namespace MwSkinAdditions {
    public class EventSubOptions {
        public BoneTransformation[] boneTransformations = null;

        public BlendShapeAnimation[] blinkAnimations = null;

        public IdleAnimation[] conditionalIdleAnimations = null;

        public VoiceGroup[] voiceGroups = null;

        [Tooltip("Set to false to disable all bone transformations in the CSS/lobby. Use the 'contexts' flags field in TransformationOptions for more customisability.")]
        public bool transformInCSS = true;

        [Obsolete("Use in-built functionality via GameObject Activations instead.")]
        public ExtraObject[] extraObjects = null;

        [Tooltip("Set to true if you are not using blink or idle animations but wish to ensure the skin has an ExpressionController for custom animation events.")]
        public bool useAnimations = false;

        public bool isAnimated => useAnimations || forceAnimations;

        private bool forceAnimations => blinkAnimations?.Length > 0 || conditionalIdleAnimations?.Length > 0;
    }
}
