using Meta.PP;
using Meta.WitAi.Json;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExampleVoiceScript : MonoBehaviour
{
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private TTSSpeaker tTSSpeaker;

    private bool _active;

    private void OnEnable()
    {
        appVoiceExperience.VoiceEvents.OnResponse.AddListener(OnRequestResponse);
    }

    private void OnDisable()
    {
        appVoiceExperience.VoiceEvents.OnResponse.RemoveListener(OnRequestResponse);
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
            tTSSpeaker.Speak(response["text"]);
            Events.Raise(new AudioVisualizerEvent(response["text"]));
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
}
