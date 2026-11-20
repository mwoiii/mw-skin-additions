using UnityEngine;

namespace MwSkinAdditions {
    public abstract class MwBaseComponent : MonoBehaviour {
        public bool beingDeleted { get; private set; }

        public virtual void OnSwapWith<T>(T oldComponent) where T : MwBaseComponent { }

        public abstract void Init(EventSub eventSub);

        public virtual void MarkForDeletion() {
            beingDeleted = true;
            Destroy(this);
        }
    }
}
