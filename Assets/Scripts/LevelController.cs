
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Coffee.UIExtensions;
using GameAnalyticsSDK;

public class LevelController : MonoBehaviour
{
    [SerializeField] TMP_Text fps;
    [SerializeField] UIParticle[] particles;
    [SerializeField] WinScreen winScreen;
    [SerializeField] ProgressBarController weaponProgressBar;
    [SerializeField] int startingMoney;
    [SerializeField] Target[] targets;
    [SerializeField] LevelEnviroment[] Levels;
    [SerializeField] Transform levelPosition;
    [SerializeField] Spawner spawner;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] int startLevel = 1;
    [SerializeField] TMP_Text weaponCost;
    [SerializeField] Sprite[] weaponSprites;
    [SerializeField] Image weaponImage;
    [SerializeField] int spawnToNextLevel = 30;
    
    public int CurrentCost = 26;

    private int _numberOfSpawns = 0;
    private int _currentSpawnWeaponLevel = 1;
    private int _currentTargetIndex = 0;
    private int _currentTargetSaveHP = 200;
    private Target _currentTarget;
    private LevelEnviroment _currentLevel;
    private Transform _targetPosition;



    void Awake()
    {
        GameAnalytics.Initialize();
        LoadWeaponSpawnStats();
        LoadTargetInfo();
        MoneySystem.Initialize(startingMoney, moneyText);
        InitTarget(_currentTargetIndex, _currentTargetSaveHP);

        spawner.RespawnGunsMinLevel(_currentSpawnWeaponLevel);
        ChangeSpawnButton();
        weaponProgressBar.InitWeaponProgressBar(spawnToNextLevel);
        weaponProgressBar.SetProgress(_numberOfSpawns);
    }

    void Update()
    {
        fps.text = "FPS: " + ((int)(1.0f / Time.deltaTime)).ToString();
    }

    public void InitTarget(int targetIndex, int targetHP = 0)
    {
        ChangeLocation(targetIndex);
        ChangeTarget(targetIndex);
        

        if (targetHP != 0)
        {
            _currentTarget.ReloadWithoutParts(_currentTarget.maxHP - _currentTargetSaveHP);
            _currentTarget.GetTargetToShoot(0);
            return;
        }

        MoneySystem.SetStartLevelMoney();
    }

    public void SpawnButton()
    {
        if(MoneySystem.CanAfford(CurrentCost))
        {
            spawner.SpawnWeapon(_currentSpawnWeaponLevel);
            MoneySystem.DeductMoney(CurrentCost);
            _numberOfSpawns++;
            ChangeSpawnButton();
            weaponProgressBar.SetProgress(_numberOfSpawns);
        }
        SaveWeaponSpawnStats();
    }

    private void ChangeLocation(int index)
    {
        if (_currentLevel != null)
        {
            Destroy(_currentLevel.gameObject);
        }

        _currentLevel = Instantiate(Levels[index], levelPosition.transform.position, Quaternion.identity);
        _targetPosition = _currentLevel.targetPosition;
        RenderSettings.skybox = _currentLevel.Skybox;
    }

    private void ChangeTarget(int index)
    {
        string levelindex = index >=9 ? "Level00" + (index + 1).ToString() : "Level000" + index.ToString();

        GameAnalytics.NewProgressionEvent (GAProgressionStatus.Start, levelindex);

        if (_currentTarget != null)
        {
            Destroy(_currentTarget.gameObject);
        }

        if (_currentTargetIndex >= targets.Length)
        {
            _currentTargetIndex-=5;
        }
        var newTarget = Instantiate(targets[index], _targetPosition.position, _targetPosition.rotation);
        newTarget.gameObject.SetActive(true);
        newTarget.targetDestroyed += () => ShowWinScreen();
        newTarget.targetDestroyed += () => InitTarget(++_currentTargetIndex);
        newTarget.targetDestroyed += () => GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, levelindex);
        newTarget.targetDamaged += (HP) => SaveTargetInfo(HP);
        _currentTarget = newTarget;
        _currentTarget.transform.SetParent(_targetPosition);
        spawner.SetupTarget(newTarget, _currentTargetIndex);
        

        ChangeSpawnButton();
    }

    public void ShowWinScreen()
    {
        winScreen.ShowWinScreen(MoneySystem.GetLevelMoney(), _currentTargetIndex+1);
        spawner.ReloadAllGuns();
        

        foreach (var particle in particles)
        {
            particle.Play();
        }

        //DOTween.Sequence().AppendInterval(3.5f).AppendCallback(() => winScreen.SetActive(false));
    }

    private void SetSpawnLevel()
    {
        switch (_currentSpawnWeaponLevel)
        {
            case 1:
                CurrentCost = 26;
                break;
            case 2:
                CurrentCost = 78;
                spawnToNextLevel = 40;
                break;
            case 3:
                CurrentCost = 234;
                spawnToNextLevel = 50;
                break;
            case 4:
                CurrentCost = 702;
                spawnToNextLevel = 60;
                break;
            case 5:
                CurrentCost = 1755;
                spawnToNextLevel = 70;
                break;
            
            case 6:
                CurrentCost = 4388;
                spawnToNextLevel = 80;
                break;
            case 7:
                CurrentCost = 10969;
                spawnToNextLevel = 90;
                break;
            case 8:
                CurrentCost = 27422;
                spawnToNextLevel = 90;
                break;
            case 9:
                CurrentCost = 54844;
                spawnToNextLevel = 100;
                break;
            case 10:
            default:
                CurrentCost = 109688;
                spawnToNextLevel = 110;
                _currentSpawnWeaponLevel = 10;
                break;
        }
    }


    private void ChangeSpawnButton()
    {
        if (_numberOfSpawns >= spawnToNextLevel)
        {
            _numberOfSpawns = 0;
            _currentSpawnWeaponLevel++;
            SetSpawnLevel();
            weaponProgressBar.InitWeaponProgressBar(spawnToNextLevel);
            spawner.RespawnGunsMinLevel(_currentSpawnWeaponLevel);
        }

        weaponImage.sprite = weaponSprites[_currentSpawnWeaponLevel-1];
        weaponCost.text = CurrentCost.ToString();
    }
    private void SaveWeaponSpawnStats()
    {
        PlayerPrefs.SetInt("WeaponLevel", _currentSpawnWeaponLevel);
        PlayerPrefs.SetInt("NumberOfSpawns", _numberOfSpawns);
        PlayerPrefs.SetInt("CurrentCost", CurrentCost);
        PlayerPrefs.SetInt("SpawnToNextLevel", spawnToNextLevel);
    }
    private void LoadWeaponSpawnStats()
    {
        spawnToNextLevel = PlayerPrefs.GetInt("SpawnToNextLevel", spawnToNextLevel);
        _currentSpawnWeaponLevel = PlayerPrefs.GetInt("WeaponLevel", startLevel);
        _numberOfSpawns = PlayerPrefs.GetInt("NumberOfSpawns", 0);
        CurrentCost = PlayerPrefs.GetInt("CurrentCost", CurrentCost);
    }
    private void SaveTargetInfo(int HP)
    {
        _currentTargetSaveHP = HP;
        PlayerPrefs.SetInt("TargetLevel", _currentTargetIndex);
        PlayerPrefs.SetInt("TargetCurrentHP", _currentTargetSaveHP);
    }
    private void LoadTargetInfo()
    {
        _currentTargetIndex = PlayerPrefs.GetInt("TargetLevel", 0);
        _currentTargetSaveHP = PlayerPrefs.GetInt("TargetCurrentHP", 0);
        if (_currentTargetSaveHP <= 0)
        {
            _currentTargetSaveHP = 0;
        }
    }

}
