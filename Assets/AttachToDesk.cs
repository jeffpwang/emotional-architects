using System;
using System.Collections;
using System.Collections.Generic;
using Meta.PP;
using UnityEngine;

public class AttachToDesk : MonoBehaviour
{
    public bool isAttached = false;
    public bool useRotation = true;
    public Vector3 offset = new Vector3(0, -0.07f, 0);

    public void OnDisable()
    {
        isAttached = false;
    }

    void Update()
    {
        if ((AppManager.Instance.desk != null) && !isAttached)
        {
            Attach();
            isAttached = true;
        }
    }

    public void Attach()
    {
        transform.position = AppManager.Instance.desk.transform.position + offset;
        if (useRotation)
        {
            transform.rotation = AppManager.Instance.desk.transform.rotation;   
        }
    }
}
