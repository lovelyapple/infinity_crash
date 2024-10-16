using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldObjectBase : MonoBehaviour
{
    public virtual void RequestTouch()
    {
        if(OnTouched != null)
        {
            OnTouched.Invoke();
        }
    }
    public Action OnTouched;
}
