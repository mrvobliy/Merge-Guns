using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Switch : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] string saveName;
    [SerializeField] RectTransform toggleIndicator;
    [SerializeField] GameObject OnText;
    [SerializeField] GameObject OffText;
    [SerializeField] Color indicatorOnColor;
    [SerializeField] Color indicatorOffColor;
    [SerializeField] float tweenTime = 0.15f;
    [SerializeField] RectTransform onPosition;
    [SerializeField] RectTransform offPosition;


    public bool IsOn => _isOn;

    private bool _isOn = true;


    public delegate void ValueChanged(bool value);
    public event ValueChanged valueChanged;



    private void Awake()
    {
        Load();
        Toggle(_isOn);
        DOTween.Sequence().AppendInterval(0.1f)
                          .AppendCallback(() => MoveIndicator(_isOn))
                          .AppendCallback(() => ToggleColor(_isOn));
    }


    private void Toggle(bool value)
    {
        if (value != IsOn)
        {

            _isOn = value;
            ToggleColor(IsOn);
            MoveIndicator(IsOn);
            Save();

            if (valueChanged != null)
                valueChanged(IsOn);
        }
    }


    private void ToggleColor(bool value)
    {
        if (value)
        {
            toggleIndicator.GetComponentInChildren<Image>().DOColor(indicatorOnColor, tweenTime);
        }
        else
        {
            toggleIndicator.GetComponentInChildren<Image>().DOColor(indicatorOffColor, tweenTime);
        }
    }

    private void MoveIndicator(bool value)
    {
        if (value)
        {
            toggleIndicator.DOMove(onPosition.position, tweenTime);
            OnText.SetActive(true);
            OffText.SetActive(false);
        }
        else
        {
            toggleIndicator.DOMove(offPosition.position, tweenTime);
            OnText.SetActive(false);
            OffText.SetActive(true);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Toggle(!IsOn);
    }



    private void Load()
    {
        _isOn = intToBool(PlayerPrefs.GetInt(saveName, 1));
    }

    private void Save()
    {
        PlayerPrefs.SetInt(saveName, boolToInt(_isOn));
    }


    private int boolToInt(bool value)
    {
        if (value)
            return 1;
        else
            return 0;
    }

    private bool intToBool(int value)
    {
        if (value != 0)
            return true;
        else
            return false;
    }
}
