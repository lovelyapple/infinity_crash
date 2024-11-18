using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBGMController : MonoBehaviour
{
    public AudioSource audioSource;
    public void Start()
    {
        audioSource.Play();
    }
    public bool IsPlaying;
    public void Update()
    {
        IsPlaying = audioSource.isPlaying;
    }
}
