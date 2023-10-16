using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class Spawner : MonoBehaviour
{
    public Weapon[] weaponPrefabs;
    [SerializeField] List<WeaponPosition> spawnPoints = new List<WeaponPosition>();
    [SerializeField] Cell[] cells;
    [SerializeField] Target target;
    [SerializeField] ProgressBarController progressBarController;
    [SerializeField] CoinManager coinManager;
    [SerializeField] float spacingBetweenWeapons = 2f;
    [SerializeField] Transform centeralPosition;
    [SerializeField] GameObject weaponSpawnParticles;
    [SerializeField] GameObject buttonLock;
    [SerializeField] GameObject SpawnButton;
    [SerializeField] LevelController levelController;
    [SerializeField] WeaponUnlockScreen weaponUnlockScreen;

    public List<Weapon> _activeWeapons = new List<Weapon>();

    
    private int _currentMaxLevel = 1;


    private void FixedUpdate()
    {
        if (CheckIfAllCellsOccupied() || MoneySystem.GetCurrentMoney() < levelController.CurrentCost)
        {
            buttonLock.SetActive(true);
            SpawnButton.GetComponent<Button>().enabled = false;
        }
        else
        {
            buttonLock.SetActive(false);
            SpawnButton.GetComponent<Button>().enabled = true;
        }    
    }
    
    void Awake()
    {
        LoadMaxWeaponLevel();
        Debug.Log(_currentMaxLevel);
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].OnWeaponDestroyedInCell += (level, cell) => DestroyWeaponRepresentation(level, cell);
            int weaponLevel = LoadWeaponLevel(i);
            if (weaponLevel != 0)
            {
                SpawnWeapon(weaponLevel, cells[i]);
            }
        }
        
        if (cells[0].CurrentWeaponLevel == -1)
        {
            SpawnWeapon(1, cells[0]);
        }

        DOTween.Sequence().AppendInterval(0.4f).AppendCallback(() =>SortWeaponRepresentations());
    }

    public void RespawnGunsMinLevel(int minLevel)
    {
        int minlevelgunsnum = 0;
        for (int i = 0; i < cells.Length; i++)
        {
            if (!cells[i].IsEmpty && cells[i].WeaponCellView.weaponLevel < minLevel)
            {
                minlevelgunsnum++;
            }
        }
        if (minlevelgunsnum % 2 == 1)
        {
            foreach (var cell in cells)
            {
                if (!cell.IsEmpty && cell.WeaponCellView.weaponLevel < minLevel)
                {
                    cell.Clear();
                    SpawnWeapon(minLevel, cell);
                    return;
                }
            }
        }
        
    }

    public bool CheckIfAllCellsOccupied()
    {
        foreach (var cell in cells)
        {
            if (cell.IsEmpty)
            {
                return false;
            }
        }
        return true;
    }

    public void ReloadAllGuns()
    {
        foreach (var gun in _activeWeapons)
        {
            gun.StopReload();
            gun.transform.DOLocalRotate(new Vector3(0f,180f,0f), 0.7f);
        }
    }

    public void SetupTarget(Target newTarget, int index)
    {
        target = newTarget;
        newTarget.Init(progressBarController, coinManager, index);
        target.targetDestroyed += StopShooting;
    }

    public void SpawnWeapon(int level = 1, Cell nextCell = null)
    {
        if(nextCell == null)
        {
            foreach (Cell cell in cells)
            {
                if (cell.WeaponCellView == null)
                {
                    nextCell = cell;
                    break;

                }
            }
        }
        

        if (nextCell != null)
        {
            var WeaponCell = Instantiate(weaponPrefabs[level-1], transform.position, Quaternion.identity);
            nextCell.InitNewCellWeapon(WeaponCell);
            WeaponCell.InitCell(this);

            SaveWeaponLevel(Array.IndexOf(cells, nextCell), level);

            //First Prefab
            SpawnModel(level);
        }
    }

    public void ShootAllGuns()
    {
        foreach(var weapon in _activeWeapons)
        {
            if (!weapon.IsReloading)
            {
                weapon.Shot(target);
                MoneySystem.IncreaseLevelMoney(weapon.TotalDagame);
                DOTween.Sequence().AppendInterval(0.4f).AppendCallback(() => coinManager.AddCoins(1, weapon.TotalDagame));
                
            } 
        }
    }

    public void SpawnModel(int level)
    {
        
        if (level > _currentMaxLevel)
        {
            _currentMaxLevel = level;
            weaponUnlockScreen.ShowWeaponScreen(_currentMaxLevel-2);
        }

        WeaponPosition weaponPosition = spawnPoints.Find(sp => sp.IsOccupied == false);
        weaponPosition.IsOccupied = true;

        var particles = Instantiate(weaponSpawnParticles, weaponPosition.transform.position, weaponPosition.transform.rotation);
        particles.transform.SetParent(weaponPosition.transform);
        particles.transform.localPosition = Vector3.zero;
        particles.transform.localEulerAngles = new Vector3(-90f,0f,0f);
        
        var weaponInstance = Instantiate(weaponPrefabs[level-1], weaponPosition.transform.position, weaponPosition.transform.rotation);
        weaponInstance.transform.SetParent(weaponPosition.transform);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.GetComponent<BoxCollider>().enabled = false;
        
        weaponInstance.InitView();
        
        Destroy(particles, 1.5f);

        weaponInstance.GunReload += CellReload;

        _activeWeapons.Add(weaponInstance);

        SortWeaponRepresentations();

        var oldScale = weaponInstance.transform.localScale;
        
        weaponInstance.transform.localScale = Vector3.zero;
        weaponInstance.transform.DOScale(oldScale, 0.4f);
    }

    private void DestroyWeaponRepresentation(int level, Cell cell)
    {
        var weaponRepresentation = _activeWeapons.Find(weapon => weapon.weaponLevel == level);
        SaveWeaponLevel(Array.IndexOf(cells, cell), 0);

        if (weaponRepresentation != null)
        {  
            _activeWeapons.Remove(weaponRepresentation);
            Destroy(weaponRepresentation.gameObject);
            SortWeaponRepresentations();
        }
    }

    private void SortWeaponPositions(Transform center)
    {
        switch (_activeWeapons.Count)
        {
            case 1:
            case 2:
            case 3:
                foreach (var weapon in _activeWeapons)
                {
                    weapon.transform.localScale = new Vector3(1.5f,1.5f,1.5f);
                    spacingBetweenWeapons = 0.36f;
                }
                break;

            case 4:
            case 5:
            case 6:
                foreach (var weapon in _activeWeapons)
                {
                    weapon.transform.localScale = new Vector3(1.2f,1.2f,1.2f);
                    spacingBetweenWeapons = 0.32f;
                }
                break;

            case 7:
            case 8:
            case 9:
                foreach (var weapon in _activeWeapons)
                {
                    weapon.transform.localScale = new Vector3(1f,1f,1f);
                    spacingBetweenWeapons = 0.28f;
                }
                break;

            default:
            break;
        }
        //нечетное число, располагаем от центральной позиции
        if (_activeWeapons.Count % 2 != 0)
        {
            //Размещаем в центре, потом на одинаковом удалении слева и справа от центра
            spawnPoints[0].transform.position = center.position;
            
            for (int i = 1; i < spawnPoints.Count; i+=2)
            {
                spawnPoints[i].transform.position = center.position + new Vector3(spacingBetweenWeapons * (i+1), 0f, 0f);
                spawnPoints[i+1].transform.position  = center.position - new Vector3(spacingBetweenWeapons * (i+1), 0f, 0f);
                
            }
        } //четное число, распалагаем влево-вправо сразу
        else
        {
            for (int i = 0; i < spawnPoints.Count-1; i+=2)
            {
                spawnPoints[i].transform.position = center.position + new Vector3(spacingBetweenWeapons * (i+1), 0f, 0f);
                spawnPoints[i+1].transform.position  = center.position - new Vector3(spacingBetweenWeapons * (i+1), 0f, 0f);
            }
        }
    }

    private void CellReload(int level)
    {
        foreach (var cell in cells)
        {
            if (cell.WeaponCellView!= null && cell.WeaponCellView.weaponLevel == level && !cell.WeaponCellView.IsReloading)
            {
                cell.WeaponCellView.StartReload();
            }
        }
    }

    private void StopShooting()
    {
        foreach (var weapon in _activeWeapons)
        {
            weapon.StopShooting();
        }
    }

    public void SortWeaponRepresentations()
    {
        foreach(var pos in spawnPoints)
        {
            pos.IsOccupied = false;
        }
        
        for (int i = 0; i < _activeWeapons.Count; i++)
        {
            _activeWeapons[i].transform.SetParent(spawnPoints[i].transform);
            _activeWeapons[i].transform.localPosition = Vector3.zero;
            spawnPoints[i].IsOccupied = true;
        }

        SortWeaponPositions(centeralPosition);
    }

    private void SaveWeaponLevel(int cellNumber, int weaponLevel)
    {
        string key = "WeaponCell" + cellNumber.ToString();
        PlayerPrefs.SetInt(key, weaponLevel);
        if (weaponLevel > _currentMaxLevel)
            PlayerPrefs.SetInt("MaxLevel", weaponLevel);
        PlayerPrefs.Save();
    }

    private void LoadMaxWeaponLevel()
    {
        _currentMaxLevel = PlayerPrefs.GetInt("MaxLevel", 1);
    }

    // Load the weapon level for a specific slot
    private int LoadWeaponLevel(int cellNumber)
    {
        string key = "WeaponCell" + cellNumber;
        return PlayerPrefs.GetInt(key, 0); // Default level is 0 if the key is not found
    }

}
