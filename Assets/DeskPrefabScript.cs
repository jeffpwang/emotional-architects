using System;
using System.Collections;
using System.Collections.Generic;
using Meta.PP;
using UnityEngine;

public class DeskPrefabScript : MonoBehaviour
{
    public static DeskPrefabScript Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        AppManager.Instance.desk = this;
    }
    
    
}
