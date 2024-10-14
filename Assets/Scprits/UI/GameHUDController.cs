using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDController : MonoBehaviour
{
    public static GameHUDController Instance;
    public GameObject SuperJumpObject;
    public List<UISkillIconController> SkillIconControllers;
    public List<UIApplicationCrashingController> UIApplicationCrashingControllers;
    public List<UIApplicationTrackingController> UIApplicationTrackings;

    public UITitleController TitleController;
    public UICountDownController CountDownController;
    public GameObject InGameHUDObj;
    public UIResultController ResultController;
    void Awake()
    {
        Instance = this;

        GameModel.Instance.OnGotoTitle += OnGotoTitle;
        GameModel.Instance.OnStartGame += OnGameStart;
        GameModel.Instance.OnFinished += OnGameFinished;
    }
    void OnDestory()
    {
        GameModel.Instance.OnGotoTitle -= OnGotoTitle;
        GameModel.Instance.OnStartGame -= OnGameStart;
        GameModel.Instance.OnFinished -= OnGameFinished;
    }

    public void OnCountDownStart()
    {
        TitleController.gameObject.SetActive(false);
        CountDownController.gameObject.SetActive(true);
        InGameHUDObj.SetActive(true);
        ResultController.gameObject.SetActive(false);
        GameMainObject.Instance.GameCharaController.enabled = true;
    }
    private void OnGameStart()
    {
        TitleController.gameObject.SetActive(false);
        CountDownController.gameObject.SetActive(false);
        InGameHUDObj.SetActive(true);
        ResultController.gameObject.SetActive(false);
        GameMainObject.Instance.GameCharaController.enabled = true;
    }
    private void OnGameFinished()
    {
        TitleController.gameObject.SetActive(false);
        CountDownController.gameObject.SetActive(false);
        InGameHUDObj.SetActive(true);
        ResultController.gameObject.SetActive(true);
        ResultController.SetupScore();
        GameMainObject.Instance.GameCharaController.enabled = false;
    }
    private void OnGotoTitle()
    {
        TitleController.gameObject.SetActive(true);
        CountDownController.gameObject.SetActive(false);
        InGameHUDObj.SetActive(false);
        ResultController.gameObject.SetActive(false);
        GameMainObject.Instance.GameCharaController.enabled = false;
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
