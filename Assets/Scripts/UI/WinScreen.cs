using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    [SerializeField] TMP_Text earnedText;
    [SerializeField] TMP_Text levelText;


    public void ShowWinScreen(int money, int level)
    {
        earnedText.text = money.ToString();
        levelText.text = "level " + level.ToString() + " completed!";
        gameObject.SetActive(true);
    }

}
