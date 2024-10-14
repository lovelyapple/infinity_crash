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
    public void GotoTitle()
    {
        _currentGmaePhase = GamgePhase.Title;
        if(OnGotoTitle != null)
        OnGotoTitle.Invoke();
    }
    public void StartGame()
    {
        _currentGmaePhase = GamgePhase.Game;
        if (OnStartGame != null)
            OnStartGame.Invoke();
    }
    public void EndGame()
    {
        _currentGmaePhase = GamgePhase.Result;
        if (OnFinished != null)
            OnFinished.Invoke();
    }

}
