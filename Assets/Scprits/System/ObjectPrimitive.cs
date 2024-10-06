using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPrimitive : MonoBehaviour
{
    private static Transform CameraTransform;
    private void Awake()
    {
        if (CameraTransform == null)
        {
            CameraTransform = Camera.main.transform;
        }
    }
    public void Update()
    {
        transform.LookAt(CameraTransform.position);
    }
}
