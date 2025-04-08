using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;



public static class RelayManager
{
    public static string JoinCode = "";

    public static async Task<string> CreateRelay(int maxConnections = 4)
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Allocation alloc = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        JoinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
        Debug.Log("[Relay] Created join code: " + JoinCode);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(alloc, "dtls"));

        NetworkManager.Singleton.StartHost();
        return JoinCode;
    }

    public static async Task JoinRelay(string code)
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(code);
        Debug.Log("[Relay] Joined with code: " + code);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(joinAlloc, "dtls"));

        NetworkManager.Singleton.StartClient();
    }

}
