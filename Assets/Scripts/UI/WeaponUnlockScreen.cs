using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class WeaponUnlockScreen : MonoBehaviour
{
    [SerializeField] Transform gunPosition;
    [SerializeField] GameObject[] weapons;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] TMP_Text levelText;



    void Update()
    {
        gunPosition.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    public void ShowWeaponScreen(int index)
    {
        levelText.text = "level " + (index+2).ToString();
        transform.localScale = Vector3.zero;
        gameObject.SetActive(true);
        foreach (var gun in weapons)
        {
            gun.gameObject.SetActive(false);
        }
        gunPosition.transform.eulerAngles = Vector3.zero;
        weapons[index].SetActive(true);
        transform.DOScale(Vector3.one, 0.5f).OnComplete(() => 
        {
            DOTween.Sequence().AppendInterval(3.0f).AppendCallback(() => gameObject.SetActive(false));
        });
        
    }

}
