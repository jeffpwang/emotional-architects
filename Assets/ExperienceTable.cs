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
            StartCoroutine(AttachToTable());
            isAttached = true;
        }
    }

    public void Attach()
    {
        transform.parent = AppManager.Instance.desk.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        transform.localPosition += offset;
        transform.localRotation = Quaternion.Euler(0, 90, 0);;
    }

    IEnumerator AttachToTable()
    {
        yield return new WaitForSeconds(3f);
        Attach();
    }
}
