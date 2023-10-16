using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TrashBin : MonoBehaviour
{
    [SerializeField] Image trashImage;
    [SerializeField] Transform trashBinPosition;
    [SerializeField] Sprite onImage;
    [SerializeField] Sprite offImage;

    public float ShrinkDuration = 1f;


    public void DisposeTrash(Weapon trashObject)
    {
        trashObject.transform.DOMove(trashBinPosition.position, 0.01f);
        trashObject.transform.DOScale(Vector3.zero, ShrinkDuration);
        
    }

    public void ChangeImage(bool flag)
    {
        if (flag)
            trashImage.sprite = onImage;
        else
            trashImage.sprite = offImage;
    }
}
