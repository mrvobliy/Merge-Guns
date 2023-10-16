using UnityEngine;
using TMPro;

public static class MoneySystem
{
    private static int _currentMoney;
    private static TextMeshProUGUI _moneyText;

    private static int _startLevelMoney;
    private static int _endLevelMoney;

    public static void Initialize(int startingMoney, TextMeshProUGUI textComponent)
    {
        _moneyText = textComponent;

        if (!LoadMoney())
        {
            _currentMoney = startingMoney;
        }

        UpdateMoneyText();
    }

    public static bool CanAfford(int amount)
    {
        return _currentMoney >= amount;
    }

    public static void AddMoney(int amount)
    {
        _currentMoney += amount;
        UpdateMoneyText();
        SaveMoney();
    }

    public static void SetStartLevelMoney()
    {
        _startLevelMoney = _currentMoney;
        _endLevelMoney = _startLevelMoney;
        Debug.Log(_currentMoney);
    }

    public static void IncreaseLevelMoney(int amount)
    {
        _endLevelMoney += amount;
    }

    public static int GetLevelMoney()
    {
        Debug.Log(_endLevelMoney);
        Debug.Log(_startLevelMoney);
        return _endLevelMoney - _startLevelMoney;
    }

    public static void DeductMoney(int amount)
    {
        if (CanAfford(amount))
        {
            _currentMoney -= amount;
            UpdateMoneyText();
            SaveMoney();
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    public static int GetCurrentMoney()
    {
        return _currentMoney;
    }

    public static Vector2 GetMoneyPosition()
    {
        return _moneyText.rectTransform.anchoredPosition;
    }

    private static void UpdateMoneyText()
    {
        if (_moneyText != null)
        {
            _moneyText.text = _currentMoney.ToString();
        }
    }

    private static void SaveMoney()
    {
        PlayerPrefs.SetInt("Money", _currentMoney);
        PlayerPrefs.SetInt("StartMoney", _startLevelMoney);
        PlayerPrefs.SetInt("EndMoney", _endLevelMoney);
        PlayerPrefs.Save();
    }

    public static bool LoadMoney()
    {
        _startLevelMoney = PlayerPrefs.GetInt("StartMoney", 0);
        _endLevelMoney = PlayerPrefs.GetInt("EndMoney", 0);
        if (PlayerPrefs.HasKey("Money"))
        {
            _currentMoney = PlayerPrefs.GetInt("Money");
            return true;
        }
        else
        {
            Debug.Log("No saved money found. Starting with default.");
            _currentMoney = 0;
            return false;
        }
        
    }
}