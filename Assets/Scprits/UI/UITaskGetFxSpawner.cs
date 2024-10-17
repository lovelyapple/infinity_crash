using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITaskGetFxSpawner : MonoBehaviour
{
    public static UITaskGetFxSpawner Instance;
    [SerializeField] private Transform TaskInstanceRootTran;
    [SerializeField] private ParticleSystemAutoDestory TaskCodeDestory;
    [SerializeField] private ParticleSystemAutoDestory TaskArtDestory;
    [SerializeField] private ParticleSystemAutoDestory TaskPlanerDestory;
    [SerializeField] private ParticleSystemAutoDestory TaskOprDestory;

    private Dictionary<ScoreType, ParticleSystemAutoDestory> PrefabDict;

    private void Awake()
    {
        Instance = this;
        PrefabDict = new Dictionary<ScoreType, ParticleSystemAutoDestory>()
        {
            { ScoreType.Enginer, TaskCodeDestory },
            { ScoreType.Artist, TaskArtDestory },
            { ScoreType.Planner, TaskPlanerDestory },
            { ScoreType.Operator, TaskOprDestory },
        };
    }

    public static void CreateOne(ScoreType scoreType)
    {
        var prefab = Instance.PrefabDict[scoreType];
        var instance = Instantiate(prefab);
        instance.transform.SetParent(Instance.TaskInstanceRootTran);
        instance.transform.localPosition = Vector3.zero;
    }
}
