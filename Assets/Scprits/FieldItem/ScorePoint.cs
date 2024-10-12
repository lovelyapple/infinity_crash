using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePoint : FieldObjectBase
{
    [SerializeField] Transform IconTransform;
    public ScoreType ScoreType;
    private static float duration;
    private const float floatHeight = 0.2f;
}
