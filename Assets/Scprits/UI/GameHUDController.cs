using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDController : MonoBehaviour
{
    public static GameHUDController Instance;
    public GameObject SuperJumpObject;
    public List<UISkillIconController> SkillIconControllers;
    public List<UIApplicationCrashingController> UIApplicationCrashingControllers;
    public List<UIApplicationTrackingController> UIApplicationTrackings;
    void Awake()
    {
        Instance = this;
    }
    public void Initialize(GameCharaConttroller gameCharaConttroller)
    {

    }

    public void UpdateSkills(List<SkillBase> skillBases)
    {
        SkillIconControllers.ForEach(ctrl =>
        {
            var cnt = skillBases.Where(skill => skill.SkillType == ctrl.SkillType).Count();

            if (cnt > 0)
            {
                ctrl.RemainCntLabel.text = cnt.ToString();
                ctrl.RemainCntLabel.gameObject.SetActive(cnt != 1);
                ctrl.gameObject.SetActive(true);
            }
            else
            {
                ctrl.gameObject.SetActive(false);
            }
        });

        SuperJumpObject.gameObject.SetActive(skillBases.Any(x => x.SkillType == SkillType.SuperJump));
    }
}
