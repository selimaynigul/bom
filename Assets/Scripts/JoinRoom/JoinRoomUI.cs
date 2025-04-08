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

    private void Start()
    {

        joinButton.onClick.AddListener(JoinRoom);
        backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
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

        LocalPlayerData.PlayerName = playerName;
        LocalPlayerData.RequestedRoomCode = roomCode;

        // Relay ba�lant�s�
        await RelayManager.JoinRelay(roomCode);

        // Odaya ba�land�ktan sonra sahneye ge�
        SceneManager.LoadScene("LobbyScene");
    }

}
