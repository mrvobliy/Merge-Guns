using System.Collections.Generic;
using UnityEngine;
using RayFire;
using RayFire.DotNet;
using System.Linq;
using DG.Tweening;
using System;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Target : MonoBehaviour
{
    [SerializeField] Quaternion targetRotation;
    [SerializeField] List<Transform> targetRoots = new List<Transform>();
    [SerializeField] RayfireBomb bomb;
    public int maxHP;

    private int _currentHP;
    private float _healtNormalized;
    private ProgressBarController _progressBarController;
    private CoinManager _coinManager;
    private bool _isDestroyed;
    private bool _hasInvoked;

    public event Action targetDestroyed;
    public event Action<int> targetDamaged;


    void Awake()
    {
        _currentHP = maxHP;
    }

    public void Init(ProgressBarController progressBar, CoinManager coinManager, int index)
    {
        _progressBarController = progressBar;
        _progressBarController.InitTargetProgressBar(maxHP,index+1);
        _progressBarController.SetProgress(_currentHP);
        _coinManager = coinManager;
        transform.rotation = targetRotation;
        
    }

    public void ReceiveDamage(int damage)
    {
        if ((_currentHP -= damage) <= 0)
        {
            
            targetDamaged?.Invoke((int)_currentHP);
            GetBoomBOOZELED();
            return;
        }
        _progressBarController.SetProgress(_currentHP);
        
        targetDamaged?.Invoke((int)_currentHP);
    }

    public Transform GetTargetToShoot(float damage)
    {
        if (_isDestroyed)
            return null;

        //Нормируем хп от 0 до 1. Дальше выделяем промежутки в % для попадания в опр. зону. Потом в зависимости от текущего хп мы выдаём случайный трансформ из группы.
        _healtNormalized = (float)_currentHP/maxHP;
        float step = 1f/targetRoots.Count;

        if (!_hasInvoked)
        {
            for(int i = targetRoots.Count; i > 1; i--)
            {
                if (i*step >= _healtNormalized)
                {
                    DOTween.Sequence().AppendInterval(0.4f).AppendCallback(() => 
                    {
                        if (targetRoots[i] != null)
                        {
                            var targetsToDestroy = targetRoots[i].GetComponentsInChildren<Rigidbody>(true).Where(x => x.gameObject != targetRoots[i].gameObject).ToList();
                            for (int j = 0; j < targetsToDestroy.Count; j++)
                            {
                                var target = targetsToDestroy[j];
                                targetsToDestroy[j].isKinematic = false;
                                targetsToDestroy[j].AddForce(new Vector3(Random.Range(-10f, 10f),Random.Range(5f, 15f),Random.Range(-10f, 15f)));
                                Destroy(targetsToDestroy[j].gameObject, 5f);
                            }
                        }
                        
                    });
                    
                }
                else
                {
                    break;
                }
            }
            _hasInvoked = true;
            DOTween.Sequence().AppendInterval(0.25f).AppendCallback(() => _hasInvoked = false);
        }

        for(int i = 0; i < targetRoots.Count; i++)
        {
            if ((i+1)*step >= _healtNormalized)
            {
                int temp = i;
                if (i != 0)
                {
                    temp--;
                }
                
                var target = targetRoots[temp].GetComponentsInChildren<Rigidbody>().Where(x => x.gameObject != targetRoots[i].gameObject).ToList() != null ? targetRoots[temp].GetComponentsInChildren<Rigidbody>().Where(x => x.gameObject != targetRoots[i].gameObject).ToList() : null;
                if (target == null)
                {
                    return null;
                }

                int index = Random.Range(0, target.Count);
                DOTween.Sequence().AppendInterval(0.4f).AppendCallback( () => 
                {
                    ReceiveDamage((int)damage);
                    if (target[index] != null)
                    {
                        target[index].GetComponent<Rigidbody>().isKinematic = false;
                        target[index].GetComponent<Rigidbody>().AddForce(Vector3.forward*100);
                    }
                }).Play();
                DestroyPreviousParts();
                return target[index].transform;
            }
        }

        Debug.LogWarning("НЕ НАШЁЛ ЦЕЛЬ");
        return targetRoots.LastOrDefault();
    }
    private void DestroyPreviousParts()
    {
        _healtNormalized = (float)_currentHP/maxHP;
        float step = 1f/targetRoots.Count;

        foreach (var root in targetRoots)
        {
            var index = targetRoots.FindIndex(a => a == root);
            if (index * step >= _healtNormalized)
            {
                DOTween.Sequence().AppendInterval(0.3f).AppendCallback(() => 
                {
                    if (root != null)
                    {
                        var targetsToDestroy = root.GetComponentsInChildren<Rigidbody>(true).Where(x => x.gameObject != root.gameObject).ToList();
                        for (int j = 0; j < targetsToDestroy.Count; j++)
                        {
                            var target = targetsToDestroy[j];
                            targetsToDestroy[j].isKinematic = false;
                            targetsToDestroy[j].AddForce(new Vector3(Random.Range(-10f, 10f),Random.Range(5f, 15f),Random.Range(-10f, 15f)));
                            Destroy(targetsToDestroy[j].gameObject, 2f);
                        }
                    }
                });
            }
        }
    }

    public void ReloadWithoutParts(int damage)
    {
        _currentHP = maxHP-damage;
        _healtNormalized = (float)_currentHP/maxHP;
        float step = 1f/targetRoots.Count;

        for(int i = targetRoots.Count; i > 1; i--)
        {
            if (i*step >= _healtNormalized)
            {
                if (targetRoots[i-1] != null)
                {
                    var targetsToDestroy = targetRoots[i-1].GetComponentsInChildren<Rigidbody>(true).Where(x => x.gameObject != targetRoots[i-1].gameObject).ToList();
                    for (int j = 0; j < targetsToDestroy.Count; j++)
                    {
                        var target = targetsToDestroy[j];
                        targetsToDestroy[j].isKinematic = false;
                        targetsToDestroy[j].AddForce(new Vector3(Random.Range(-10f, 10f),Random.Range(5f, 15f),Random.Range(-10f, 15f)));
                        Destroy(targetsToDestroy[j].gameObject, 0.01f);
                    }
                }
            }
        }
    }


    
    private void GetBoomBOOZELED()
    {
        //bomb.Explode(0.1f);
        if (!_isDestroyed)
        {
            DOTween.Kill(gameObject);
            _isDestroyed = true;
            targetDestroyed?.Invoke();
        }
    }

    private Vector2 GetCanvasPosition(Vector3 worldPosition)
    {
        // Get the camera that is rendering the canvas
        Camera canvasCamera = Camera.main;

        // Convert the 3D world position to a screen point
        Vector3 screenPoint = canvasCamera.WorldToScreenPoint(worldPosition);

        // Convert the screen point to a canvas position
        RectTransform canvasRectTransform = _coinManager.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRectTransform.sizeDelta;
        Vector2 canvasPosition = new Vector2(screenPoint.x / Screen.width * canvasSize.x, screenPoint.y / Screen.height * canvasSize.y);

        return canvasPosition;
    }


}
