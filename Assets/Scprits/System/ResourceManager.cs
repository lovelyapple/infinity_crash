using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoSingletoneBase<ResourceManager>
{
    public List<SkillSettingData> SkillSettingDatas;
    [SerializeField] List<GameObject> ScreenEffects;
    public void TurnOnEffect(string effectName)
    {
        foreach (var f in ScreenEffects)
        {
            if (f.gameObject.name == effectName)
            {
                f.gameObject.SetActive(true);
                return;
            }
        }
    }
    public void TurneOffEffect(string effectName)
    {
        foreach (var f in ScreenEffects)
        {
            if (f.gameObject.name == effectName)
            {
                f.gameObject.SetActive(false);
                return;
            }
        }
    }
}
