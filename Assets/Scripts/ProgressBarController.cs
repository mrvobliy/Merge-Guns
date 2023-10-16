using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    [SerializeField] float scaleTimer = 0.25f;
    [SerializeField] TMP_Text ProgressText;
    [SerializeField] TMP_Text CurrentLevelText;
    [SerializeField] Image progressFiller;


    private float _maxValue;
    private float _currentValue;
    private int _currentLevelIndex = 0;

    public void InitWeaponProgressBar(float maxHP)
    {
        _currentLevelIndex++;
        CurrentLevelText.text = _currentLevelIndex.ToString();
        progressFiller.fillAmount = 0;
        _maxValue = maxHP;
        _currentValue = 0;
        ProgressText.SetText($"{_maxValue.ToString()}/{_maxValue.ToString()}");
    }

    public void InitTargetProgressBar(float maxHP, int levelNum)
    {
        _currentLevelIndex = levelNum;
        CurrentLevelText.text = _currentLevelIndex.ToString();
        progressFiller.fillAmount = 1;
        _maxValue = maxHP;
        _currentValue = maxHP;
        ProgressText.SetText($"{_maxValue.ToString("#,##0")}/{_maxValue.ToString("#,##0")}");
    }

    public void SetProgress(float progress)
    {
        _currentValue = progress;
        progressFiller.fillAmount = _currentValue/_maxValue;
        
        ProgressText.SetText($"{_currentValue.ToString("#,##0")}/{_maxValue.ToString("#,##0")}");
    }
}
