using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDController : MonoBehaviour
{
    public GameObject SuperJumpObject;
    public TextMeshProUGUI SuperJumpLeftText;
    public void Initialize(GameCharaConttroller gameCharaConttroller)
    {
        gameCharaConttroller.OnSuperJumpUpdated = UpdateSuperJump;
    }
    public void UpdateSuperJump(GameCharaConttroller gameCharaConttroller)
    {

    }
}
