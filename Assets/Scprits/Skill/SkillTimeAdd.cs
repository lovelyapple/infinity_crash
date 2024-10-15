using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTimeAdd : SkillBase
{
    public const float TimeAdd = 10f;
    public override void OnSkillFire()
    {
        base.OnSkillFire();

        GameModel.Instance.AddTime();
    }
}
