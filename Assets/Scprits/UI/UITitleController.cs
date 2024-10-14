using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITitleController : MonoBehaviour
{
    private void Awake()
    {
        GameModel.Instance.OnStartGame += OnGameStart;
        GameModel.Instance.OnFinished += OnGameFinished;
        GameModel.Instance.OnGotoTitle += OnGotoTitle;
    }
    private void OnDestory()
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
        GameMainObject.Instance.RequestStartGame();
    }
}
