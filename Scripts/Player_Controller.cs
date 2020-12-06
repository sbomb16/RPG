using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player_Controller : MonoBehaviourPun
{

    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public int gold;
    public int curHP;
    public int maxHP;
    public bool dead;

    [Header("Attack")]
    public int damage;
    public float attackRange;
    public float attackRate;
    private float lastAttackTime;

    [Header("Components")]
    public Rigidbody2D rig;
    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator weaponAnim;

    public static Player_Controller me;
    public Header_Info headerInfo;

    // Update is called once per frame
    void Update()
    {

        if(!photonView.IsMine)
        {
            return;
        }

        Move();

        if(Input.GetMouseButtonDown(0) && Time.time - lastAttackTime > attackRate)
        {
            Attack();
        }

        float mouseX = (Screen.width / 2) - Input.mousePosition.x;

        if (mouseX < 0)
        {
            weaponAnim.transform.parent.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            weaponAnim.transform.parent.localScale = new Vector3(-1, 1, 1);
        }
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        rig.velocity = new Vector2(x, y) * moveSpeed;
    }

    void Attack()
    {
        lastAttackTime = Time.time;

        Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);

        if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
        }

        weaponAnim.SetTrigger("Attack");        

    }

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        //Debug.Log(id);
        photonPlayer = player;

        headerInfo.Initialize(player.NickName, maxHP);

        if (player.IsLocal)
        {
            me = this;
        }
        else
        {
            rig.isKinematic = true;
        }

        Game_Manager.instance.players[id - 1] = this;
        //Debug.Log(id);        

    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        curHP -= damage;

        if (curHP <= 0)
        {
            Die();
        }            
        else
        {
            StartCoroutine(DamageFlash());
            IEnumerator DamageFlash()
            {
                sr.color = Color.red;

                yield return new WaitForSeconds(0.05f);

                sr.color = Color.white;
            }

            headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHP);
        }
    }

    [PunRPC]
    void Die()
    {
        dead = true;
        rig.isKinematic = true;

        transform.position = new Vector3(0, 99, 0);

        Vector3 spawnPos = Game_Manager.instance.spawnPoints[Random.Range(0, Game_Manager.instance.spawnPoints.Length)].position;

        StartCoroutine(Spawn(spawnPos, Game_Manager.instance.respawnTime));

    }

    IEnumerator Spawn(Vector3 spawnPos, float timeToSpawn)
    {

        yield return new WaitForSeconds(timeToSpawn);

        dead = false;

        transform.position = spawnPos;

        curHP = maxHP;

        rig.isKinematic = false;

        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHP);
    }

    [PunRPC]
    void Heal(int amountToHeal)
    {
        curHP = Mathf.Clamp(curHP + amountToHeal, 0, maxHP);
    }

    [PunRPC]
    void GiveGold(int goldToGive)
    {
        gold += goldToGive;
        Game_UI.instance.UpdateGoldText(gold);
    }

}