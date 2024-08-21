using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class TestRelay : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinLobbyCode;
    public int maxPlayer;
    public TextMeshProUGUI joinCodeText;
    
    private async void Start()
    {
       await UnityServices.InitializeAsync();

       AuthenticationService.Instance.SignedIn += () =>
       {
            Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
       };
       await AuthenticationService.Instance.SignInAnonymouslyAsync();
       Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);

    }

    public async void CreateRelay()
    {
        try
        {
            //Takes parameter for max amount of players in a game not including host
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            //This string is used to connect to friends
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            joinCodeText.text = joinCode;
            // Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    public async void JoinRelay(string joinCode)
    {
        joinCode = joinLobbyCode.text;
        Debug.Log(joinCode);
        
        try
        {
            Debug.Log("Joining Relay with: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
