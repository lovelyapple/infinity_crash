using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class UIPauseController : MonoBehaviour
{
    public static UIPauseController Instance;
    const float mouseSenMax = 10f;
    [SerializeField] Slider MouseSlider;
    [SerializeField] GameSettings CurGameSettings;
    CursorLockMode prevState;
    public void OnEnable()
    {
        var v = MouseSensitivity.CurrentValue;
        MouseSlider.value = v / mouseSenMax;
        prevState = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
    }
    public void Close()
    {
        var v = Mathf.Max(0.1f, MouseSlider.value * mouseSenMax);
        MouseSensitivity.CurrentValue = v;
        gameObject.SetActive(false);
        GameMainObject.IsPausing = false;
        CurGameSettings.CurCharacterSettings.MouseSensitivity = MouseSensitivity.CurrentValue;
        Cursor.lockState = prevState;
    }
    public void OnQuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
