using System.Collections;
using System.Collections.Generic;
using Meta.PP;
using UnityEngine;


public class Welcome : MonoBehaviour
{
    public PlayAudio _playAudio;
    // Start is called before the first frame update
    void Start()
    {
        _playAudio.PlaySound();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
