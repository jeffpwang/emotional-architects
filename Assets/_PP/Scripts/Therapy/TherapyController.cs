using System.Collections;
using UnityEngine;

namespace Meta.PP
{
    public class TherapyController : MonoBehaviour
    {
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private PlayAudio openingAudio;

        private void Start()
        {
            StartCoroutine(TherapyFlow());
        }

        private IEnumerator TherapyFlow()
        {
            openingAudio.PlaySound();

            while (audioManager.IsPlaying())
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }
}