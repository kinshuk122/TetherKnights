using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimationRigging : MonoBehaviour
{
    private Rig rig;
    private float targetWeight;
    
    private void Awake()
    {
        rig = GetComponent<Rig>();
    }

    private void Update()
    {
        rig.weight = Mathf.Lerp(rig.weight, targetWeight, Time.deltaTime * 10f);

        if (Input.GetKeyDown(KeyCode.T))
        {
            targetWeight = 1f;
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            targetWeight = 0f;
        }
    }
}
