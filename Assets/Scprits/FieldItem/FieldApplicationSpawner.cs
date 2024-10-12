using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldApplicationSpawner : SpawnerBase
{
    public FieldAplicationSwitcher Switcher;
    public override void OnRequestTurnOn()
    {
        base.OnRequestTurnOn();
    }
    public override void OnRequestTurnOff()
    {
        base.OnRequestTurnOff();
        Switcher.DestoryIcon();
    }
}
