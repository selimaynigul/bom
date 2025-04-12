using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class GameUI : MonoBehaviour
{
    public Transform wordListContainer;
    public GameObject wordButtonPrefab;
    public TextMeshProUGUI lastReceivedText;
    public TextMeshProUGUI currentTurnText;
    public GameObject yourTurnBanner;
    public Button giveButton;
    private string selectedWord = null;
    public GameObject winnerPanel;
    public TextMeshProUGUI winnerText;
    public Button backToLobbyButton;


    public static GameUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        winnerPanel.SetActive(false);
        backToLobbyButton.gameObject.SetActive(false);
        backToLobbyButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("LobbyScene");
        });

        giveButton.onClick.AddListener(OnGiveClicked);
        giveButton.gameObject.SetActive(false);

        RefreshUI();
    }

    private void OnEnable()
    {
        if (NetworkGameManager.Instance != null)
            NetworkGameManager.Instance.OnGameStateChanged += RefreshUI;
    }

    private void OnDisable()
    {
        if (NetworkGameManager.Instance != null)
            NetworkGameManager.Instance.OnGameStateChanged -= RefreshUI;
    }


    private void RefreshUI()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        var manager = NetworkGameManager.Instance;

        // Sýra sende mi?
        bool isMyTurn = (manager.CurrentPlayerId == localId);
        yourTurnBanner.SetActive(isMyTurn);

        // Sýradaki oyuncunun adý
        string name = manager.GetPlayerName(manager.CurrentPlayerId);
        currentTurnText.text = $"Sýradaki Oyuncu: {name}";

        // Son alýnan kelime
        string lastWord = manager.GetLastReceivedWord(localId);
        lastReceivedText.text = $"Son Aldýðýn: {lastWord}";

        // Ver butonunu sýfýrla
        giveButton.gameObject.SetActive(isMyTurn && selectedWord != null);

        // Elindeki kelimeler
        foreach (Transform child in wordListContainer)
            Destroy(child.gameObject);

        var hand = LocalPlayerData.Hand;
        foreach (string word in hand)
        {
            GameObject obj = Instantiate(wordButtonPrefab, wordListContainer);
            var txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = word;

            var btn = obj.GetComponent<Button>();
            btn.interactable = isMyTurn;

            btn.onClick.AddListener(() => OnWordClicked(word));
        }
    }

    private void OnWordClicked(string word)
    {
        if (selectedWord == word)
        {
            selectedWord = null;
        }
        else
        {
            selectedWord = word;
        }

        giveButton.gameObject.SetActive(selectedWord != null && NetworkGameManager.Instance.CurrentPlayerId == NetworkManager.Singleton.LocalClientId);
    }

    private void OnGiveClicked()
    {
        if (selectedWord != null)
        {
            NetworkGameManager.Instance.SendWordToNextPlayerServerRpc(selectedWord);
            selectedWord = null;
            giveButton.gameObject.SetActive(false);

            RefreshUI(); // yerel temizlik
        }
    }

    public void ShowWinner(string playerName, string word)
    {
        winnerText.text = $"{playerName} oyunu kazandý!\nKelime: {word}";
        winnerPanel.SetActive(true);
        backToLobbyButton.gameObject.SetActive(true); // göster
    }
}
