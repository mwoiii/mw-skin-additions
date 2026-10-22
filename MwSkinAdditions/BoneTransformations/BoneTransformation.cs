using System;
using UnityEngine;

namespace MwSkinAdditions {
    public class BoneTransformation {
        public string armaturePath;

        public Vector3 localScale;

        public Vector3 position;

        public string relativeBonePath;

        public TransformContexts contexts = TransformContexts.All;

        public BoneTransformation(string armaturePath, Vector3 localScale, Vector3 position, string relativeBonePath = null) {
            this.armaturePath = armaturePath;
            this.localScale = localScale;
            this.position = position;
            this.relativeBonePath = relativeBonePath;
        }

        public BoneTransformation(string armaturePath, Vector3 localScale, Vector3 position, TransformContexts contexts, string relativeBonePath = null)
            : this(armaturePath, localScale, position, relativeBonePath) {
            this.contexts = contexts;
        }

        [Obsolete("Use the overload with TransformContexts instead.")]
        public BoneTransformation(string armaturePath, Vector3 localScale, Vector3 position, bool applyInLobby, string relativeBonePath = null)
            : this(armaturePath, localScale, position, relativeBonePath) {
            contexts |= TransformContexts.CSS;
        }
    }
}
