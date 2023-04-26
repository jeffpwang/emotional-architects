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

        public void GoToNextSequence()
        {
            AppManager.Instance.MoveToNextSequence(true);
        }
    }
}
