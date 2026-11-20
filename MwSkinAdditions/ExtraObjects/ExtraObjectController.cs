using System.Collections.Generic;
using UnityEngine;

namespace MwSkinAdditions {
    public class ExtraObjectController : MwBaseComponent {
        public List<GameObject> extraObjs = new List<GameObject>();

        public override void Init(EventSub eventSub) {
            var model = SkinEvents.GetModelFromEventBody(gameObject).transform;
            foreach (var extraObject in eventSub.options.extraObjects) {
                GameObject instance = Instantiate(extraObject.prefab);
                instance.transform.parent = model.Find(extraObject.armatureParentPath);
                instance.transform.localPosition = extraObject.localPosition;
                instance.transform.localEulerAngles = extraObject.localEulerAngles;
                instance.transform.localScale = extraObject.localScale;
                extraObjs.Add(instance);
            }
        }

        public override void MarkForDeletion() {
            foreach (GameObject obj in extraObjs) {
                Destroy(obj);
            }
            base.MarkForDeletion();
        }
    }
}
