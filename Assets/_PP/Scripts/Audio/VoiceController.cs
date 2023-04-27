using Meta.PP;
using Meta.WitAi.Json;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class VoiceController : MonoBehaviour
{
    [Serializable]
    public enum VoiceControllerState
    {
        AICommunication = 0,
        TextToSpeech = 1,
    }

    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private TTSSpeaker tTSSpeaker;
    [SerializeField] private VoiceControllerState _voiceControllerState;

    private bool _active;

    private void OnEnable()
    {
        appVoiceExperience.VoiceEvents.OnResponse.AddListener(OnRequestResponse);
        appVoiceExperience.VoiceEvents.OnStartListening.AddListener(OnStartListening);
        appVoiceExperience.VoiceEvents.OnStoppedListening.AddListener(OnStoppedListening);
    }

    private void OnDisable()
    {
        appVoiceExperience.VoiceEvents.OnResponse.RemoveListener(OnRequestResponse);
        appVoiceExperience.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
        appVoiceExperience.VoiceEvents.OnStoppedListening.RemoveListener(OnStoppedListening);
    }

    private void OnStoppedListening()
    {
        Debug.Log(nameof(OnStoppedListening));
    }

    private void OnStartListening()
    {
        Debug.Log(nameof(OnStartListening));
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            SetActivation(!_active);
        }
    }

    private void OnRequestResponse(WitResponseNode response)
    {
        if (!string.IsNullOrEmpty(response["text"]))
        {
            switch (_voiceControllerState)
            {
                case VoiceControllerState.AICommunication:
                    Events.Raise(new AudioVisualizerEvent(response["text"]));
                    break;
                case VoiceControllerState.TextToSpeech:
                    tTSSpeaker.Speak(response["text"]);
                    break;
            }
        }
    }

    public void SetActivation(bool toActivated)
    {
        if (_active != toActivated)
        {
            _active = toActivated;
            if (_active)
            {
                appVoiceExperience.Activate();
            }
            else
            {
                appVoiceExperience.Deactivate();
            }
        }
    }


    public void SetVoiceControllerState(VoiceControllerState voiceControllerState)
    {
        _voiceControllerState = voiceControllerState;
    }
}
