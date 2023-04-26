using UnityEngine;

namespace Meta.PP
{
    public class EyeGazing : MonoBehaviour
    {
        [SerializeField] private Transform _eyeTransform;
        [SerializeField] private float _speed = 0.5f;

        private RaycastHit _currentRaycastHit;
        private RaycastHit _previousRaycastHit;

        public bool IsLooking { get; private set; }
        public bool IsMovingTooFast { get; private set; }

        private void Update()
        {
            Vector3 fwd = _eyeTransform.transform.TransformDirection(Vector3.forward);

            if (Physics.Raycast(_eyeTransform.transform.position, fwd, out _currentRaycastHit, 50)
                && _currentRaycastHit.transform.gameObject.CompareTag("TargetOrb"))
            {
                IsLooking = true;
                IsMovingTooFast = (_currentRaycastHit.point - _previousRaycastHit.point).magnitude > _speed;
                _previousRaycastHit = _currentRaycastHit;
            }
            else
            {
                IsLooking = false;
                IsMovingTooFast = false;
            }
        }
    }
}