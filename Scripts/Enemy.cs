using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Enemy : MonoBehaviourPun
{

    [Header("Info")]
    public string enemyName;
    public float moveSpeed;
    public int curHp;
    public int maxHp;
    public float chaseRange;
    public float attackRange;
    private Player_Controller targetPlayer;
    public float playerDetectRate = 0.2f;
    private float lastPlayerDetectTime;
    public string objectToSpawnOnDeath;

    [Header("Attack")]
    public int damage;
    public float attackRate;
    private float lastAttackTime;

    [Header("Components")]
    public Header_Info healthBar;
    public SpriteRenderer sr;
    public Rigidbody2D rig;

    public Game_Manager manager;
    public Game_UI ui;

    // Start is called before the first frame update
    void Start()
    {
        healthBar.Initialize(enemyName, maxHp);

        manager = GameObject.Find("GameManager").GetComponent<Game_Manager>();
        ui = GameObject.Find("GameManager").GetComponent<Game_UI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
            
        if (targetPlayer != null)
        {

            float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);

            if (dist < attackRange && Time.time - lastAttackTime >= attackRange)
            {
                Attack();
            }         
            else if (dist > attackRange)
            {
                Vector3 dir = targetPlayer.transform.position - transform.position;
                rig.velocity = dir.normalized * moveSpeed;
            }
            else
            {
                rig.velocity = Vector2.zero;
            }
        }
        DetectPlayer();
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        targetPlayer.photonView.RPC("TakeDamage", targetPlayer.photonPlayer, damage);
    }

    void DetectPlayer()
    {
        if (Time.time - lastPlayerDetectTime > playerDetectRate)
        {
            lastPlayerDetectTime = Time.time;
        }

        for(int i = 0; i < manager.players.Length; i++)
        {

            //Debug.Log(Game_Manager.instance.players[0]);
            float dist = Vector2.Distance(transform.position, manager.players[i].transform.position);

            if (manager.players[i] == targetPlayer)
            {
                if (dist > chaseRange)
                {
                    targetPlayer = null;
                }
            }
            else if (dist < chaseRange)
            {
                if (targetPlayer == null)
                {
                    targetPlayer = manager.players[i];
                }
            }
        }

        //foreach (Player_Controller player in Game_Manager.instance.players)
        //{
            
        //    float dist = Vector2.Distance(transform.position, player.transform.position);

        //    if (player == targetPlayer)
        //    {
        //        if (dist > chaseRange)
        //        {
        //            targetPlayer = null;
        //        }                    
        //    }
        //    else if (dist < chaseRange)
        //    {
        //        if (targetPlayer == null)
        //        {
        //            targetPlayer = player;
        //        }                    
        //    }
        //}
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        curHp -= damage;

        healthBar.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);

        if (curHp <= 0)
        {
            Die();
        }            
        else
        {
            photonView.RPC("FlashDamage", RpcTarget.All);
        }
    }

    [PunRPC]
    void FlashDamage()
    {
        StartCoroutine(DamageFlash());
        IEnumerator DamageFlash()
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            sr.color = Color.white;
        }
    }

    void Die()
    {
        if (objectToSpawnOnDeath != string.Empty)
        {
            PhotonNetwork.Instantiate(objectToSpawnOnDeath, transform.position, Quaternion.identity);
        }

        Game_Manager.instance.enemiesRemaining--;

        Game_UI.instance.UpdateEnemiesRemainingText(Game_Manager.instance.enemiesRemaining);

        PhotonNetwork.Destroy(gameObject);

        if(Game_Manager.instance.enemiesRemaining <= 0)
        {
            ui.ActivateWinText();
            manager.photonView.RPC("Win", RpcTarget.All);
        }
    }
}