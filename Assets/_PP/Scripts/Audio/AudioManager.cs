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

        private AudioTypeEnum _currentAudioTypeEnum;

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
            if (_currentAudioTypeEnum == audioEvent.AudioTypeEnum)
            {
                return;
            }

            AudioDictionary audioDictionary = _audioDictionary.FirstOrDefault(x => x.AudioTypeEnum == audioEvent.AudioTypeEnum);

            if (audioDictionary == null || audioDictionary.AudioClip == _audioSource.clip)
            {
                return;
            }

            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
                _audioSource.clip = null;
            }
            AudioClip audioClip = audioDictionary.AudioClip;
            _audioSource.clip = audioClip;
            _audioSource.Play();
            _currentAudioTypeEnum = audioEvent.AudioTypeEnum;
        }
    }
}