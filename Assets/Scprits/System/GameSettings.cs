using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class MouseSensitivity
{
    const string MoveSensitivityKey = "MouseSensitivity";
    private static float? _currentValue = null;
    public static float CurrentValue
    {
        get
        {
            if (!_currentValue.HasValue)
            {
                _currentValue = PlayerPrefs.GetFloat(MoveSensitivityKey, 2f);
            }

            return _currentValue.Value;
        }
        set
        {
            if (!_currentValue.HasValue)
            {
                _currentValue = PlayerPrefs.GetFloat(MoveSensitivityKey, 2f);
            }

            if (_currentValue != value)
            {
                _currentValue = value;
                PlayerPrefs.SetFloat(MoveSensitivityKey, _currentValue.Value);
            }

        }
    }
}
public class GameSettings : MonoBehaviour
{
    public static GameSettings Ins;
    public CharacterSettings CurCharacterSettings;

    [Serializable]
    public class CharacterSettings
    {
        public float InputForceVelocity = 1f;
        public float MaxSpeedNormal = 10f;
        public float MaxSpeedBuffed = 12f;

        public float MouseSensitivity = 2f;
        public float JumpForce = 15f;
        public float FloatingJumpTimeLimie = 0.3f;

        public float GravitySpeed = 9.8f;
        public float MaxGravitySpeed = 10f;

        public float FallFootSlideDegree = 30f;
    }
}
