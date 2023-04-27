using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Meta.PP
{
    public class CubeAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator _merge;
        [SerializeField] private Animator _start;
        [SerializeField] private Animator _therapy;
        [SerializeField] private Animator _split;
        [SerializeField] private GazeController _gazeController;
        [SerializeField] private float _therapyLoop;
        [SerializeField] private Transform _timerBar;

        private float _startingScaleX;

        private void Start()
        {
            StartTherapy();
        }

        public void StartTherapy()
        {
            StartCoroutine(AnimationSequence());
            StartCoroutine(TimeBarSequence());
        }

        private IEnumerator AnimationSequence()
        {
            yield return new WaitForSeconds(1);
            EnableMergeCube(true);
            yield return new WaitForSeconds(GetAnimationLenght(_merge) - 0.05f);

            EnableMergeCube(false);
            EnableStartCube(true);
            yield return new WaitForSeconds(GetAnimationLenght(_start) - 0.05f);

            _gazeController.IsActive = true;
            EnableStartCube(false);
            EnableTherapyCube(true);
            yield return new WaitForSeconds((GetAnimationLenght(_therapy) * _therapyLoop) - 0.05f);

            _gazeController.IsActive = false;
            EnableTherapyCube(false);
            EnableSplitCube(true);
            yield return new WaitForSeconds(GetAnimationLenght(_split) - 0.05f);
            EnableSplitCube(false);
        }

        private IEnumerator TimeBarSequence()
        {
            yield return new WaitForSeconds(1);
            _timerBar.gameObject.SetActive(true);
            _startingScaleX = _timerBar.localScale.x;
            float timer = 0;

            float lenght = 17.5f;

            while (timer < lenght)
            {
                var increment = Time.deltaTime;
                timer += Time.deltaTime;
                var newScale = (1 - timer / lenght) * _startingScaleX;
                _timerBar.localScale = new Vector3(newScale, _timerBar.localScale.y, _timerBar.localScale.z);
                yield return new WaitForSeconds(increment);
            }
            _timerBar.gameObject.SetActive(false);
        }

        private void EnableCube(Animator target, bool enable)
        {
            target.gameObject.SetActive(enable);
        }

        private void EnableStartCube(bool enable)
        {
            EnableCube(_start, enable);
        }

        private void EnableMergeCube(bool enable)
        {
            EnableCube(_merge, enable);
        }

        private void EnableTherapyCube(bool enable)
        {
            EnableCube(_therapy, enable);
        }

        private void EnableSplitCube(bool enable)
        {
            EnableCube(_split, enable);
        }

        private float GetAnimationLenght(Animator animator)
        {
            return animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        }
    }
}