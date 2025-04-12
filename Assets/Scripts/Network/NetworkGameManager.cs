using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;
using UnityEngine.SceneManagement;


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

        // Client kendi ismini gönderir
        if (IsClient && !IsHost)
        {
            SubmitPlayerNameServerRpc(LocalPlayerData.PlayerName);
        }

        // Host kendi ismini doðrudan ekler
        if (IsServer && IsHost)
        {
            FixedString32Bytes hostName = LocalPlayerData.PlayerName;
            if (!playerNames.Contains(hostName))
            {
                playerNames.Add(hostName);
                Debug.Log($"[Server] Host joined as: {hostName}");
            }
        }
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

    // TODO: Game play logic



    private Dictionary<ulong, List<string>> playerHands = new();
    private Dictionary<ulong, string> lastReceivedWord = new();
    private NetworkVariable<ulong> currentPlayerId = new();
    private int currentPlayerIndex = 0;
    private List<ulong> connectedClientIds = new();


    public NetworkVariable<ulong> CurrentPlayerIdVar => currentPlayerId;

    // Event listener
    public event Action OnGameStateChanged;

    private void OnEnable()
    {
        currentPlayerId.OnValueChanged += (_, _) => OnGameStateChanged?.Invoke();
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (!IsServer) return;

        // Oyuncu ID listesini topla
        connectedClientIds.Clear();
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            connectedClientIds.Add(client.ClientId);
        }

        // Kelimeleri çarp ve karýþtýr
        var pool = new List<string>();
        int totalWords = connectedClientIds.Count * PlayerCount.Value;
        int wordIndex = 0;

        for (int i = 0; i < totalWords; i++)
        {
            pool.Add(words[wordIndex]);
            wordIndex = (wordIndex + 1) % words.Count;
        }


        Shuffle(pool);

        // Her oyuncuya N tane kelime ver
        int index = 0;
        foreach (var clientId in connectedClientIds)
        {
            playerHands[clientId] = pool.GetRange(index, PlayerCount.Value);
            lastReceivedWord[clientId] = "";

            SendHandToPlayer(clientId); // buraya ekle

            index += PlayerCount.Value;
        }


        // Ýlk oyuncuyu sýraya ata
        currentPlayerIndex = 0;
        currentPlayerId.Value = connectedClientIds[currentPlayerIndex];

        Debug.Log("[Game] Oyun baþlatýldý!");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendWordToNextPlayerServerRpc(string word, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        ulong receiverId = GetNextPlayerId(senderId);

        if (!playerHands.ContainsKey(senderId) || !playerHands[senderId].Contains(word)) return;

        playerHands[senderId].Remove(word);
        playerHands[receiverId].Add(word);
        lastReceivedWord[receiverId] = word;

        SendHandToPlayer(senderId);
        SendHandToPlayer(receiverId);


        Debug.Log($"[Game] {senderId} -> {receiverId} kelime gönderdi: {word}");

        // Kazanma kontrolü
        CheckWinCondition(receiverId);

        // Sýra deðiþtir
        UpdateTurn();
    }


    private void UpdateTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % connectedClientIds.Count;
        currentPlayerId.Value = connectedClientIds[currentPlayerIndex];

        OnGameStateChanged?.Invoke(); // sýra deðiþti, herkese bildir
    }


    private void CheckWinCondition(ulong playerId)
    {
        var hand = playerHands[playerId];

        foreach (var word in hand)
        {
            int count = hand.FindAll(w => w == word).Count;
            if (count == PlayerCount.Value)
            {
                Debug.Log($"[Game] Oyuncu kazandý: {playerId} - {word}");
                AnnounceWinnerClientRpc(playerId, word);
                return;
            }
        }
    }

    // Her client’a kendi elini gönder
    [ClientRpc]
    private void UpdateHandClientRpc(FixedString32Bytes[] handArray, ClientRpcParams clientRpcParams = default)
    {
        if (!NetworkManager.Singleton.IsClient) return;

        List<string> converted = new();
        foreach (var w in handArray)
        {
            converted.Add(w.ToString());
        }

        LocalPlayerData.Hand = converted;
        Debug.Log($"[Client] El güncellendi: {string.Join(", ", converted)}");
    }

    private void SendHandToPlayer(ulong clientId)
    {
        var list = playerHands[clientId];
        FixedString32Bytes[] converted = new FixedString32Bytes[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            converted[i] = list[i];
        }

        var target = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new List<ulong> { clientId }
            }
        };

        UpdateHandClientRpc(converted, target);
    }


    [ClientRpc]
    private void AnnounceWinnerClientRpc(ulong winnerId, string winningWord)
    {
        string playerName = GetPlayerName(winnerId);
        Debug.Log($"Kazanan: {playerName} - {winningWord}");

        if (GameUI.Instance != null)
            GameUI.Instance.ShowWinner(playerName, winningWord);
    }


    private ulong GetNextPlayerId(ulong current)
    {
        int i = connectedClientIds.IndexOf(current);
        int next = (i + 1) % connectedClientIds.Count;
        return connectedClientIds[next];
    }

    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }

    public ulong CurrentPlayerId => currentPlayerId.Value;

    public List<string> GetHand(ulong clientId)
    {
        if (playerHands.ContainsKey(clientId))
            return playerHands[clientId];
        return new List<string>();
    }

    public string GetPlayerName(ulong clientId)
    {
        if (clientId < (ulong)playerNames.Count)
            return playerNames[(int)clientId].ToString();

        return $"Player {clientId}";
    }

    public string GetLastReceivedWord(ulong clientId)
    {
        if (lastReceivedWord.ContainsKey(clientId))
            return lastReceivedWord[clientId];

        return "";
    }

    [ClientRpc]
    public void ReturnToLobbyClientRpc()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void ResetGame()
    {
        playerHands.Clear();
        lastReceivedWord.Clear();
        connectedClientIds.Clear();

        currentPlayerId.Value = 0;
        currentPlayerIndex = 0;

        Debug.Log("[GameManager] Oyun durumu sýfýrlandý.");
    }


}
