using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerBase : MonoBehaviour
{
    public enum FieldSpawnType
    {
        Auto,
        Manual,
    }

    public float SpawnTime;
    public FieldSpawnType SpawnType;
    public FieldObjectBase HoldingObject;
    public GameObject EffectObject;
    

    public bool IsActive;
    public float NextSpawnTimeLeft;
    public Vector3 FloatPos;
    public void Awake()
    {
        OnRequestTurnOn();
        var ray = new Ray();
        ray.origin = transform.position;
        ray.direction = Vector3.down;
        FloatPos = EffectObject.transform.position;
        if (Physics.Raycast(ray, out var hit, 10))
        {
            transform.position = hit.point - Vector3.up * 0.01f;
        }

        EffectObject.transform.position = FloatPos;
        HoldingObject.OnTouched = OnRequestTurnOff;
    }
    public void Update()
    {
        if(IsActive)
        {
            return;
        }

        if(SpawnType == FieldSpawnType.Manual)
        {
            return;
        }

        NextSpawnTimeLeft -= Time.deltaTime;

        if(NextSpawnTimeLeft <= 0)
        {
            OnRequestTurnOn();
        }
    }
    public void OnRequestTurnOn()
    {
        HoldingObject.gameObject.SetActive(true);
        IsActive = true;
        NextSpawnTimeLeft = SpawnTime;
    }
    public void OnRequestTurnOff()
    {
        EffectObject.SetActive(true);
        HoldingObject.gameObject.SetActive(false);
        IsActive = false;
    }
}
