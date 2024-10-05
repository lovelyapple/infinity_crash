using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    None,
    SpeedRun,
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
    public SkillTimeType TimeType;
    public float Duration;
    public EffectType EffectType;
    public ParticleSystem EffectPrefab;
}
public class ISkillBase
{
    public SkillType Type { get; }
    public ParticleSystem Effect { get; set; }
    public float TimeLeft { get; set; }
    public void Initialize(SkillSettingData settingData, Transform fieldObjTransform)
    {
        if (settingData.EffectPrefab != null)
        {
            Effect = GameObject.Instantiate(settingData.EffectPrefab);

            if (settingData.EffectType == EffectType.Field)
            {
                Effect.gameObject.transform.SetParent(fieldObjTransform, false);
            }
            else
            {

            }
        }
    }
    public void OnUpdate()
    {

    }
    public void Finished()
    {

    }
}
