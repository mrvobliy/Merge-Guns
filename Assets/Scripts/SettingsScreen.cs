using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SettingsScreen : MonoBehaviour
{
    [SerializeField] Switch vibro;
    [SerializeField] Switch sound;
    [SerializeField] GameObject back;


    void Awake()
    {
        vibro.valueChanged += SetVibration;
        sound.valueChanged += SetAudio;
        SetVibration(vibro.IsOn);
        SetAudio(sound.IsOn);
    }
    
    private void SetVibration(bool flag)
    {
        if (flag)
            Vibration.IsON = true;
        else
            Vibration.IsON = false;
    }

    private void SetAudio(bool flag)
    {
        if (flag)
            AudioListener.volume = 1.0f;
        else
            AudioListener.volume = 0f;
    }


    public void ShowSettingsScreen()
    {
        transform.DOLocalMoveX(0f,0.35f).SetEase(Ease.InOutQuad).OnComplete(() => back.SetActive(true));
    }

    public void HideSettingsScreen()
    {
        back.SetActive(false);
        transform.DOLocalMoveX(-2200f,0.35f).SetEase(Ease.InOutQuad);
    }

}
