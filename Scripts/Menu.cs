using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Menu : MonoBehaviourPunCallbacks, ILobbyCallbacks
{

    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject lobbyScreen;
    public GameObject lobbyBrowserScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button findRoomButton;

    [Header("Lobby")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomInfoText;
    public Button startGameButton;

    [Header("Lobby Browser")]
    public RectTransform roomListContainer;
    public GameObject roomButtonPrefab;

    private List<GameObject> roomButtons = new List<GameObject>();
    private List<RoomInfo> roomList = new List<RoomInfo>();

    // Start is called before the first frame update
    void Start()
    {
        createRoomButton.interactable = false;
        findRoomButton.interactable = false;

        Cursor.lockState = CursorLockMode.None;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    void SetScreen(GameObject screen)
    {
        mainScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        lobbyBrowserScreen.SetActive(false);

        screen.SetActive(true);

        if(screen != lobbyBrowserScreen)
        {
            UpdateLobbyBrowserUI();
        }
    }

    public void OnPlayerNameValueChanged(TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    public override void OnConnectedToMaster()
    {
        createRoomButton.interactable = true;
        findRoomButton.interactable = true;
    }

    public void OnCreateRoomButton()
    {
        SetScreen(createRoomScreen);
    }

    public void OnFindRoomButton()
    {
        SetScreen(lobbyBrowserScreen);
    }

    public void OnBackButton()
    {
        SetScreen(mainScreen);
    }

    public void OnCreateButton(TMP_InputField roomNameInput)
    {
        Network_Manager.instance.CreateRoom(roomNameInput.text);
    }

    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    [PunRPC]
    public void UpdateLobbyUI()
    {
        startGameButton.interactable = PhotonNetwork.IsMasterClient;

        playerListText.text = "";

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + "\n";
        }

        roomInfoText.text = "<b>Room Name</b>\n" + PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    public void OnStartGameButton()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        Network_Manager.instance.photonView.RPC("ChangeScenes", RpcTarget.All, "Game_Scene");
    }

    public void OnLeaveLobbyButton()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }

    void UpdateLobbyBrowserUI()
    {
        foreach(GameObject Button in roomButtons)
        {
            Button.SetActive(false);
        }

        for(int i = 0; i < roomList.Count; i++)
        {
            GameObject button = i >= roomButtons.Count ? CreateRoomButton() : roomButtons[i];

            button.SetActive(true);

            button.transform.Find("Room_Name_Text").GetComponent<TextMeshProUGUI>().text = roomList[i].Name;

            button.transform.Find("Player_Count_Text").GetComponent<TextMeshProUGUI>().text = roomList[i].PlayerCount + " / " + roomList[i].MaxPlayers;

            Button buttonComp = button.GetComponent<Button>();
            string roomName = roomList[i].Name;
            buttonComp.onClick.RemoveAllListeners();
            buttonComp.onClick.AddListener(() => { OnJoinRoomButton(roomName); });
        }

    }

    GameObject CreateRoomButton()
    {
        GameObject buttonOBJ = Instantiate(roomButtonPrefab, roomListContainer.transform);

        roomButtons.Add(buttonOBJ);

        return buttonOBJ;
    }

    public void OnJoinRoomButton(string roomName)
    {
        Network_Manager.instance.JoinRoom(roomName);
    }

    public void OnRefreshButton()
    {
        UpdateLobbyBrowserUI();
    }

    public override void OnRoomListUpdate(List<RoomInfo> allRooms)
    {
        roomList = allRooms;
    }
}
