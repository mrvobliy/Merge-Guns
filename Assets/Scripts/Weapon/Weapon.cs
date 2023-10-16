using System.Collections;
using DG.Tweening;
using UnityEngine;
using RayFire;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using System;
using TMPro;

[RequireComponent(typeof(RayfireGun))]
public class Weapon : MonoBehaviour
{
    [SerializeField] float recoilAngle = 15f;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletPosition;
    [SerializeField] float bulletSpeed = 0.5f;
    [SerializeField] float fireRate = 0.2f;
    [SerializeField] int numberOfShots = 5;
    [SerializeField] int damagePerShot = 20;
    [SerializeField] Quaternion inGameRotation;
    [SerializeField] Vector3 inGameScale;
    [SerializeField] GameObject inGameModel;
    [SerializeField] GameObject onHitParticlePrefab;
    [SerializeField] GameObject muzzlePrefab;
    [SerializeField] Image reloadImage;
    [SerializeField] TMP_Text levelText;

    public int TotalDagame = 15;
    public Cell CurrentCell;
    public bool IsReloading = false;
    public int weaponLevel = 1;


    private Vector3 _oldPosition;
    private bool isDragging = false;
    public bool _isShooting;
    private bool _isWeaponModel;
    private Plane _plane;
    private RayfireGun _rayFireGun;
    private float distance = 100f;
    private Spawner _spawner;
    private float _timer = 0f;
    private bool _hasHit;

    public event Action<int> GunReload;


    void Awake()
    {
        _rayFireGun = GetComponent<RayfireGun>();
        _rayFireGun.showRay = false;
        _rayFireGun.showHit = false;
        _rayFireGun.damage = damagePerShot;
        _rayFireGun.rounds = numberOfShots;
        _rayFireGun.rate = fireRate;
    }


    public void InitCell(Spawner spawner)
    {
        _spawner = spawner;
        levelText.gameObject.SetActive(true);
        levelText.text = weaponLevel.ToString();
    }

    public void InitView()
    {
        transform.localRotation = inGameRotation;
        inGameModel.transform.localScale = inGameScale;
        _isWeaponModel = true;
    }

    public void StartReload()
    {
        StartCoroutine(Reload());
        reloadImage.gameObject.SetActive(false);
    }


    void Update()
    {

        //Debug.DrawLine(transform.position, transform.position - transform.forward * 10000, Color.red, 0.1f);

        if (Input.GetMouseButton(0) && isDragging)
        {
            Move();
        }
    }

    public void StopShooting()
    {
        StopCoroutine("ShootRoutine");
    }

    private void OnMouseDown()
    {
        isDragging = true;
        _plane = new Plane(CurrentCell.WeaponCellPosition.transform.forward, CurrentCell.WeaponCellPosition.transform.position);

        _oldPosition = transform.position;
        CurrentCell.ChangeBarrierState(true);
    }

    private void OnMouseUp() 
    {
        Physics.IgnoreLayerCollision(gameObject.layer, 7);
        isDragging = false;
        RaycastHit hit;
        if (Physics.Linecast(transform.position, transform.position - transform.forward * 1000, out hit))
        {
            Debug.Log(hit.collider.gameObject.name);
            if (hit.transform.CompareTag("Cell"))
            {
                var newCell = hit.collider.gameObject.GetComponent<Cell>();
                if (newCell.IsEmpty == true)
                {
                    //перемещение в новую ячейку

                    CurrentCell.Leave();

                    CurrentCell = newCell;
                    CurrentCell.InitNewCellWeapon(this);
                    
                    _oldPosition = CurrentCell.WeaponCellPosition.position;
                }
                else
                {
                    TryMerge(newCell);
                }
                
            }
            else if(hit.transform.CompareTag("Bin"))
            {
                hit.collider.gameObject.GetComponent<TrashBin>().DisposeTrash(this);
                DOTween.Sequence().AppendInterval(hit.collider.gameObject.GetComponent<TrashBin>().ShrinkDuration).AppendCallback(() => 
                {
                    CurrentCell.Clear();
                });
            }
            else 
            {
                transform.position = _oldPosition;
            }
            
        }
        else
        {
            transform.position = _oldPosition;
        }
        CurrentCell.ChangeBarrierState(false);
    }

    public void Shot(Target target)
    {
        if (!_isShooting && !IsReloading)
        {
            _isShooting = true;
            StartCoroutine(ShootRoutine(target));
            IsReloading = true;
            StartReload();
            GunReload?.Invoke(weaponLevel);
            
        }

    }

    public void StopReload()
    {
        _timer = 99f;
    }

    private IEnumerator ShootRoutine(Target target)
    {
        for (int i = 0; i < numberOfShots; i++)
        {
            // 1) Start the shooting animation
            //weaponAnimation.Play("ShootAnim");
            //DOTween.Sequence().AppendCallback(() => transform.DOLocalRotate(inGameRotation.eulerAngles + new Vector3(0f,0f,-10f), fireRate/4))
                                  //.OnComplete(() => transform.DOLocalRotate(inGameRotation.eulerAngles, fireRate/4)).Play();

            // 2) Find Target
            try
            {
                Transform shootTarget = target.GetTargetToShoot(damagePerShot);
                if (shootTarget == null)
                {
                    break;
                }
                

                Vector3 direction = shootTarget.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            
                transform.DORotateQuaternion(Quaternion.Euler(transform.position.x, rotation.eulerAngles.y + 90f, transform.position.z), 0.1f).OnComplete(() => {
                    
                    DOTween.Sequence().AppendCallback(() => transform.DOLocalRotate(transform.localEulerAngles + new Vector3(0f,0f,-recoilAngle), fireRate/2))
                                    .AppendInterval(fireRate/2).AppendCallback(() => transform.DOLocalRotate(transform.localEulerAngles + new Vector3(0f,0f,recoilAngle), fireRate/2));
                    
                    GameObject bullet = Instantiate(bulletPrefab, bulletPosition.position, Quaternion.identity);
                    GameObject muzzle = Instantiate(muzzlePrefab, bulletPosition.position, Quaternion.identity);
                    Destroy(bullet.gameObject, 1f);
                    Destroy(muzzle.gameObject, 1.5f);
                    bullet.transform.LookAt(shootTarget);
                    if (bullet.tag == "FX")
                        bullet.GetComponent<ECExplodingProjectile>().Init(shootTarget);

                    bullet.transform.DOMove(shootTarget.position, Vector3.Distance(bullet.transform.position, shootTarget.position)/bulletSpeed).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        if (shootTarget != null && !_hasHit)
                        {
                            _hasHit = true;
                            shootTarget.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-50f, 50f),Random.Range(30f, 50f),Random.Range(30f, 100f)));
                        }
                        GameObject hitEffect = Instantiate(onHitParticlePrefab, bullet.transform.position, Quaternion.identity);
                        Destroy(hitEffect, 5f);
                        Destroy(bullet);
                    });
                });
            }
            catch (System.NullReferenceException)
            {
                Debug.Log("OSHIBKA");
            }
            

            // 3) Instantiate a bullet at the desired position
            

            // GameObject effectPlayer = Instantiate(shootEffect, bulletPosition.position, Quaternion.identity, bulletPosition);
            // effectPlayer.transform.localScale = new Vector3(2f,2f,2f);
            // effectPlayer.transform.localPosition = Vector3.zero;

            // 4) Make the bullet move towards the target
            // bullet.transform.DOMove(shootTarget.position, Vector3.Distance(bullet.transform.position, shootTarget.position)/bulletSpeed).SetEase(Ease.Linear).OnComplete(() =>
            // {
            //     // Add your code here to handle hitting the target
            //     shootTarget.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-50f, 50f),Random.Range(30f, 50f),Random.Range(30f, 100f)));
            //     _rayFireGun.Shoot();
            //     GameObject hitEffect = Instantiate(onHitParticlePrefab, bullet.transform.position, Quaternion.identity);
            //     Destroy(hitEffect, 1f);
            //     //Destroy(effectPlayer.gameObject);
            // });

            // Delay before the next shot
            yield return new WaitForSeconds(fireRate);
        }
        _isShooting = false;
        transform.DOLocalRotate(inGameRotation.eulerAngles, 0.5f);
        _hasHit = false;
        StopCoroutine("Reload");
        StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
         if(_isWeaponModel)
             reloadImage.gameObject.SetActive(true);

        _timer = 0f;
        reloadImage.color = Color.green;
        // reloadImage.DOColor(Color.green, weaponLevel);

        while (_timer < (float)weaponLevel/2)
        {
            _timer += Time.deltaTime;
            // Update the filled image gradually from 0 to 1
            reloadImage.fillAmount = _timer / ((float)weaponLevel/2);
            yield return null;
        }

        IsReloading = false;
        reloadImage.fillAmount = 1f;
        reloadImage.gameObject.SetActive(false);
    }

    private void Move()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        _plane.Raycast(ray, out distance);
        Vector3 point = ray.GetPoint(distance);

        transform.position = point;
    }

    private void TryMerge(Cell cell)
    {
        if (CurrentCell.CurrentWeaponLevel == cell.CurrentWeaponLevel && weaponLevel < 13)
        {
            Upgrade(cell);
        }
        else
        {
            CurrentCell.ChangeBarrierState(false);
            var otherCellWeapon = cell.WeaponCellView;
            CurrentCell.InitNewCellWeapon(otherCellWeapon);
            cell.InitNewCellWeapon(this);
        }

    }

    private void Upgrade(Cell cell)
    {
        var cellweapon = cell.WeaponCellView.transform;
        transform.position = cell.WeaponCellPosition.position;

        //Соединяем вместе, потом играем партиклы и удаляем старое + спавним новое
        Sequence upgradeTween = DOTween.Sequence();

        upgradeTween.AppendCallback(() => 
        {
            Vector3 pos1 = cell.WeaponCellPosition.position + Vector3.left * 0.4f;
            transform.DOMove(pos1, 0.1f);
            Vector3 pos2 = cell.WeaponCellPosition.position + Vector3.right * 0.4f;
            cellweapon.DOMove(pos2, 0.1f);}
        ).AppendInterval(0.1f);

        upgradeTween.Append(transform.DOMoveX(cell.WeaponCellPosition.position.x, 0.1f).SetEase(Ease.InOutQuad));
        upgradeTween.Join(cellweapon.DOMoveX(cell.WeaponCellPosition.position.x, 0.1f).SetEase(Ease.InOutQuad));
        upgradeTween.AppendCallback(() => 
        {
            if (Vibration.IsON)
            {
                Vibration.Vibrate(250);
                Debug.Log("BRRRRRR");
            }
            cell.PlayParticles();
            cell.Clear();
            CurrentCell.Clear();
            weaponLevel++;
            _spawner.SpawnWeapon(weaponLevel, cell);
        });
    }

}
