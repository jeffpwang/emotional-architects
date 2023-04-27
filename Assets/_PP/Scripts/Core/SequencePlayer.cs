using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.PP
{
    public class SequencePlayer : MonoBehaviour
    {
        [Header("Sequences")] 
        public List<CustomSequence> sequences;
        
        public void StartSequence()
        {
            AppManager.Instance.EndSequence();
            sequences[0].PlaySequence();
            
            AppManager.Instance.currentSceneSequence = sequences[0];
        }

        public void GoToNextSequence()
        {
            AppManager.Instance.MoveToNextSequence(true); 
            Debug.Log($"GOING TO NEXT FROM {gameObject.name}");
        }
    }
}
