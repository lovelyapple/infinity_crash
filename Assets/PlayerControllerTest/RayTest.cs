using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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


        if (IsSim)
        {
            CheckGravity(TargetTransform.position - transform.position);
        }
        else
        {
            var prevState = CurrentGroundTouchState;
            DirectDownSpeed = Mathf.Min(MAX_GRAVITY, DirectDownSpeed + GRAVITY * Time.deltaTime);
            var vec = CheckGravity(Vector3.down * DirectDownSpeed * Time.deltaTime);
            
            // 字面にタッチした瞬間だけ、重力加速度を0にする（疑似反発力）
            if (prevState == GroundTouchState.Floating && prevState != CurrentGroundTouchState)
            {
                DirectDownSpeed = 0;
            }
            
            transform.position += vec;
            
        }
    }
    public enum GroundTouchState
    {
        Floating,
        Sliding,
        Stationary,
    }

    public GroundTouchState CurrentGroundTouchState;
    public float DirectDownSpeed = 0;
    public bool IsSim = false;
    public float GRAVITY = 3f;
    public float MAX_GRAVITY = 9.8f;
    public float GroundFrictionlessAngle = 30;
    public float groundDegree;
    Vector3 gizmo1;
    Vector3 gizmo2;
    Vector3 gizmo3;
    public float remaingDistance0;
    public float remaingDistance1;
    public float remaingDistance2;
    public LineRenderer normalLine0;
    public LineRenderer normalLine1;
    public LineRenderer normalLine2;
    private Vector3 CheckPhysic1(Vector3 move)
    {
        const float HALF_SIZE = 0.5f;
        const float RAY_BACK_DISTANCE = 0.01f;
        const float FLOAT_AROUND = 0.0001f;
        const float MAX_CHECK_DISTANCE = 10f;
        var curPos = transform.position;

        var moveDir0 = move.normalized;
        var moveDistance0 = move.magnitude;
        var ray0 = new Ray(curPos - moveDir0 * RAY_BACK_DISTANCE, moveDir0);

        var hits = Physics.SphereCastAll(ray0, HALF_SIZE, moveDistance0 + RAY_BACK_DISTANCE).Where(x => x.point != Vector3.zero).OrderBy(x => x.distance).ToList();

        if(hits.Count >= 3)
        {
            moveDistance0 = Mathf.Max(0, hits[0].distance - RAY_BACK_DISTANCE - FLOAT_AROUND);
            gizmo1 = curPos + moveDir0 * moveDistance0;
            return moveDir0 * moveDistance0;
        }
        else if(hits.Count == 2)
        {
            var hit20 = hits[0];
            var hit21 = hits[1];
            var hitDistance20 = Mathf.Max(0, hit20.distance - RAY_BACK_DISTANCE - FLOAT_AROUND);

            var remainingDistance21 = moveDistance0 - hitDistance20;
            var hit20MoveResult = moveDir0 * hitDistance20;
            var hit20TargetPos = curPos + hit20MoveResult;

            if (remainingDistance21 <= 0)
            {
                return moveDir0 * hitDistance20;
            }

            var moveDirCross20 = Vector3.Cross(hit20.normal, hit21.normal).normalized;
            var moveDirCross21 = Vector3.Cross(hit21.normal, hit20.normal).normalized;

            var dot20aD = Vector3.Dot(moveDirCross20, moveDir0);
            var dot20bD = Vector3.Dot(moveDirCross21, moveDir0);

            var moveDir21 = Vector3.zero;
            if (dot20aD > 0)
            {
                moveDir21 = moveDirCross20;
                SetLinePos(LineRendererDir, hit20TargetPos, hit20TargetPos + moveDir21 * 5);
            }
            else if (dot20bD > 0)
            {
                moveDir21 = moveDirCross21;
                SetLinePos(LineRendererDir, hit20TargetPos, hit20TargetPos + moveDir21 * 5);
            }
            else
            {
                SetLinePos(LineRendererDir, Vector3.zero, Vector3.up);
                moveDir21 = Vector3.zero;
            }

            var ray21 = new Ray(hit20TargetPos - moveDir21 * RAY_BACK_DISTANCE, moveDir21 * RAY_BACK_DISTANCE);
            var hit21MoveResult = hit20MoveResult + moveDir21 * remainingDistance21;
            var hit21TargetPos = curPos + hit21MoveResult;
            gizmo3 = hit20TargetPos;

            return hit21MoveResult;
        }


        if (!Physics.SphereCast(ray0, HALF_SIZE, out var hit0, MAX_CHECK_DISTANCE))
        {
            SetLinePos(normalLine0, Vector3.zero, Vector3.up);
            return move;
        }

        var hitDistance0 = hit0.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        remaingDistance0 = moveDistance0 - hitDistance0;

        gizmo1 = curPos + move;
        SetLinePos(normalLine0, hit0.point, hit0.point + hit0.normal * 3);

        // ヒット距離がmove距離より長いので、そのまま進むことができる
        if (remaingDistance0 < 0)
        {
            return move;
        }
        

        // そうでなければ、ヒットした手前まで移動する座標を作る
        var hit0MoveResult = moveDir0 * hitDistance0;
        var hit0TargetPos = curPos + hit0MoveResult;
        gizmo1 = hit0TargetPos;

        // 横滑りの情報用意
        var moveDir1 = Vector3.ProjectOnPlane(moveDir0, hit0.normal).normalized;
        var moveDistance1 = remaingDistance0;
        var ray1 = new Ray(hit0TargetPos - moveDir1 * RAY_BACK_DISTANCE, moveDir1);

        // もしヒットしなかった時のResult用意
        var hit1MoveResult = hit0MoveResult + moveDir1 * moveDistance1;
        var hit1TargetPos = curPos + hit1MoveResult;
        gizmo2 = hit1TargetPos;

        if (!Physics.SphereCast(ray1, HALF_SIZE, out var hit1, MAX_CHECK_DISTANCE))
        {
            SetLinePos(normalLine1, Vector3.zero, Vector3.up);
            return hit1MoveResult;
        }

        var hitDistance1 = hit1.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        remaingDistance1 = remaingDistance0 - hitDistance1;
        SetLinePos(normalLine1, hit1.point, hit1.point + hit1.normal * 3);

        if (remaingDistance1 < 0)
        {
            return hit1MoveResult;
        }
        

        hit1MoveResult = moveDir1 * hitDistance1;
        hit1TargetPos = curPos + hit0MoveResult + hit1MoveResult;
        gizmo2 = hit1TargetPos;

        var moveDir2a = Vector3.Cross(hit0.normal, hit1.normal).normalized;
        var moveDir2b = Vector3.Cross(hit1.normal, hit0.normal).normalized;

        var dot2aD = Vector3.Dot(moveDir2a, moveDir1);
        var dot2bD = Vector3.Dot(moveDir2b, moveDir1);

        var moveDir2 = Vector3.zero;
        if (dot2aD > 0)
        {
            moveDir2 = moveDir2a;
            SetLinePos(LineRendererDir, hit1TargetPos, hit1TargetPos + moveDir2 * 5);
        }
        else if (dot2bD > 0)
        {
            moveDir2 = moveDir2b;
            SetLinePos(LineRendererDir, hit1TargetPos, hit1TargetPos + moveDir2 * 5);
        }
        else
        {
            SetLinePos(LineRendererDir, Vector3.zero, Vector3.up);
            moveDir2 = Vector3.zero;
        }

        var ray2 = new Ray(hit1TargetPos - moveDir2 * RAY_BACK_DISTANCE, moveDir2 * RAY_BACK_DISTANCE);
        var hit2MoveResult = hit0MoveResult + hit1MoveResult + moveDir2 * remaingDistance1;
        var hit2TargetPos = curPos + hit2MoveResult;
        gizmo3 = hit2TargetPos;

        if (!Physics.SphereCast(ray2, HALF_SIZE, out var hit2, MAX_CHECK_DISTANCE))
        {
            SetLinePos(normalLine2, Vector3.zero, Vector3.up);
            return hit2MoveResult;
        }

        var hitDistance2 = hit2.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        remaingDistance2 = remaingDistance1 - hitDistance2;
        SetLinePos(normalLine2, hit2.point, hit2.point + hit2.normal * 3);

        if (remaingDistance2 < 0)
        {
            return hit2MoveResult;
        }

        //3点タッチしてあれば、もう計算しない

        hit2MoveResult = moveDir2 * hitDistance2;
        hit2TargetPos = curPos + hit1MoveResult + hit2MoveResult;

        gizmo3 = hit2TargetPos;
        return hit1MoveResult + hit2MoveResult;
    }
    private Vector3 CheckGravity(Vector3 move)
    {
        const float HALF_SIZE = 0.5f;
        const float RAY_BACK_DISTANCE = 0.01f;
        const float FLOAT_AROUND = 0.0001f;
        const float MAX_CHECK_DISTANCE = 10f;
        var curPos = transform.position;

        var moveDir0 = move.normalized;
        var moveDistance0 = move.magnitude;
        var ray0 = new Ray(curPos - moveDir0 * RAY_BACK_DISTANCE, moveDir0);

        var hits = Physics.SphereCastAll(ray0, HALF_SIZE, moveDistance0 + RAY_BACK_DISTANCE).Where(x => x.point != Vector3.zero).OrderBy(x => x.distance).ToList();

        if(hits.Count >= 3)
        {
            moveDistance0 = Mathf.Max(0, hits[0].distance - RAY_BACK_DISTANCE - FLOAT_AROUND);
            gizmo1 = curPos + moveDir0 * moveDistance0;
            CurrentGroundTouchState = GroundTouchState.Stationary;
            return moveDir0 * moveDistance0;
        }
        else if(hits.Count == 2)
        {
            var hit20 = hits[0];
            var hit21 = hits[1];
            var hitDistance20 = Mathf.Max(0, hit20.distance - RAY_BACK_DISTANCE - FLOAT_AROUND);

            var remainingDistance21 = moveDistance0 - hitDistance20;
            var hit20MoveResult = moveDir0 * hitDistance20;
            var hit20TargetPos = curPos + hit20MoveResult;

            if (remainingDistance21 <= 0)
            {
                CurrentGroundTouchState = GroundTouchState.Sliding;
                return moveDir0 * hitDistance20;
            }

            var moveDirCross20 = Vector3.Cross(hit20.normal, hit21.normal).normalized;
            var moveDirCross21 = Vector3.Cross(hit21.normal, hit20.normal).normalized;

            var dot20aD = Vector3.Dot(moveDirCross20, moveDir0);
            var dot20bD = Vector3.Dot(moveDirCross21, moveDir0);

            var moveDir21 = Vector3.zero;
            if (dot20aD > 0)
            {
                moveDir21 = moveDirCross20;
                SetLinePos(LineRendererDir, hit20TargetPos, hit20TargetPos + moveDir21 * 5);
            }
            else if (dot20bD > 0)
            {
                moveDir21 = moveDirCross21;
                SetLinePos(LineRendererDir, hit20TargetPos, hit20TargetPos + moveDir21 * 5);
            }
            else
            {
                SetLinePos(LineRendererDir, Vector3.zero, Vector3.up);
                moveDir21 = Vector3.zero;
            }
            
            groundDegree =  90 - Vector3.Angle(Vector3.down, moveDir21);
            if (groundDegree < GroundFrictionlessAngle  || groundDegree == 90f)
            {
                CurrentGroundTouchState = GroundTouchState.Stationary;
                return Vector3.zero;
            }

            var ray21 = new Ray(hit20TargetPos - moveDir21 * RAY_BACK_DISTANCE, moveDir21 * RAY_BACK_DISTANCE);
            var hit21MoveResult = hit20MoveResult + moveDir21 * remainingDistance21;
            var hit21TargetPos = curPos + hit21MoveResult;
            gizmo3 = hit20TargetPos;

            CurrentGroundTouchState = GroundTouchState.Sliding;
            return hit21MoveResult;
        }


        if (!Physics.SphereCast(ray0, HALF_SIZE, out var hit0, MAX_CHECK_DISTANCE))
        {
            SetLinePos(normalLine0, Vector3.zero, Vector3.up);
            CurrentGroundTouchState = GroundTouchState.Floating;
            return move;
        }

        var hitDistance0 = hit0.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        remaingDistance0 = moveDistance0 - hitDistance0;

        gizmo1 = curPos + move;
        SetLinePos(normalLine0, hit0.point, hit0.point + hit0.normal * 3);

        // ヒット距離がmove距離より長いので、そのまま進むことができる
        if (remaingDistance0 < 0)
        {
            CurrentGroundTouchState = GroundTouchState.Floating;
            return move;
        }
        

        // そうでなければ、ヒットした手前まで移動する座標を作る
        var hit0MoveResult = moveDir0 * hitDistance0;
        var hit0TargetPos = curPos + hit0MoveResult;
        gizmo1 = hit0TargetPos;

        // 横滑りの情報用意
        var moveDir1 = Vector3.ProjectOnPlane(moveDir0, hit0.normal).normalized;

         groundDegree =  90 - Vector3.Angle(Vector3.down, moveDir1);

        if (groundDegree < GroundFrictionlessAngle || groundDegree == 90f)
        {
            CurrentGroundTouchState = GroundTouchState.Stationary;
            return hit0MoveResult;
        }
        
        var moveDistance1 = remaingDistance0;
        var ray1 = new Ray(hit0TargetPos - moveDir1 * RAY_BACK_DISTANCE, moveDir1);

        // もしヒットしなかった時のResult用意
        var hit1MoveResult = hit0MoveResult + moveDir1 * moveDistance1;
        var hit1MoveResultPrev = hit1MoveResult;
        var hit1TargetPos = curPos + hit1MoveResult;
        gizmo2 = hit1TargetPos;

        if (!Physics.SphereCast(ray1, HALF_SIZE, out var hit1, MAX_CHECK_DISTANCE))
        {
            SetLinePos(normalLine1, Vector3.zero, Vector3.up);
            CurrentGroundTouchState = GroundTouchState.Sliding;
            return hit1MoveResult;
        }

        var hitDistance1 = hit1.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        remaingDistance1 = remaingDistance0 - hitDistance1;
        SetLinePos(normalLine1, hit1.point, hit1.point + hit1.normal * 3);

        if (remaingDistance1 < 0)
        {
            CurrentGroundTouchState = GroundTouchState.Sliding;
            return hit1MoveResult;
        }
        

        hit1MoveResult = moveDir1 * hitDistance1;
        hit1TargetPos = curPos + hit0MoveResult + hit1MoveResult;
        gizmo2 = hit1TargetPos;

        var moveDir2a = Vector3.Cross(hit0.normal, hit1.normal).normalized;
        var moveDir2b = Vector3.Cross(hit1.normal, hit0.normal).normalized;

        var dot2aD = Vector3.Dot(moveDir2a, moveDir1);
        var dot2bD = Vector3.Dot(moveDir2b, moveDir1);

        var moveDir2 = Vector3.zero;
        if (dot2aD > 0)
        {
            moveDir2 = moveDir2a;
            SetLinePos(LineRendererDir, hit1TargetPos, hit1TargetPos + moveDir2 * 5);
        }
        else if (dot2bD > 0)
        {
            moveDir2 = moveDir2b;
            SetLinePos(LineRendererDir, hit1TargetPos, hit1TargetPos + moveDir2 * 5);
        }
        else
        {
            SetLinePos(LineRendererDir, Vector3.zero, Vector3.up);
            moveDir2 = Vector3.zero;
        }

        groundDegree = 90 - Vector3.Angle(Vector3.down, moveDir2);
        if (groundDegree < GroundFrictionlessAngle || groundDegree == 90f)
        {
            CurrentGroundTouchState = GroundTouchState.Stationary;
            return hit0MoveResult + hit1MoveResult;
        }

        var ray2 = new Ray(hit1TargetPos - moveDir2 * RAY_BACK_DISTANCE, moveDir2 * RAY_BACK_DISTANCE);
        var hit2MoveResult = hit0MoveResult + hit1MoveResult + moveDir2 * remaingDistance1;
        var hit2TargetPos = curPos + hit2MoveResult;
        gizmo3 = hit2TargetPos;

        if (!Physics.SphereCast(ray2, HALF_SIZE, out var hit2, MAX_CHECK_DISTANCE))
        {
            SetLinePos(normalLine2, Vector3.zero, Vector3.up);
            CurrentGroundTouchState = GroundTouchState.Sliding;
            return hit2MoveResult;
        }

        var hitDistance2 = hit2.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        remaingDistance2 = remaingDistance1 - hitDistance2;
        SetLinePos(normalLine2, hit2.point, hit2.point + hit2.normal * 3);

        if (remaingDistance2 < 0)
        {
            CurrentGroundTouchState = GroundTouchState.Sliding;
            return hit2MoveResult;
        }

        //3点タッチしてあれば、もう計算しない

        hit2MoveResult = moveDir2 * hitDistance2;
        hit2TargetPos = curPos + hit1MoveResult + hit2MoveResult;

        gizmo3 = hit2TargetPos;
        CurrentGroundTouchState = GroundTouchState.Stationary;
        return hit1MoveResult + hit2MoveResult;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gizmo1, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gizmo2, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(gizmo3, 0.1f);
    }
    void SetLinePos(LineRenderer lineRenderer, Vector3 s, Vector3 e)
    {
        lineRenderer.SetPositions(new Vector3[] { s, e });
    }
}
