using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class UITutorialOneImage : MonoBehaviour
{
    public UITutorialOneImage NextTutorialImage;
    public Action OnEnd;
    public void OnClickNext()
    {
        this.gameObject.SetActive(false);
        
        if (NextTutorialImage != null)
        {
            NextTutorialImage.gameObject.SetActive(true);
            return;
        }

        if (OnEnd != null)
        {
            OnEnd.Invoke();
            OnEnd = null;
        }
    }
}
