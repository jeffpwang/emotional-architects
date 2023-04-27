using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingScale : MonoBehaviour
{
    public GameObject select;
    public List<Vector3> positions;

    public int currentPosition = 0;

    private void OnEnable()
    {
        currentPosition = 0;
        select.transform.localPosition = positions[0];
    }

    public void MoveUp()
    {
        currentPosition++;
        if (currentPosition > positions.Count - 1)
        {
            return;
        }

        select.transform.localPosition = positions[currentPosition];
    }
    
    public void MoveDown()
    {
        currentPosition--;
        if (currentPosition < 0)
        {
            return;
        }

        select.transform.localPosition = positions[currentPosition];
    }
}
