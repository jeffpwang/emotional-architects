using Meta.WitAi.Json;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExampleVoiceScript : MonoBehaviour
{
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private APIIntegration APIIntegration;
    [SerializeField] private TTSSpeaker tTSSpeaker;
    [SerializeField] private GameObject _targetCube;

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
            Debug.LogError(response["text"]);
            tTSSpeaker.Speak(response["text"]);
            StartCoroutine(APIIntegration.GenerateImage(response["text"], _targetCube));
        }
    }

    public void EnableCube(bool enable)
    {
        _targetCube.SetActive(enable);
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
