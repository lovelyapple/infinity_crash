using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum ScoreType
{
    Enginer,
    Planner,
    Artist,
    Operator,
}
public enum ApplicationType
{
    Unity,
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
    public long Pressure;
}
[Serializable]
public class ScorePressureSetting
{
    public ScoreType ScoreType;
    public List<PressureInfo> PressureInfos;
}
public class ApplicationPressureManager : MonoSingletoneBase<ApplicationPressureManager>
{
    public List<ScorePressureSetting> PressureSettings;
    public List<PressureInfo> CurrentPressures;
    public List<FieldApplicationSpawner> FieldApplicationSpawners;

    public void AddPressure(ScoreType scoreType)
    {
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
                }

                curPres.Pressure += pressure.Pressure;
            }
        }
    }
    public void SpawnApplicationIcon(ApplicationType applicationType)
    {
        if(FieldApplicationSpawners == null)
        {
            FieldApplicationSpawners = FindObjectsOfType<FieldApplicationSpawner>().ToList();
        }
    }
}
