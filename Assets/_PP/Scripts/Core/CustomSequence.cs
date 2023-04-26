using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.PP
{
    // parent class for individual sequences that play during a scene
    public class CustomSequence : MonoBehaviour
    {
        public bool hideChildrenOnStart = true;
        public float actionDelay = 1f;
        
        public UnityEvent OnStart;
        
        public UnityEvent OnPlaySequence;
        public UnityEvent OnEndSequence;
        
        protected bool _isWaiting = false; // action delay coroutine

        public void Start()
        {
            if (hideChildrenOnStart)
            {
                HideChildren();
            }
            OnStart?.Invoke();
        }

        public void ShowChildren()
        {
            // show all child objects for preview
            for (int i = 0; i< transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        public void HideChildren()
        {
            // hide all child objects
            for (int i = 0; i< transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        protected IEnumerator TakeAction()
        {
            if (_isWaiting)
            {
                yield return null;
            }

            _isWaiting = true;
            yield return new WaitForSeconds(actionDelay);

            TriggerAction();
            
            _isWaiting = false;
        }

        public void TriggerAction()
        {
            OnPlaySequence?.Invoke();
            Debug.Log($"[CustomSequence] PLAY SEQUENCE...{name}");
        }

        public void PlaySequence()
        {
            StartCoroutine(TakeAction());
            
            AppManager.OnStop += EndSequence; // if scene stops prematurely
        }

        public void EndSequence()
        {
            StopAllCoroutines();
            
            OnEndSequence?.Invoke();
            
            AppManager.OnStop -= EndSequence;
            
            Debug.Log($"[CustomSequence: END SEQUENCE {gameObject.name}]");
        }
        
    }

}