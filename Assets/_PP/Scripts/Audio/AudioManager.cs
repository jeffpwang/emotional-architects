using System;
using System.Linq;
using UnityEngine;

namespace Meta.PP
{
    public class AudioManager : MonoBehaviour
    {
        [Serializable]
        public class AudioDictionary
        {
            public AudioClip AudioClip;
            public AudioTypeEnum AudioTypeEnum;
        }

        [SerializeField] private AudioDictionary[] _audioDictionary;
        [SerializeField] private AudioSource _audioSource;

        public bool IsPlaying()
        {
            if (_audioSource == null)
            {
                return false;
            }
            else
            {
                return _audioSource.isPlaying;   
            }
        }

        private void OnEnable()
        {
            Events.AddListener<AudioEvent>(PlayAudio);
        }

        private void OnDisable()
        {
            Events.RemoveListener<AudioEvent>(PlayAudio);
        }

        private void PlayAudio(AudioEvent audioEvent)
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
                _audioSource.clip = null;
            }

            AudioClip audioClip = _audioDictionary.First(x => x.AudioTypeEnum == audioEvent.AudioTypeEnum).AudioClip;
            _audioSource.clip = audioClip;
            _audioSource.Play();
        }
    }
}