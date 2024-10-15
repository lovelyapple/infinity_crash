using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameModel
{
    private static GameModel _instance;
    public static GameModel Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new GameModel();
            }

            return _instance;
        }
    }

    private enum GamgePhase
    {
        Title,
        Game,
        Result,
    }
    private GamgePhase _currentGmaePhase = GamgePhase.Title;
    public Action OnGotoTitle;
    public Action OnStartGame;
    public Action OnFinished;
    const float DefaultGameTime = 60 * 3f;
    public float TimeLeft = DefaultGameTime;
    public void GotoTitle()
    {
        _currentGmaePhase = GamgePhase.Title;
        TimeLeft = DefaultGameTime;
        GameMainObject.Instance.DoFOn();
        if (OnGotoTitle != null)
            OnGotoTitle.Invoke();
    }
    public void StartGame()
    {
        _currentGmaePhase = GamgePhase.Game;
        TimeLeft = DefaultGameTime;
        GameMainObject.Instance.DoFOff();
        if (OnStartGame != null)
            OnStartGame.Invoke();
    }
    public void EndGame()
    {
        _currentGmaePhase = GamgePhase.Result;
        FieldObjectListController.Instance.ResetAllApplicationSpawner();
        GameMainObject.Instance.DoFOn();
        if (OnFinished != null)
            OnFinished.Invoke();
    }
    public float UpdateTimeLeft()
    {
        if(_currentGmaePhase != GamgePhase.Game)
        {
            return -1;
        }

        TimeLeft -= Time.deltaTime;

        if(TimeLeft <= 0)
        {
            EndGame();
            TimeLeft = 0;
        }

        return TimeLeft;
    }
}
