using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.PP
{
    public class PlayAudio : MonoBehaviour
    {
        [SerializeField] private AudioTypeEnum soundType;

        public void PlaySound()
        {
            Events.Raise(new AudioEvent(soundType));
        }
    }
}