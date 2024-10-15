using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorAutoTurnOff : MonoBehaviour
{
    public void OnAinmationFinished()
    {
        gameObject.SetActive(false);
    }
}
