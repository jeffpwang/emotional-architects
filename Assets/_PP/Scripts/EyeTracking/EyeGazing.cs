using UnityEngine;

namespace Meta.PP
{
    public class EyeGazing : MonoBehaviour
    {
        [SerializeField] private Transform _eyeTransform;

        private void Update()
        {
            Vector3 fwd = _eyeTransform.transform.TransformDirection(Vector3.forward);
            if (Physics.Raycast(_eyeTransform.transform.position, fwd, out var objectHit, 50))
            {
                Debug.LogError(objectHit.transform.name);
            }
        }
    }
}