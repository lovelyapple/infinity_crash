using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemAutoDestory : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (!_particleSystem.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
