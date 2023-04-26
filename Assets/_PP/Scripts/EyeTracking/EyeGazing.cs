using UnityEngine;

namespace Meta.PP
{
    public class EyeGazing : MonoBehaviour
    {
        [SerializeField] private Transform _eyeTransform;
        [SerializeField] private float _speed = 0.5f;

        private RaycastHit _currentRaycastHit;
        private RaycastHit _previousRaycastHit;

        private void Update()
        {
            Vector3 fwd = _eyeTransform.transform.TransformDirection(Vector3.forward);
            if (Physics.Raycast(_eyeTransform.transform.position, fwd, out _currentRaycastHit, 50))
            {
                // Debug.LogError(_currentRaycastHit.transform.name);
                if ((_currentRaycastHit.point - _previousRaycastHit.point).magnitude > _speed)
                {
                    // Debug.LogError("Too fast");
                }
                _previousRaycastHit = _currentRaycastHit;
            }
        }
    }
}