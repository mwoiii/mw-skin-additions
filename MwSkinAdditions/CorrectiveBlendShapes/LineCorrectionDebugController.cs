using UnityEngine;

namespace MwSkinAdditions.CorrectiveBlendShapes {
    public class LineCorrectionDebugController : MonoBehaviour {

        public LineCorrection correction;

        public Transform boneTransform;

        public LineRenderer axis1Preview;

        public LineRenderer axis2Preview;

        public LineRenderer forwardPreview;

        public const float width = 0.05f;

        public void Init(CorrectionInstance instance) {
            if (!(instance.source.correctionMethod is LineCorrection)) {
                return;
            }

            boneTransform = instance.boneTransform;
            correction = instance.source.correctionMethod as LineCorrection;

            var axis1Obj = new GameObject("Axis1Preview");
            axis1Obj.transform.parent = boneTransform;
            axis1Preview = axis1Obj.AddComponent<LineRenderer>();
            //axis1Preview.startColor = Color.black;
            //axis1Preview.endColor = Color.black;
            axis1Preview.useWorldSpace = true;
            axis1Preview.startWidth = width;
            axis1Preview.endWidth = width;
            axis1Preview.positionCount = 2;

            var axis2Obj = new GameObject("Axis2Preview");
            axis2Obj.transform.parent = boneTransform;
            axis2Preview = axis2Obj.AddComponent<LineRenderer>();
            //axis2Preview.startColor = Color.black;
            //axis2Preview.endColor = Color.black;
            axis2Preview.useWorldSpace = true;
            axis2Preview.startWidth = width;
            axis2Preview.endWidth = width;
            axis2Preview.positionCount = 2;

            var forwardPreviewObj = new GameObject("ForwardPreview");
            forwardPreviewObj.transform.parent = boneTransform;
            forwardPreview = forwardPreviewObj.AddComponent<LineRenderer>();
            //forwardPreview.startColor = Color.white;
            //forwardPreview.endColor = Color.white;
            forwardPreview.useWorldSpace = true;
            forwardPreview.startWidth = width;
            forwardPreview.endWidth = width;
            forwardPreview.positionCount = 2;
        }

        public void Update() {
            axis1Preview.SetPosition(0, boneTransform.position);
            axis1Preview.SetPosition(1, boneTransform.position + boneTransform.parent.rotation * correction.point1 * 1.5f);

            axis2Preview.SetPosition(0, boneTransform.position);
            axis2Preview.SetPosition(1, boneTransform.position + boneTransform.parent.rotation * correction.point2 * 1.5f);

            forwardPreview.SetPosition(0, boneTransform.position);
            forwardPreview.SetPosition(1, boneTransform.position + boneTransform.rotation * correction.forward * 1.5f);
        }
    }
}
