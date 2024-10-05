using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSpeedRun : ISkillBase
{
    public SkillType Type => SkillType.SpeedRun;
    public ParticleSystem Effect { get; set; }
    public float TimeLeft { get; set; }
    public void Initialize(SkillSettingData settingData)
    {

    }
    public void OnUpdate()
    {

    }
    public void Finished()
    {

    }
}
