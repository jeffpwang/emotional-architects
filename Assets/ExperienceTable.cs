using System.Collections;
using System.Collections.Generic;
using Meta.PP;
using UnityEngine;

public class ExperienceTable : MonoBehaviour
{
    public bool isAttached = false;
    public Vector3 offset = new Vector3(0, -0.07f, 0);

    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
    
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
        // transform.rotation = AppManager.Instance.desk.transform.rotation;
    }
}
