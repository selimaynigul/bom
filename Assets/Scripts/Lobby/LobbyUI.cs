using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbyUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI roomCodeText;
    public Button startButton;
    public Button backButton;
    public Transform playerListContainer;
    public GameObject playerNamePrefab; // prefab to list names

    private void OnEnable()
    {
        StartCoroutine(WaitForGameManager());
    }

    private IEnumerator WaitForGameManager()
    {
        float timeout = 5f;
        float timer = 0f;

        while (NetworkGameManager.Instance == null && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (NetworkGameManager.Instance == null)
        {
            Debug.LogError("NetworkGameManager still not found after waiting!");
            yield break;
        }

        Debug.Log("NetworkGameManager successfully found!");

        // Ana UI kurulumu + event baðlantýsý
        SetupLobbyUI();
        SetupLobbyEvents();
    }

    private void SetupLobbyEvents()
    {
        // Çift kayýt engelle
        NetworkGameManager.Instance.PlayerNamesChanged -= SetupLobbyUI;
        NetworkGameManager.Instance.PlayerNamesChanged += SetupLobbyUI;
    }

    private void SetupLobbyUI()
    {
        Debug.Log("SetupLobbyUI çalýþtý!");

        var manager = NetworkGameManager.Instance;

        // Oda kodunu göster
        roomCodeText.text = "Room Code: " + (manager.RoomCode != "" ? manager.RoomCode : "----");
        Debug.Log("Room Code: " + manager.RoomCode);

        // Host'a özel start butonu
        startButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartClicked);

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(OnBackClicked);

        // Eski isimleri temizle
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        // Yeni isimleri ekle
        foreach (var name in manager.PlayerNames)
        {
            Debug.Log($"[UI] Oyuncu: {name.ToString()}");
            GameObject nameObj = Instantiate(playerNamePrefab, playerListContainer);
            nameObj.GetComponentInChildren<TextMeshProUGUI>().text = name.ToString();
        }
    }

    private void OnStartClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
