using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPosition : MonoBehaviour
{
    public bool IsOccupied = false;
    [SerializeField] Image reloadImage;



    public IEnumerator Reload(int time)
    {
        reloadImage.gameObject.SetActive(true);

        float timer = 0f;

        while (timer < time)
        {
            timer += Time.deltaTime;
            // Update the filled image gradually from 0 to 1
            reloadImage.fillAmount = timer / time;
            yield return null;
        }

        reloadImage.fillAmount = 1f;
        reloadImage.gameObject.SetActive(false);
    }

}
