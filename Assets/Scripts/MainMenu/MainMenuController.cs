using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OnJoinRoomClicked()
    {
        SceneManager.LoadScene("JoinRoomScene");
    }

    public void OnCreateRoomClicked()
    {
        SceneManager.LoadScene("CreateRoomScene");
    }
}
