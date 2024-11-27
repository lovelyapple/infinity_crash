using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITitleController : MonoBehaviour
{
    [SerializeField] private GameObject StartButton;
    [SerializeField] private UITutorialOneImage FirstRuleTutorial;
    [SerializeField] private UITutorialOneImage LastTutorialOneImage;
    private const string HasShowDescriptionPrefsKey = "DescriptionShown";
    private void Awake()
    {
        GameModel.Instance.OnStartGame += OnGameStart;
        GameModel.Instance.OnFinished += OnGameFinished;
        GameModel.Instance.OnGotoTitle += OnGotoTitle;
        GameMainObject.Instance.DoFOn();
    }
    private void OnDestroy()
    {
        GameModel.Instance.OnStartGame -= OnGameStart;
        GameModel.Instance.OnFinished -= OnGameFinished;
        GameModel.Instance.OnGotoTitle -= OnGotoTitle;
    }
    private void OnGameStart()
    {

    }
    private void OnGameFinished()
    {

    }
    private void OnGotoTitle()
    {

    }
    public void OnClickStart()
    {
        var shown = PlayerPrefs.GetInt(HasShowDescriptionPrefsKey, 0) != 0;

        if (!shown)
        {
            FirstRuleTutorial.gameObject.SetActive(true);
            StartButton.gameObject.SetActive(false);
            PlayerPrefs.SetInt(HasShowDescriptionPrefsKey, 1);
            LastTutorialOneImage.OnEnd = () =>
            {
                StartButton.gameObject.SetActive(true);
                GameMainObject.Instance.RequestStartGame();
                SoundManager.Instance.PlayOneShot(OneShotSeName.Gmae_Start);
            };
        }
        else
        {
            GameMainObject.Instance.RequestStartGame();
            SoundManager.Instance.PlayOneShot(OneShotSeName.Gmae_Start);
        }
    }
    public void OnClickTutorial()
    {
        FirstRuleTutorial.gameObject.SetActive(true);
        StartButton.gameObject.SetActive(false);
        LastTutorialOneImage.OnEnd = () =>
        {
            StartButton.gameObject.SetActive(true);
        };
    }
}
