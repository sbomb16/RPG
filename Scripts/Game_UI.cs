using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game_UI : MonoBehaviour
{

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI enemyText;
    public TextMeshProUGUI winText;

    public static Game_UI instance;

    void Awake()
    {
        instance = this;
    }

    public void UpdateGoldText(int gold)
    {
        goldText.text = "<b>Gold:</b> " + gold;
    }

    public void UpdateEnemiesRemainingText(int enemies)
    {
        enemyText.text = "<b>Enemies Remaining: </b>" + enemies;
    }

    public void ActivateWinText()
    {
        winText.enabled = true;
    }
}
