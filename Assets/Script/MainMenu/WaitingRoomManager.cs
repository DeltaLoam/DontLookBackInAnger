using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class WaitingRoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerListText;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text voiceStatusText;

    private void Start()
    {
        roomNameText.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        UpdatePlayerList();

    }

    private void UpdatePlayerList()
    {
        playerListText.text = "";

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + (player.IsMasterClient ? " (Host)\n" : "\n");
        }
    }

    public void OnClickStartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Load main game scene for everyone
        PhotonNetwork.LoadLevel("MainGame");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MultiplayerLobby");
    }
}
