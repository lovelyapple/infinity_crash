using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
public enum ScoreType
{
    Enginer,
    Planner,
    Artist,
    Operator,
}
public enum ApplicationType
{
    Unity = 0,
    CodeTool,
    Office,
    Blender,
    NetExplorer,
    Slack,
}
[Serializable]
public class PressureInfo
{
    public ApplicationType ApllicationType;
    public long PressureLevel;
    public float Pressure;
}
[Serializable]
public class ScorePressureSetting
{
    public ScoreType ScoreType;
    public List<PressureInfo> PressureInfos;
}
[Serializable] 
public class ScoreInfo
{
    public ScoreType ScoreType;
    public long Score;
}
public class ApplicationPressureManager : MonoSingletoneBase<ApplicationPressureManager>
{
    public List<ScorePressureSetting> PressureSettings;
    public List<PressureInfo> CurrentPressures;
    public List<ScoreInfo> ScoreInfos;

    public float PressureCrashing = 10000;
    public float PressureMax = 15000;
    public float PressureDecreaseOnDestory = 8000;
    private bool IsPlaying = true;
    public override void OnAwake()
    {
        GameModel.Instance.OnGotoTitle += OnGotoTitle;
        GameModel.Instance.OnStartGame += OnGameStart;
        GameModel.Instance.OnFinished += OnFinished;
        base.OnAwake();
    }
    public void OnDestory()
    {
        GameModel.Instance.OnGotoTitle -= OnGotoTitle;
        GameModel.Instance.OnStartGame -= OnGameStart;
        GameModel.Instance.OnFinished -= OnFinished;
    }
    public void OnGotoTitle()
    {
        IsPlaying = true;
        CurrentPressures.ForEach(x => { x.Pressure = 0; x.PressureLevel = 0; });
        ScoreInfos.Clear();
    }
    public void OnGameStart()
    {
        IsPlaying = true;
    }
    public void OnFinished()
    {
        IsPlaying = false;
    }
    public void AddPressure(ScoreType scoreType)
    {
        if(!IsPlaying)
        {
            return; 
        }
        var setting = PressureSettings.FirstOrDefault(x => x.ScoreType == scoreType);

        if(setting != null)
        {
            var pressures = setting.PressureInfos;
            foreach(var pressure in pressures)
            {
                var curPres = CurrentPressures.FirstOrDefault(x => x.ApllicationType == pressure.ApllicationType);

                if(curPres == null)
                {
                    curPres = new PressureInfo();
                    curPres.ApllicationType = pressure.ApllicationType;
                    CurrentPressures.Add(curPres);
                    var ui = GameHUDController.Instance.UIApplicationCrashingControllers.FirstOrDefault(x => x.ApplicationType == curPres.ApllicationType);
                    if(ui != null)
                    {
                        ui.Init(curPres);
                    }
                }

                curPres.PressureLevel += pressure.PressureLevel;
            }
        }

        var scoreInfo = ScoreInfos.FirstOrDefault(x => x.ScoreType == scoreType);
        
        if(scoreInfo != null)
        {
            scoreInfo.Score++;
        }
        else
        {
            ScoreInfos.Add(new ScoreInfo() { ScoreType = scoreType });
        }
    }
    public void Update()
    {
        if (!IsPlaying)
        {
            return;
        }
        foreach (var pressure in CurrentPressures)
        {
            float prePressure = pressure.Pressure;
            pressure.Pressure += pressure.PressureLevel * Time.deltaTime;
            if (pressure.Pressure > PressureMax)
            {
                pressure.Pressure = PressureMax;
                GameModel.Instance.EndGame(new GameModel.GameResult(pressure.ApllicationType));
            }

            if(prePressure < PressureCrashing && pressure.Pressure > PressureCrashing)
            {
                var spawner = FieldObjectListController.Instance.GetOneEmptyRandom();
                
                if(spawner != null)
                {
                    SpawnApplicationIcon(spawner, pressure.ApllicationType, pressure);
                }
            }
        }
    }
    public void SpawnApplicationIcon(FieldApplicationSpawner spawner,
     ApplicationType applicationType,
     PressureInfo pressureInfo)
    {
        spawner.CreateApplicationIcon(applicationType);
        var tracker = GameHUDController.Instance.UIApplicationTrackings.FirstOrDefault(x => x.ApplicationType == applicationType);
        tracker.Init(spawner, pressureInfo);

        var crasher = GameHUDController.Instance.UIApplicationCrashingControllers.FirstOrDefault(x => x.ApplicationType == applicationType);
        crasher.OnStartCrashing();
        SoundManager.Instance.PlayOneShot(OneShotSeName.App_Crashing);
    }
    public void OnApplicationDestory(ApplicationType applicationType)
    {
        var setting = CurrentPressures.FirstOrDefault(x => x.ApllicationType == applicationType);
        var tracker = GameHUDController.Instance.UIApplicationTrackings.FirstOrDefault(x => x.ApplicationType == applicationType);

        tracker.Clear();
        SoundManager.Instance.PlayOneShot(OneShotSeName.App_Recover);

        if (setting != null)
        {
            setting.Pressure = Mathf.Max(0, setting.Pressure - PressureDecreaseOnDestory);

            if(setting.Pressure < PressureCrashing)
            {
                var crasher = GameHUDController.Instance.UIApplicationCrashingControllers.FirstOrDefault(x => x.ApplicationType == applicationType);
                crasher.OnReview();
            }
        }
    }
}
