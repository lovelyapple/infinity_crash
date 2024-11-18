using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOneShotOnAwakeController : MonoBehaviour
{
    public OneShotSeName OneShotSeName;
    public void OnEnable()
    {
        SoundManager.Instance.PlayOneShot(OneShotSeName);
    }
}
