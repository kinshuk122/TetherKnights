using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    public GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        Transform gameobjectPosition = this.gameObject.transform; 
        Instantiate(player, gameobjectPosition);
    }
}
