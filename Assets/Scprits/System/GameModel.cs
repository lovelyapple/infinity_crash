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
    public enum GameResultType
    {
        Timeout,
        AppCrashed,
    }
    public class GameResult
    {
        public readonly GameResultType ResultType;
        public readonly ApplicationType CrashedAppType;
        public GameResult()
        {
            ResultType = GameResultType.Timeout;
        }
        public GameResult(ApplicationType applicationType)
        {
            ResultType = GameResultType.AppCrashed;
            CrashedAppType = applicationType;
        }
    }
    private GamgePhase _currentGmaePhase = GamgePhase.Title;
    public Action OnGotoTitle;
    public Action OnStartGame;
    public Action OnFinished;
    const float DefaultGameTime = 60 * 3f;
    public float TimeLeft = DefaultGameTime;
    public int TimeAddedCount = 0;
    public GameResult CurrentGameResult;
    public void GotoTitle()
    {
        _currentGmaePhase = GamgePhase.Title;
        TimeLeft = DefaultGameTime;
        TimeAddedCount = 0;
        CurrentGameResult = null;
        GameMainObject.Instance.DoFOn();
        if (OnGotoTitle != null)
            OnGotoTitle.Invoke();
    }
    public void StartGame()
    {
        _currentGmaePhase = GamgePhase.Game;
        TimeLeft = DefaultGameTime;
        TimeAddedCount = 0;
        CurrentGameResult = null;
        GameMainObject.Instance.DoFOff();
        if (OnStartGame != null)
            OnStartGame.Invoke();
    }
    public void EndGame(GameResult result)
    {
        CurrentGameResult = result;
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
            EndGame(new GameResult());
            TimeLeft = 0;
        }

        return TimeLeft;
    }
    public void AddTime()
    {
        if(_currentGmaePhase == GamgePhase.Game)
        {
            TimeLeft += SkillTimeAdd.TimeAdd;
            TimeAddedCount++;
        }
    }
    public void DecreaseTime()
    {
        TimeLeft -= 2f;
    }
}
