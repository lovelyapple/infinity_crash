using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSkillChooser : MonoSingletoneBase<GameSkillChooser>
{
    public List<SkillSettingData> SkillSettingDatas;

    public SkillSettingData GetOneSkillData()
    {
        var count = SkillSettingDatas.Count;
        var index = Random.Range(0, count);
        return SkillSettingDatas[index];
    }

    public static SkillBase CreateSkill(SkillSettingData skillSettingData)
    {
        switch (skillSettingData.Type)
        {
            case SkillType.SpeedRun:
                return new SkillSpeedRun();
            case SkillType.SuperJump:
                return new SkillSuperJump();
            case SkillType.AddTime:
                return new SkillTimeAdd();
            default:
                return null;
        }
    }
}
