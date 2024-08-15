using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerMultiplayerScript : NetworkBehaviour
{
    [SerializeField] private Transform spawnObjectPrefab;
    private Transform spawnedObject;
    
    //Syncronize variables on all the screens, give permissions to owner and server
    private NetworkVariable<MyCustomeData> randomNumber = new NetworkVariable<MyCustomeData>(new MyCustomeData
    {
        _int = 56,
        _bool = true,
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //CustomData for networkvariable
    public struct MyCustomeData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message; //one character = one byte. Pick the right FixedString byte limit
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);

        }
    }
    
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomeData previousValue, MyCustomeData newValue) =>
        {
            Debug.Log(OwnerClientId + ": Random Number: " + newValue._int + "  " + newValue._bool + " " + newValue.message);
        };
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            spawnedObject = Instantiate(spawnObjectPrefab);
            spawnedObject.GetComponent<NetworkObject>().Spawn(true); //This is used to spawn gameobject on host and client both
            
            // TestClientRpc(new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>{1}}});
            // randomNumber.Value = new MyCustomeData
            // {
            //     _int =  10,
            //     _bool = false,
            //     message = "All your base are belong to us!"
            // };
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            spawnedObject.GetComponent<NetworkObject>().Despawn(true); //This is used to despawn gameobject on host and client both. Can be used to remove the gamobject from network but keep the object alive
            //Destroy(spawnedObject.gameObject);
        }
    }

    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams) //Always end function name with "ServerRpc". This does not run on client only on host
    {
        Debug.Log("TestServerRPc " + OwnerClientId + " " + serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams) //This shows on both host and client
    {
        Debug.Log("Test Client Rpc");
    }
    
}
