using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GrabHand : MonoBehaviour
{
    public UnityEvent DoThis;
    public bool isColliding = false;
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(GoToNext());
        Debug.Log("ENTER HAND");
    }
    
    private void OnTriggerExit(Collider other)
    {
        isColliding = false;
        StopAllCoroutines();
        Debug.Log("EXIT HAND");
    }
    
    IEnumerator GoToNext()
    {
        if (isColliding)
        {
            yield break;
        }

        isColliding = true;
        yield return new WaitForSeconds(2f);
        DoThis?.Invoke();
        Debug.Log("TRIGGER HAND");
    }
    
}
