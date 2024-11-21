using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIResultCrashedApp : MonoBehaviour
{
    [SerializeField] Sprite[] appSprs;
    [SerializeField] Image appImage;
    public void Setup(ApplicationType applicationType)
    {
        gameObject.SetActive(true);
        appImage.sprite = appSprs[(int)applicationType];
    }
}
