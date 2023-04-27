using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GrabHand : MonoBehaviour
{
    public GameObject glow;
    public Animator Animator;
    public float delayBeforeNextEvent = 3f;
    public UnityEvent DoThis;
    public bool isColliding = false;
    public bool isTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(GoToNext());
        Animator.SetBool("IsGrabbed", true);
        
        Debug.Log("ENTER HAND");
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (isTriggered)
        {
            return;
        }
        isColliding = false;
        StopAllCoroutines();
        Animator.SetBool("IsGrabbed", false);
        
        Debug.Log("EXIT HAND");
    }
    
    IEnumerator GoToNext()
    {
        if (isColliding)
        {
            yield break;
        }

        isColliding = true;
        
        yield return new WaitForSeconds(delayBeforeNextEvent);

        DoThis?.Invoke();
        isTriggered = true;
        Debug.Log("TRIGGER HAND");
    }
    
}
