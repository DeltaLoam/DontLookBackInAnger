using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MultiplayerUIManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField roomNameInput;

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text)) return;
        PhotonNetwork.CreateRoom(roomNameInput.text, new RoomOptions { MaxPlayers = 4 });
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text)) return;
        PhotonNetwork.JoinRoom(roomNameInput.text);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        SceneManager.LoadScene("WaitingRoom");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join failed: " + message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Create failed: " + message);
    }
}
