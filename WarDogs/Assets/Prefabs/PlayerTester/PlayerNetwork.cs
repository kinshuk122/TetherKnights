using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        Vector3 moveDir = new Vector3(0, 0, 0);
        
        if(Input.GetKey(KeyCode.W))
        {
            moveDir.z += 1;
        }
        
        if(Input.GetKey(KeyCode.S))
        {
            moveDir.z -= 1;
        }
        
        if(Input.GetKey(KeyCode.A))
        {
            moveDir.x -= 1;
        }
        
        if(Input.GetKey(KeyCode.D))
        {
            moveDir.x += 1;
        }

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
