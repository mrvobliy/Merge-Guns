using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour
{
    [SerializeField] GameObject cellMergeParticles;
    [SerializeField] Vector3 particleOffset;
    [SerializeField] GameObject CellBarrier;
    public Weapon WeaponCellView;
    public Transform WeaponCellPosition;
    public int CurrentWeaponLevel = 0;
    public bool IsEmpty = true;
    public event Action<int, Cell> OnWeaponDestroyedInCell;

    
    public void InitNewCellWeapon(Weapon weapon)
    {
        CurrentWeaponLevel = weapon.weaponLevel;
        WeaponCellView = weapon;
        weapon.transform.SetParent(WeaponCellPosition);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        weapon.CurrentCell = this;
        IsEmpty = false;

        //InitWeaponView(weapon);
    }

    public void Clear()
    {
        OnWeaponDestroyedInCell?.Invoke(CurrentWeaponLevel, this);
        Destroy(WeaponCellView.gameObject);
        CurrentWeaponLevel = 0;
        IsEmpty = true;
    }

    public void Leave()
    {
        WeaponCellView = null;
        CurrentWeaponLevel = 0;
        IsEmpty = true;
        ChangeBarrierState(false);
    }

    public void ChangeBarrierState(bool flag)
    {
        CellBarrier.SetActive(flag);
    }

    public void PlayParticles()
    {
        var particles = Instantiate(cellMergeParticles, WeaponCellPosition);
        particles.transform.localEulerAngles = Vector3.zero;
        particles.transform.localPosition = particleOffset;
    }

}
