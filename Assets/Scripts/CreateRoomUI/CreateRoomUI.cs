using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Collections;
using System.Linq;

public class CreateRoomUI : MonoBehaviour
{
    // Referans olarak prefab'ý Inspector'dan atayacaksýn
    public GameObject gameManagerPrefab;

    [Header("Player Count")]
    public TextMeshProUGUI playerCountText;
    public Button increaseButton;
    public Button decreaseButton;

    [Header("Word Slots")]
    public Transform wordSlotContainer;
    public GameObject wordSlotPrefab;

    [Header("Game Mode")]
    public ToggleGroup gameModeToggleGroup;

    [Header("Navigation Buttons")]
    public Button backButton;
    public Button continueButton;

    private int playerCount = 4;
    private const int maxPlayers = 6;
    private const int minPlayers = 3;

    private List<GameObject> wordSlots = new();

    private void Start()
    {
        increaseButton.onClick.AddListener(IncreasePlayerCount);
        decreaseButton.onClick.AddListener(DecreasePlayerCount);
        backButton.onClick.AddListener(HandleBack);
        continueButton.onClick.AddListener(HandleContinue);
        DontDestroyOnLoad(NetworkManager.Singleton.gameObject);

        RefreshWordSlots();
    }

    private void IncreasePlayerCount()
    {
        if (playerCount < maxPlayers)
        {
            playerCount++;
            RefreshWordSlots();
        }
    }

    private void DecreasePlayerCount()
    {
        if (playerCount > minPlayers)
        {
            playerCount--;
            RefreshWordSlots();
        }
    }

    private void RefreshWordSlots()
    {
        playerCountText.text = playerCount.ToString();

        foreach (var slot in wordSlots)
        {
            Destroy(slot);
        }
        wordSlots.Clear();

        for (int i = 0; i < playerCount; i++)
        {
            GameObject newSlot = Instantiate(wordSlotPrefab, wordSlotContainer);
            TMP_InputField input = newSlot.GetComponentInChildren<TMP_InputField>();
            input.text = $"Word {i + 1}";
            wordSlots.Add(newSlot);
        }
    }

    private void HandleBack()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private async void HandleContinue()
    {
        List<string> words = new();
        foreach (var slot in wordSlots)
        {
            var input = slot.GetComponentInChildren<TMP_InputField>();
            words.Add(input.text);
        }

        string selectedMode = "Classic"; // placeholder

        // 1. Relay üzerinden host baþlat
        string relayCode = await RelayManager.CreateRelay();

        // 2. NetworkGameManager sahnede yoksa instantiate et
        if (NetworkGameManager.Instance == null)
        {
            GameObject obj = Instantiate(gameManagerPrefab);
            NetworkGameManager.Instance.SetRoomCode(relayCode);
            obj.GetComponent<NetworkObject>().Spawn();
        }


        // 3. Veriyi gönder
        FixedString32Bytes mode = selectedMode;
        NetworkGameManager.Instance.SetGameSettingsServerRpc(playerCount, mode);

        FixedString32Bytes[] wordArray = new FixedString32Bytes[words.Count];
        for (int i = 0; i < words.Count; i++)
        {
            wordArray[i] = words[i];
        }
        NetworkGameManager.Instance.SetWordsServerRpc(wordArray);

        // 4. Sahne deðiþtir (Netcode scene manager ile)
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }


}
