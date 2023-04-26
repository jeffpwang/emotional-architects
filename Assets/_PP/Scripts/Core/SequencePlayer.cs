using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.PP
{
    public class SequencePlayer : MonoBehaviour
    {
        public string sceneId;
        public bool autoPlay = false;

        public float delayBeforePlay = 3f;
        
        [Header("Sequences")] 
        public List<CustomSequence> sequences;

        private void OnDisable()
        {
            AppManager.OnPlay -= PlayScene;
            AppManager.OnStop -= EndScene;
        }

        public void SetupScene()
        {
            // begin listening for play controls
            AppManager.OnPlay += PlayScene;
        }

        public void PlayScene()
        {
            // activate all sequences
            foreach (CustomSequence sequence in sequences)
            {
                sequence.HideChildren();
            }
            
            // play scene automatically
            if (autoPlay)
            {
                StartCoroutine(PlayAfterDelay());
            }
            
            // begin listening to stop controls
            AppManager.OnPlay -= PlayScene;
            AppManager.OnStop += EndScene;
        }
        
        public void EndScene()
        {
            StopAllCoroutines();
            
            // turn off all sequences and show previews
            foreach (CustomSequence sequence in sequences)
            {
                sequence.ShowChildren();
            }
            
            AppManager.OnStop -= EndScene;
        }

        IEnumerator PlayAfterDelay()
        {
            yield return new WaitForSeconds(delayBeforePlay);
            
            AppManager.Instance.MoveToNextSequence(true);
            Debug.Log($"[CustomScene] Playing scene {gameObject.name}");
        }
    }
}
