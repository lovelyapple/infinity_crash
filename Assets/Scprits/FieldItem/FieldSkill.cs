using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldSkill : FieldObjectBase
{
    public GameObject EffectObject;
    public Vector3 rotationSpeed = new Vector3(0, 100, 0);
    public void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);  
    }
}
