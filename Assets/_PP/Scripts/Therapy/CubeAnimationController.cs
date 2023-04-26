using System.Collections;
using UnityEngine;

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

        private void Awake()
        {
            StartCoroutine(AnimationSequence());
        }

        private IEnumerator AnimationSequence()
        {
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