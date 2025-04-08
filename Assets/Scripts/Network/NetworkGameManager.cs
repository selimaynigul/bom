using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;

    [Header("Synchronized Settings")]
    public NetworkVariable<int> PlayerCount = new();
    public NetworkVariable<FixedString32Bytes> GameMode = new();

    [Header("Host-only Settings")]
    private List<string> words = new();
    public List<string> Words => words;

    private NetworkVariable<FixedString32Bytes> roomCode = new("");
    public string RoomCode => roomCode.Value.ToString();

    public event Action PlayerNamesChanged;
    // New: Player name list (server holds)
    private NetworkList<FixedString32Bytes> playerNames = new();

    public List<FixedString32Bytes> PlayerNames
    {
        get
        {
            List<FixedString32Bytes> copy = new();
            foreach (var name in playerNames)
            {
                copy.Add(name);
            }
            return copy;
        }
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (Instance == null)
            Instance = this;

        if (playerNames == null)
            playerNames = new NetworkList<FixedString32Bytes>();

        playerNames.OnListChanged += (change) =>
        {
            PlayerNamesChanged?.Invoke();
        };

        if (IsClient && !IsHost)
        {
            SubmitPlayerNameServerRpc(LocalPlayerData.PlayerName);
        }

        playerNames.OnListChanged += change =>
        {
            Debug.Log("Player list changed");
        };
    }


    [ServerRpc(RequireOwnership = false)]
    public void SubmitPlayerNameServerRpc(string playerName)
    {
        FixedString32Bytes name = playerName;
        if (!playerNames.Contains(name))
        {
            playerNames.Add(name);
            Debug.Log($"[Server] Player joined: {playerName}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetGameSettingsServerRpc(int playerCount, FixedString32Bytes gameMode)
    {
        PlayerCount.Value = playerCount;
        GameMode.Value = gameMode;

        Debug.Log($"[Host] Game Settings Set: {playerCount} Players, Mode = {gameMode}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetWordsServerRpc(FixedString32Bytes[] wordArray)
    {
        words = new List<string>();
        foreach (var w in wordArray)
        {
            words.Add(w.ToString());
        }

        Debug.Log($"[Host] Word List Received: {string.Join(", ", words)}");
    }

    public List<string> GetWords()
    {
        return words;
    }

    public List<FixedString32Bytes> GetPlayerNames()
    {
        List<FixedString32Bytes> copy = new();
        foreach (var name in playerNames)
        {
            copy.Add(name);
        }
        return copy;
    }



    public void SetRoomCode(string code)
    {
        roomCode.Value = code;
    }

}
