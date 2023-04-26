using System.Collections;
using UnityEngine;

namespace Meta.PP
{
    public class GazeController : MonoBehaviour
    {
        [SerializeField] private float _notLookingDelay = 2;
        [SerializeField] private float _lookingTooFast = 2;

        private EyeGazing[] _eyeGazers;

        public bool IsActive { get; set; }

        private Coroutine _notLookingCoroutine;
        private Coroutine _lookingTooFastCoroutine;

        private void Awake()
        {
            _eyeGazers = FindObjectsOfType<EyeGazing>();
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            foreach (var eyeGazer in _eyeGazers)
            {
                if (!eyeGazer.IsLooking && _notLookingCoroutine == null)
                {
                    _notLookingCoroutine = StartCoroutine(UserNotLooking());
                }

                if (eyeGazer.IsMovingTooFast && _lookingTooFastCoroutine == null)
                {
                    _lookingTooFastCoroutine = StartCoroutine(UserMovingTooFast());
                }
            }
        }

        private IEnumerator UserNotLooking()
        {
            yield return new WaitForSeconds(_notLookingDelay);
            Events.Raise(new AudioEvent(AudioTypeEnum.EyesOnOrb));
            _notLookingCoroutine = null;
        }

        private IEnumerator UserMovingTooFast()
        {
            yield return new WaitForSeconds(_lookingTooFast);
            Events.Raise(new AudioEvent(AudioTypeEnum.EyesOnOrb));
            _lookingTooFastCoroutine = null;

        }
    }
}