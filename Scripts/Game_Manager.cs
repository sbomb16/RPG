using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class Game_Manager : MonoBehaviourPun
{

    [Header("Players")]
    public string playerPrefabLocation;
    public Transform[] spawnPoints;
    public float respawnTime;

    private int playersInGame;
    public int enemiesRemaining;

    public Player_Controller[] players;

    public static Game_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

        players = new Player_Controller[PhotonNetwork.PlayerList.Length];

        photonView.RPC("ImInGame", RpcTarget.AllBuffered);

        enemiesRemaining = 25;

    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;

        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }            
    }

    [PunRPC]
    void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

        playerObj.GetComponent<PhotonView>().RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);

        //players[playersInGame - 1] = playerObj.GetComponent<Player_Controller>();
    }
    
    public Player_Controller GetPlayer(int playerID)
    {
        foreach(Player_Controller player in players)
        {
            if(player != null && player.id == playerID)
            {
                Debug.Log(player);
                return player;
            }
        }
        return null;
    }

    public Player_Controller GetPlayer(GameObject playerObj)
    {

        foreach(Player_Controller player in players)
        {
            if(player != null && player.gameObject == playerObj)
            {
                return player;
            }
        }

        return null;
    }

    [PunRPC]
    public void Win()
    {       

        StartCoroutine(Finish());
        IEnumerator Finish()
        {            

            yield return new WaitForSeconds(5f);

            photonView.RPC("GoBackToMenu", RpcTarget.All);
        }
    }

    [PunRPC]
    void GoBackToMenu()
    {
        Network_Manager.instance.ChangeScenes("Menu_Scene");
    }
}
