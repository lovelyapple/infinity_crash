using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    None,
    SpeedRun,
    SuperJump,
    AddTime,
}
public enum SkillTimeType
{
    OneShot,
    ElapseTime,
}
public enum EffectType
{
    Field,
    PlayerScreen,
}
[Serializable]
public class SkillSettingData
{
    public SkillType Type;
    public bool IsAutoFire;
    public SkillTimeType TimeType;
    public float Duration;
    public EffectType EffectType;
    public ParticleSystem FieldEffectPrefab;
    public string ScreenEffectName;
    public bool CanStack;
}
[Serializable]
public class SkillBase
{
    public SkillType SkillType => _settingData.Type;
    public bool CanStack => _settingData.CanStack;
    public bool IsAutoFire => _settingData.IsAutoFire;
    public SkillTimeType TimeType => _settingData.TimeType;
    public ParticleSystem Effect { get; set; }
    public float TimeLeft { get; set; }
    public SkillSettingData _settingData;
    private bool _isRuning = false;
    public bool IsFinished = false;
    private GameCharaConttrollerNew _player;
    private Transform _fieldTransform;
    public virtual void Initialize(SkillSettingData settingData, GameCharaConttrollerNew player, Transform fieldObjTransform)
    {
        _settingData = settingData;
        _player = player;
        TimeLeft = _settingData.Duration;
        _fieldTransform = fieldObjTransform;
    }
    public virtual void OnSkillFire()
    {
        if (_settingData.TimeType == SkillTimeType.ElapseTime)
        {
            _isRuning = true;
        }
        else
        {
            IsFinished = true;
        }

        if (_settingData.FieldEffectPrefab != null)
        {
            Effect = GameObject.Instantiate(_settingData.FieldEffectPrefab);

            if (_settingData.EffectType == EffectType.Field)
            {
                Effect.gameObject.transform.localScale = Vector3.one;
                Effect.gameObject.transform.position = _fieldTransform.position;
            }
        }
        else if (!string.IsNullOrEmpty(_settingData.ScreenEffectName))
        {
            ResourceManager.Instance.TurnOnEffect(_settingData.ScreenEffectName);
        }
    }
    public virtual void OnSkillpdate()
    {
        if (!_isRuning)
        {
            return;
        }

        TimeLeft -= Time.deltaTime;

        if (TimeLeft <= 0)
        {
            _isRuning = false;
            IsFinished = true;
        }
    }
    public virtual void OnSkillFinished()
    {
        if (Effect)
        {
            // GameObject.Destroy(Effect);
        }

        if (!string.IsNullOrEmpty(_settingData.ScreenEffectName) && _settingData.TimeType != SkillTimeType.OneShot)
        {
            ResourceManager.Instance.TurnOffEffect(_settingData.ScreenEffectName);
        }

        _player.OnRemoveSkill(this);
        _settingData = null;
    }
}