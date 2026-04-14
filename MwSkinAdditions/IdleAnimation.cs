using System;

namespace MwSkinAdditions {
    public class IdleAnimation {

        public BlendShapeAnimation[] animations;

        public Func<ExpressionController, bool> condition;

        public bool cancelOnConditionFalse;

        public IdleAnimation(BlendShapeAnimation[] animations, Func<ExpressionController, bool> condition, bool cancelOnConditionFalse = true) {
            this.animations = animations;
            this.condition = condition;
            this.cancelOnConditionFalse = cancelOnConditionFalse;
        }
    }
}
