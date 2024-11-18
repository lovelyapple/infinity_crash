using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class SoundOneShotSeController : MonoBehaviour
{
    [SerializeField] AudioSource AudioSource;

    public Action<SoundOneShotSeController> OnPlayEnd = null;
    public OneShotSeName OneShotSeName;
    public void Update()
    {
        if(!AudioSource.isPlaying)
        {
            if(OnPlayEnd != null)
            {
                OnPlayEnd.Invoke(this);
            }
        }
    }
    public void Play()
    {
        gameObject.SetActive(true);
        AudioSource.Play();
    }
}
