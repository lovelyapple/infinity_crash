using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldApplicationSpawner : SpawnerBase
{
    public FieldAplicationSwitcher Switcher;
    public bool IsEmpty => Switcher.IsEmpty;
    public LineRenderer lineRenderer;
    public void CreateApplicationIcon(ApplicationType apllicationType)
    {
        Switcher.CreateIcon(apllicationType);
        OnRequestTurnOn();
        if(lineRenderer == null)
        {
            lineRenderer = Instantiate<LineRenderer>(ResourceManager.Instance.LineRenderer);
            lineRenderer.SetPositions(new Vector3[2]{
                transform.position + Vector3.up * 2,
                transform.position + Vector3.up * 50
            });
        }

        lineRenderer.gameObject.SetActive(true);
    }
    public override void OnRequestTurnOff()
    {
        var destoryType = Switcher.HoldingFieldIcon.ApplicationType;
        ApplicationPressureManager.Instance.OnApplicationDestory(destoryType);
        base.OnRequestTurnOff();
        Switcher.DestoryIcon();

        if (lineRenderer != null)
        {
            lineRenderer.gameObject.SetActive(false);
        }
    }
}
