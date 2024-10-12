using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIApplicationCrashingController : MonoBehaviour
{
    public ApplicationType ApplicationType;
    public Image FillIamge;
    public GameObject CrashedObjRoot;
    public void ActiveCrashing()
    {
        gameObject.SetActive(true);
    }
    
}
