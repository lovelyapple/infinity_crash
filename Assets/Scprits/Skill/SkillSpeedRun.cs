using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSpeedRun : SkillBase
{
    private float _prevFov;
    private float _targetFov;
    public override void Initialize(SkillSettingData settingData, GameCharaConttroller player, Transform fieldObjTransform)
    {
        base.Initialize(settingData, player, fieldObjTransform);
        _prevFov = Camera.main.fieldOfView;
        _targetFov = _prevFov * 1.3f;
    }
    public override void OnSkillFire()
    {
        base.OnSkillFire();
        GameMainObject.Instance.StartCoroutine(ChangeFov(_targetFov, 2f));
    }
    public override void OnSkillFinished()
    {
        base.OnSkillFinished();
        GameMainObject.Instance.StartCoroutine(ChangeFov(_prevFov, 1f));
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
}
