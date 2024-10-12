using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class UIApplicationTrackingController : MonoBehaviour
{
    public ApplicationType ApplicationType;
    public RectTransform rectTransform;
    public FieldAppIcon HoldingIcon;
    public float fovInRadians;
    public RectTransform TargetRect;
    float halfWidth;
    public void SetupIcon(FieldAppIcon icon)
    {
        HoldingIcon = icon;
        gameObject.SetActive(true);

    }
    public void Start()
    {
        fovInRadians = (90 - Camera.main.fieldOfView / 2f) * Mathf.Deg2Rad;
        halfWidth = TargetRect.rect.width / 2f;
    }

    public void FixedUpdate()
    {
        if(HoldingIcon == null)
        {
            gameObject.SetActive(false);
            return;
        }

        var localPos = Camera.main.transform.InverseTransformPoint(HoldingIcon.transform.position).normalized;
        // ワールド座標をスクリーン座標に変換
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(HoldingIcon.transform.position);

        if (localPos.z < 0 || screenPosition.x < 0 || screenPosition.x > Screen.width || screenPosition.y < 0 || screenPosition.y > Screen.height)
        {
            if(localPos.x < 0)
            {
                rectTransform.anchoredPosition = new Vector2(-halfWidth, rectTransform.anchoredPosition.y);
            }
            else
            {
                rectTransform.anchoredPosition = new Vector2(halfWidth, rectTransform.anchoredPosition.y);
            }
        }
        else
        {
            Vector2 uiPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(TargetRect, screenPosition, null, out uiPosition);
            uiPosition.y = 0;
            rectTransform.anchoredPosition = uiPosition;
        }
    }
}
