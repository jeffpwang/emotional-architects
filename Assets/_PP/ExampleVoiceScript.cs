using Meta.WitAi.Json;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using UnityEngine;

public class ExampleVoiceScript : MonoBehaviour
{
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private TTSSpeaker tTSSpeaker;
    [SerializeField] private GameObject _targetCube;

    private void OnEnable()
    {
        appVoiceExperience.VoiceEvents.OnResponse.AddListener(OnRequestResponse);
    }

    private void OnDisable()
    {
        appVoiceExperience.VoiceEvents.OnResponse.RemoveListener(OnRequestResponse);
    }

    private void OnRequestResponse(WitResponseNode response)
    {
        if (!string.IsNullOrEmpty(response["text"]))
        {
            tTSSpeaker.Speak(response["text"]);
        }
    }

    public void EnableCube(bool enable) 
    {
        _targetCube.SetActive(enable);
    }
}
