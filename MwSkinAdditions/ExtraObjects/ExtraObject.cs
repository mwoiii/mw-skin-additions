using UnityEngine;

namespace MwSkinAdditions {
    public class ExtraObject {

        public GameObject prefab;

        public string armatureParentPath;

        public Vector3 localScale;

        public Vector3 localPosition;

        public Vector3 localEulerAngles;

        public ExtraObject(GameObject prefab, string armatureParentPath, Vector3 localScale, Vector3 localPosition, Vector3 localEulerAngles) {
            this.prefab = prefab;
            this.armatureParentPath = armatureParentPath;
            this.localScale = localScale;
            this.localPosition = localPosition;
            this.localEulerAngles = localEulerAngles;
        }
    }
}
