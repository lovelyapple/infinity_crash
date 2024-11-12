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


        // var ray = new Ray(transform.position, (TargetTransform.position - transform.position).normalized);
        // LineRendererDir.SetPositions(new Vector3[] { transform.position, TargetTransform.position });

        // if (Physics.SphereCast(ray, 0.5f, out var hitInfo, 50))
        // {
        //     if (hitInfo.point == Vector3.zero)
        //     {
        //         if (Physics.Raycast(ray, out hitInfo, 50))
        //         {
        //             LineRendererMove.SetPositions(new Vector3[] { transform.position, hitInfo.point, hitInfo.point + Vector3.up * 5f });
        //         }
        //         else
        //         {
        //             LineRendererMove.SetPositions(new Vector3[] { transform.position, transform.position + ray.direction * 5 + Vector3.up * 5 });
        //         }
        //         return;
        //     }

        //     LineRendererMove.SetPositions(new Vector3[] { transform.position, hitInfo.point, hitInfo.point + Vector3.up, hitInfo.point + Vector3.up + hitInfo.normal * 5f });
        //     TargetPostTrasform.position = hitInfo.point + hitInfo.normal * 0.5f;
        // }
        // else
        // {
        //     LineRendererMove.SetPositions(new Vector3[] { transform.position, transform.position + ray.direction * 5 + Vector3.up * 5 });
        // }

        CheckPhysic(TargetTransform.position - transform.position);
    }
    Vector3 gizmo1;
    Vector3 gizmo2;
    Vector3 gizmo3;
    public Vector3 CheckPhysic(Vector3 move)
    {
        const float HALF_SIZE = 0.5f;
        const float RAY_BACK_DISTANCE = 0.01f;
        const float FLOAT_AROUND = 0.0001f;
        const float MAX_CHECK_DISTANCE = 10f;

        var moveDir0 = move.normalized;
        var moveDistance0 = move.magnitude;
        var ray0 = new Ray(transform.position - moveDir0 * RAY_BACK_DISTANCE, moveDir0);

        if(!Physics.SphereCast(ray0, HALF_SIZE, out var hit0, MAX_CHECK_DISTANCE))
        {
            return move;
        }

        var hitDistance0 = hit0.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance0 = moveDistance0 - hitDistance0;

        var hit0MoveResult = Vector3.zero;
        var hit0TargetPos = Vector3.zero;

        if (remaingDistance0 < 0)
        {
            hit0MoveResult = moveDir0 * moveDistance0;
            hit0TargetPos = transform.position + hit0MoveResult;
            gizmo1 = hit0TargetPos;
            return hit0MoveResult;
        }

        hit0MoveResult = moveDir0 * hitDistance0;
        hit0TargetPos = transform.position + hit0MoveResult;
        gizmo1 = hit0TargetPos;

        var moveDir1 = Vector3.ProjectOnPlane(moveDir0, hit0.normal).normalized;
        var moveDistance1 = remaingDistance0;
        var ray1 = new Ray(hit0TargetPos - moveDir1 * RAY_BACK_DISTANCE, moveDir1);
        var hit1MoveResult = moveDir1 * moveDistance1;
        var hit1TargetPos = hit0TargetPos + hit1MoveResult;

        if (!Physics.SphereCast(ray1, HALF_SIZE, out var hit1, MAX_CHECK_DISTANCE))
        {
            gizmo2 = hit1TargetPos;
            return hit0MoveResult + hit1MoveResult;
        }

        var hitDistance1 = hit1.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance1 = moveDistance1 - hitDistance1;

        if (remaingDistance1 < 0)
        {
            gizmo2 = hit1TargetPos;
            return hit0MoveResult + hit1MoveResult;
        }

        hit1MoveResult = moveDir1 * hitDistance1;
        hit1TargetPos = hit0TargetPos + hit1MoveResult;
        gizmo2 = hit1TargetPos;

        var moveDir2a = Vector3.Cross(hit0.normal, hit1.normal).normalized;
        var moveDir2b = Vector3.Cross(hit0.normal, hit1.normal).normalized;

        var dot2aD = Vector3.Dot(moveDir2a, moveDir1);
        var dot2bD = Vector3.Dot(moveDir2b, moveDir1);

        var moveDir2 = Vector3.zero;
        if(dot2aD > 0)
        {
            moveDir2 = moveDir2a;
        }
        else if(dot2bD > 0)
        {
            moveDir2 = moveDir2b;
        }
        else
        {
            moveDir2 = Vector3.zero;
        }

        var ray2 = new Ray();
        return Vector3.zero;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gizmo1, 0.1f);
        Gizmos.DrawWireSphere(gizmo2, 0.1f);
    }
}
