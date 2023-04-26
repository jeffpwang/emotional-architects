using System.Collections;
using UnityEngine;

namespace Meta.PP
{
    public class CubeAnimationController : MonoBehaviour
    {
        [SerializeField] private GameObject _therapistCube;
        [SerializeField] private GameObject _patientCube;
        [SerializeField] private GameObject _combinedCube;

        private void EnableCube(GameObject target, bool enable)
        {
            target.SetActive(enable);
        }

        public void EnableTherapistCube(bool enable)
        {
            EnableCube(_therapistCube, enable);
        }

        public void EnablePatientCube(bool enable)
        {
            EnableCube(_patientCube, enable);
        }

        public void EnableCombinedCube(bool enable)
        {
            EnableCube(_combinedCube, enable);
        }
    }
}