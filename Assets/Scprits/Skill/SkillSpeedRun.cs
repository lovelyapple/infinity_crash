using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillSpeedRun : SkillBase
{
    private float _prevFov;
    private float _targetFov;
    private const float fillImageChargeTime = 0.5f;
    private Coroutine _fillImageCorutine = null;
    private List<Image> _fillImages;
    public override void Initialize(SkillSettingData settingData, GameCharaConttrollerNew player, Transform fieldObjTransform)
    {
        base.Initialize(settingData, player, fieldObjTransform);
        _prevFov = Camera.main.fieldOfView;
        _targetFov = _prevFov * 1.3f;
        _fillImages = GameHUDController.Instance.SpeedRunTimeLeftImages.ToList();
        _fillImages.ForEach(x  => x.gameObject.SetActive(true));
    }
    public override void OnSkillFire()
    {
        base.OnSkillFire();
        GameMainObject.Instance.StartCoroutine(ChangeFov(_targetFov, 2f));
        _fillImageCorutine = GameMainObject.Instance.StartCoroutine(StartSpeedImage());
    }
    public override void OnSkillpdate()
    {
        base.OnSkillpdate();

        if(_fillImageCorutine == null)
        {
            _fillImages.ForEach(x => x.fillAmount = (TimeLeft - fillImageChargeTime) / _settingData.Duration);
        }
    }
    public override void OnSkillFinished()
    {
        base.OnSkillFinished();
        GameMainObject.Instance.StartCoroutine(ChangeFov(_prevFov, 1f));

        if(_fillImageCorutine != null)
        {
            GameMainObject.Instance.StopCoroutine(_fillImageCorutine);
        }

        _fillImages.ForEach(x => x.gameObject.SetActive(false));
    }

    private IEnumerator ChangeFov(float target, float duration)
    {
        var prev = Camera.main.fieldOfView;
        var diff = prev - target;

        var elapseTime = 0f;
        while (elapseTime < duration)
        {
            Camera.main.fieldOfView = prev - diff * (elapseTime / duration);
            elapseTime += Time.deltaTime;
            yield return null;
        }

        Camera.main.fieldOfView = target;
    }
    private IEnumerator StartSpeedImage()
    {
        var elapseTime = 0f;
        while(elapseTime < fillImageChargeTime)
        {
            elapseTime += Time.deltaTime;
            _fillImages.ForEach(x => x.fillAmount = elapseTime / fillImageChargeTime);
            yield return null;
        }

        _fillImages.ForEach(x => x.fillAmount = 1);
        _fillImageCorutine = null;
    }
}
