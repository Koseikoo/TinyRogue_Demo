using DG.Tweening;
using UnityEngine;

namespace Views
{
    public class MoveCameraView : MonoBehaviour
    {
        private Vector3 _localStartPosition;
        private Quaternion _startRotation;
        
        private void Start()
        {
            var camTransform = UIHelper.Camera.transform;
            _localStartPosition = camTransform.localPosition;
            _startRotation = camTransform.rotation;
        }

        public void SetCameraTransform(Transform reference)
        {
            var camTransform = UIHelper.Camera.transform;

            Sequence moveSequence = DOTween.Sequence();

            moveSequence.Insert(0, camTransform.DOMove(reference.position, .4f))
                .Insert(0f, camTransform.DORotateQuaternion(reference.rotation, .4f));
        }

        public void ResetCameraTransform()
        {
            var camTransform = UIHelper.Camera.transform;

            Sequence moveSequence = DOTween.Sequence();

            moveSequence.Insert(0, camTransform.DOLocalMove(_localStartPosition, .4f))
                .Insert(0f, camTransform.DORotateQuaternion(_startRotation, .4f));
        }
    }
}