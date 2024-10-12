using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FieldObjectEditor : EditorWindow
{
    private static FieldObjectEditor _intance;
    public float ScoreFloatHeigth = 0.2f;
    private Dictionary<ScoreType, int> ScoreCount = new Dictionary<ScoreType, int>();
    [MenuItem("GameDebug/FieldObjectEditor")]
    public static void Open()
    {
        if(_intance == null)
        {
            _intance = GetWindow<FieldObjectEditor>();
            _intance.Initialize();
        }
    }
    public void Initialize()
    {
        foreach(ScoreType type in Enum.GetValues(typeof(ScoreType)))
        {
            ScoreCount.Add(type, 0);
        }
    }
    public void OnGUI()
    {
        ScoreFloatHeigth = EditorGUILayout.FloatField(ScoreFloatHeigth);
        if (GUILayout.Button("Reset Score Pos"))
        {
            ResetScorePointPos();
        }

        foreach(var k in ScoreCount.Keys)
        {
            GUILayout.Label($"{k} : {ScoreCount[k]}");
        }
    }
    private void ResetScorePointPos()
    {
        var spawners = FindObjectsOfType<ScorePoint>().Select(x => x.transform.parent);

        foreach(var spawner in spawners)
        {
            if(Physics.Raycast(spawner.position, Vector3.down, out var hit, 10f))
            {
                spawner.transform.position = hit.point + Vector3.up * ScoreFloatHeigth;
            }
        }
    }
    private float _updateTimeLeft;
    public void Update()
    {
        if (_updateTimeLeft > 0)
        {
            _updateTimeLeft -= Time.deltaTime;
            return;
        }
        foreach (ScoreType k in Enum.GetValues(typeof(ScoreType)))
        {
            ScoreCount[k] = 0;
        }
        _updateTimeLeft += 1f;
        var spawners = FindObjectsOfType<ScorePoint>().ToList();

        foreach (var sc in spawners)
        {
            ScoreCount[sc.ScoreType]++;
        }
        Repaint();
    }
}
