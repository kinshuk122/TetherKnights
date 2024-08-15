using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DirectionalGravity : MonoBehaviour
{
    [SerializeField] private Transform testCube;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            testCube.eulerAngles = new Vector3(90f, 0f, 0f);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            testCube.eulerAngles = new Vector3(180f, 0f, 0f);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            testCube.eulerAngles = new Vector3(270f, 0f, 0f);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            testCube.eulerAngles = new Vector3(360f, 0f, 0f);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            testCube.eulerAngles = new Vector3(0f, 0f, -90f);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            testCube.eulerAngles = new Vector3(0f, 0f, 90f);
        }
        
        
    }
}
