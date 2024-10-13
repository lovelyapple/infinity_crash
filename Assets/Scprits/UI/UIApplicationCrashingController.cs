using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIApplicationCrashingController : MonoBehaviour
{
    public ApplicationType ApplicationType;
    public Image FillIamge;
    public GameObject CrashedObjRoot;
    private PressureInfo _pressureInfo;
    private float _fillRate;

    public void Init(PressureInfo pressureInfo)
    {
        _pressureInfo = pressureInfo;
        gameObject.SetActive(true);
    }
    public void Update()
    {
        var curPres = _pressureInfo.Pressure;
        var maxDiff = ApplicationPressureManager.Instance.PressureCrashing;

        _fillRate = curPres / maxDiff;

        if(_fillRate > 1)
        {
            _fillRate = 1;
        }

        FillIamge.fillAmount = _fillRate;
    }
    public void OnStartCrashing()
    {
        gameObject.SetActive(false);
    }
    public void OnReview()
    {
        gameObject.SetActive(true);
    }
}
