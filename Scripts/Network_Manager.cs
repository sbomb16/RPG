using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Network_Manager : MonoBehaviourPunCallbacks
{

    public int maxPlayers = 10;

    public static Network_Manager instance;
    // Start is called before the first frame update

    private void Awake()
    {
        if(instance != null && instance != false)
        {
            gameObject.SetActive(false);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }        
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("We're connected to the master server!");

        PhotonNetwork.JoinLobby();
    }


    public void CreateRoom(string roomName)
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)maxPlayers;

        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    [PunRPC]
    public void ChangeScenes(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("Menu_Scene");
    }

    //public override void OnPlayerLeftRoom(Player otherPlayer)
    //{
    //    Game_Manager.instance.alivePlayers--;
    //    Game_UI.instance.UpdatePlayerInfoText();

    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        Game_Manager.instance.CheckWinCondition();
    //    }
    //}
}
