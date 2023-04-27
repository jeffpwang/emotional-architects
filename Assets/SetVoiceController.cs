using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVoiceController : MonoBehaviour
{
    public VoiceController voiceController;

    public void SetVoiceControllerTo()
    {
        
        voiceController.SetVoiceControllerState(VoiceController.VoiceControllerState.Keywords);
        voiceController.SetActivation(true);
    }
}
