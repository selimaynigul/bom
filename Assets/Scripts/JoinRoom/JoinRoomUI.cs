using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class JoinRoomUI : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public TMP_InputField roomCodeInput;
    public Button joinButton;
    public Button backButton;

    public GameObject loadingPanel; // UI'da görünür hale getireceðiz

    private void Start()
    {
        joinButton.onClick.AddListener(JoinRoom);
        backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        loadingPanel.SetActive(false); // baþlangýçta kapalý
    }

    private async void JoinRoom()
    {
        string playerName = playerNameInput.text;
        string roomCode = roomCodeInput.text;

        if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("Name and code are required.");
            return;
        }

        // UI kilitle
        joinButton.interactable = false;
        backButton.interactable = false;
        loadingPanel.SetActive(true);

        LocalPlayerData.PlayerName = playerName;
        LocalPlayerData.RequestedRoomCode = roomCode;

        // Relay baðlantýsý
        await RelayManager.JoinRelay(roomCode);

        // Sahneye geç
        SceneManager.LoadScene("LobbyScene");
    }
}
