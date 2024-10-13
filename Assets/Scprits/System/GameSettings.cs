using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Ins;
    public CharacterSettings CurCharacterSettings;

    [Serializable]
    public class CharacterSettings
    {
        public float InputForce = 15f;
        public float MaxSpeedNormal = 10f;
        public float MaxSpeedBuffed = 12f;

        public float MouseSensitivity = 2f;
        public float JumpForce = 15f;
    }
}
