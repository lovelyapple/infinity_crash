using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTest : MonoBehaviour
{
    [SerializeField] Transform TargetTransform;
    [SerializeField] Transform TargetPostTrasform;
    public LineRenderer LineRendererMove;
    public LineRenderer LineRendererDir;

    public void Update()
    {
        if(TargetTransform == null)
        {
            return;
        }


        var ray = new Ray(transform.position, (TargetTransform.position - transform.position).normalized);
        LineRendererDir.SetPositions(new Vector3[] { transform.position, TargetTransform.position });

        if (Physics.SphereCast(ray, 0.5f, out var hitInfo, 50))
        {
            if (hitInfo.point == Vector3.zero)
            {
                if (Physics.Raycast(ray, out hitInfo, 50))
                {
                    LineRendererMove.SetPositions(new Vector3[] { transform.position, hitInfo.point, hitInfo.point + Vector3.up * 5f });
                }
                else
                {
                    LineRendererMove.SetPositions(new Vector3[] { transform.position, transform.position + ray.direction * 5 + Vector3.up * 5 });
                }
                return;
            }

            LineRendererMove.SetPositions(new Vector3[] { transform.position, hitInfo.point, hitInfo.point + Vector3.up, hitInfo.point + Vector3.up + hitInfo.normal * 5f });
            TargetPostTrasform.position = hitInfo.point + hitInfo.normal * 0.5f;
        }
        else
        {
            LineRendererMove.SetPositions(new Vector3[] { transform.position, transform.position + ray.direction * 5 + Vector3.up * 5 });
        }
    }
}
