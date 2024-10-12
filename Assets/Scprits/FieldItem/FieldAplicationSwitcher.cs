using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldAplicationSwitcher : FieldObjectBase
{
    public FieldAppIcon HoldingFieldIcon;
    public override void RequestTouch()
    {
        base.RequestTouch();
        DestoryIcon();
    }
    public void DestoryIcon()
    {
        if(HoldingFieldIcon != null)
        Destroy(HoldingFieldIcon.gameObject);
    }
    public void CreateIcon(ApplicationType apllicationType)
    {
        if(HoldingFieldIcon != null)
        {
            DestoryIcon();
        }

        HoldingFieldIcon = ResourceManager.Instance.CreateFieldIcon(apllicationType);
        HoldingFieldIcon.transform.SetParent(transform);
        HoldingFieldIcon.transform.localPosition = Vector3.zero;
    }
}
