using System.Collections.Generic;
using UnityEngine;

namespace MwSkinAdditions {
    public class ExtraObjectController : MonoBehaviour {
        public List<GameObject> extraObjs;

        public void Awake() {
            extraObjs = new List<GameObject>();
        }
    }
}
