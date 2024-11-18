using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GroundTouchState
{
    Floating,
    Sliding,
    Stationary,
}
public static class CollisionUtility
{
    public static Vector3 MoveCheck(Vector3 move, Vector3 curPos, float half_size)
    {
        const float RAY_BACK_DISTANCE = 0.01f;
        const float FLOAT_AROUND = 0.0001f;
        const float MAX_CHECK_DISTANCE = 10f;

        var moveDir0 = move.normalized;
        var moveDistance0 = move.magnitude;
        var ray0 = new Ray(curPos - moveDir0 * RAY_BACK_DISTANCE, moveDir0);

        var hits = Physics.SphereCastAll(ray0, half_size, moveDistance0 + RAY_BACK_DISTANCE).Where(x => x.point != Vector3.zero).OrderBy(x => x.distance).ToList();

        if (hits.Count >= 3)
        {
            moveDistance0 = Mathf.Max(0, hits[0].distance - RAY_BACK_DISTANCE - FLOAT_AROUND);
            // gizmo1 = curPos + moveDir0 * moveDistance0;
            return moveDir0 * moveDistance0;
        }
        else if (hits.Count == 2)
        {
            // var hit20 = hits[0];
            // var hit21 = hits[1];
            // var hitDistance20 = Mathf.Max(0, hit20.distance - RAY_BACK_DISTANCE - FLOAT_AROUND);

            // var remainingDistance21 = moveDistance0 - hitDistance20;
            // var hit20MoveResult = moveDir0 * hitDistance20;
            // var hit20TargetPos = curPos + hit20MoveResult;

            // if (remainingDistance21 <= 0)
            // {
            //     return moveDir0 * hitDistance20;
            // }

            // var moveDirCross20 = Vector3.Cross(hit20.normal, hit21.normal).normalized;
            // var moveDirCross21 = Vector3.Cross(hit21.normal, hit20.normal).normalized;

            // var dot20aD = Vector3.Dot(moveDirCross20, moveDir0);
            // var dot20bD = Vector3.Dot(moveDirCross21, moveDir0);

            // var moveDir21 = Vector3.zero;
            // if (dot20aD > 0)
            // {
            //     moveDir21 = moveDirCross20;
            //     // SetLinePos(LineRendererDir, hit20TargetPos, hit20TargetPos + moveDir21 * 5);
            // }
            // else if (dot20bD > 0)
            // {
            //     moveDir21 = moveDirCross21;
            //     // SetLinePos(LineRendererDir, hit20TargetPos, hit20TargetPos + moveDir21 * 5);
            // }
            // else
            // {
            //     // SetLinePos(LineRendererDir, Vector3.zero, Vector3.up);
            //     moveDir21 = Vector3.zero;
            // }

            // var ray21 = new Ray(hit20TargetPos - moveDir21 * RAY_BACK_DISTANCE, moveDir21 * RAY_BACK_DISTANCE);
            // var hit21MoveResult = hit20MoveResult + moveDir21 * remainingDistance21;
            // var hit21TargetPos = curPos + hit21MoveResult;
            // // gizmo3 = hit20TargetPos;

            // if (!Physics.SphereCast(ray21, half_size, out var hit22, MAX_CHECK_DISTANCE))
            // {
            //     // SetLinePos(normalLine2, Vector3.zero, Vector3.up);
            //     return hit21MoveResult;
            // }

            // var hitDistance22 = hit22.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
            // var remainingDistance22 = remainingDistance21 - hitDistance22;
            // // SetLinePos(normalLine2, hit22.point, hit22.point + hit22.normal * 3);

            // if (remainingDistance22 < 0)
            // {
            //     return hit21MoveResult;
            // }

            // hit21MoveResult = moveDir21 * hitDistance22;
            // hit21TargetPos = curPos + hit20MoveResult + hit21MoveResult;

            // // gizmo3 = hit21TargetPos;
            // return hit20MoveResult + hit21MoveResult;

        }


        if (!Physics.SphereCast(ray0, half_size, out var hit0, MAX_CHECK_DISTANCE))
        {
            // SetLinePos(normalLine0, Vector3.zero, Vector3.up);
            return move;
        }

        var hitDistance0 = hit0.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance0 = moveDistance0 - hitDistance0;

        // gizmo1 = curPos + move;
        // SetLinePos(normalLine0, hit0.point, hit0.point + hit0.normal * 3);

        // ヒット距離がmove距離より長いので、そのまま進むことができる
        if (remaingDistance0 < 0)
        {
            return move;
        }


        // そうでなければ、ヒットした手前まで移動する座標を作る
        var hit0MoveResult = moveDir0 * hitDistance0;
        var hit0TargetPos = curPos + hit0MoveResult;
        // gizmo1 = hit0TargetPos;

        // 横滑りの情報用意
        var moveDir1 = Vector3.ProjectOnPlane(moveDir0, hit0.normal).normalized;
        // Debug.LogWarning($"{moveDir1}");
        var moveDistance1 = remaingDistance0;
        var ray1 = new Ray(hit0TargetPos - moveDir1 * RAY_BACK_DISTANCE, moveDir1);

        // もしヒットしなかった時のResult用意
        var hit1MoveResult = hit0MoveResult + moveDir1 * moveDistance1;
        var hit1TargetPos = curPos + hit1MoveResult;
        // gizmo2 = hit1TargetPos;

        if (!Physics.SphereCast(ray1, half_size, out var hit1, MAX_CHECK_DISTANCE))
        {
            //  SetLinePos(normalLine1, Vector3.zero, Vector3.up);
            return hit1MoveResult;
        }

        var hitDistance1 = hit1.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance1 = remaingDistance0 - hitDistance1;
        // SetLinePos(normalLine1, hit1.point, hit1.point + hit1.normal * 3);

        if (remaingDistance1 < 0)
        {
            return hit1MoveResult;
        }


        hit1MoveResult = moveDir1 * hitDistance1;
        hit1TargetPos = curPos + hit0MoveResult + hit1MoveResult;
        // gizmo2 = hit1TargetPos;

        var moveDir2a = Vector3.Cross(hit0.normal, hit1.normal).normalized;
        var moveDir2b = Vector3.Cross(hit1.normal, hit0.normal).normalized;

        var dot2aD = Vector3.Dot(moveDir2a, moveDir1);
        var dot2bD = Vector3.Dot(moveDir2b, moveDir1);

        var moveDir2 = Vector3.zero;
        if (dot2aD > 0)
        {
            moveDir2 = moveDir2a;
            // SetLinePos(LineRendererDir, hit1TargetPos, hit1TargetPos + moveDir2 * 5);
        }
        else if (dot2bD > 0)
        {
            moveDir2 = moveDir2b;
            // SetLinePos(LineRendererDir, hit1TargetPos, hit1TargetPos + moveDir2 * 5);
        }
        else
        {
            // SetLinePos(LineRendererDir, Vector3.zero, Vector3.up);
            moveDir2 = Vector3.zero;
        }

        var ray2 = new Ray(hit1TargetPos - moveDir2 * RAY_BACK_DISTANCE, moveDir2 * RAY_BACK_DISTANCE);
        var hit2MoveResult = hit0MoveResult + hit1MoveResult + moveDir2 * remaingDistance1;
        var hit2TargetPos = curPos + hit2MoveResult;
        // gizmo3 = hit2TargetPos;

        if (!Physics.SphereCast(ray2, half_size, out var hit2, MAX_CHECK_DISTANCE))
        {
            // SetLinePos(normalLine2, Vector3.zero, Vector3.up);
            return hit2MoveResult;
        }

        var hitDistance2 = hit2.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance2 = remaingDistance1 - hitDistance2;
        // SetLinePos(normalLine2, hit2.point, hit2.point + hit2.normal * 3);

        if (remaingDistance2 < 0)
        {
            return hit2MoveResult;
        }

        //3点タッチしてあれば、もう計算しない

        hit2MoveResult = moveDir2 * hitDistance2;
        hit2TargetPos = curPos + hit1MoveResult + hit2MoveResult;

        // gizmo3 = hit2TargetPos;
        return hit1MoveResult + hit2MoveResult;
    }
    public static Vector3 FallCheck(Vector3 move, Vector3 curPos, float half_size, float foot_silde_degree, out float curFootDegree, out GroundTouchState groundTouchState, out string groundTag) 
    {
        curFootDegree = 0;
        groundTag = "";
        const float RAY_BACK_DISTANCE = 0.01f;
        const float FLOAT_AROUND = 0.0001f;
        const float MAX_CHECK_DISTANCE = 10f;

        var moveDir0 = move.normalized;
        var moveDistance0 = move.magnitude;
        var ray0 = new Ray(curPos - moveDir0 * RAY_BACK_DISTANCE, moveDir0);

        var hits = Physics.SphereCastAll(ray0, half_size, moveDistance0 + RAY_BACK_DISTANCE).Where(x => x.point != Vector3.zero).OrderBy(x => x.distance).ToList();

        if (hits.Count >= 3)
        {
            moveDistance0 = Mathf.Max(0, hits[0].distance - RAY_BACK_DISTANCE - FLOAT_AROUND);
            // gizmo1 = curPos + moveDir0 * moveDistance0;
            groundTouchState = GroundTouchState.Stationary;
            groundTag = hits[0].transform.gameObject.tag;
            return moveDir0 * moveDistance0;
        }
        else if (hits.Count == 2)
        {
            // var hit20 = hits[0];
            // var hit21 = hits[1];
            // var hitDistance20 = Mathf.Max(0, hit20.distance - RAY_BACK_DISTANCE - FLOAT_AROUND);

            // var remainingDistance21 = moveDistance0 - hitDistance20;
            // var hit20MoveResult = moveDir0 * hitDistance20;
            // var hit20TargetPos = curPos + hit20MoveResult;

            // if (remainingDistance21 <= 0)
            // {
            //     CurrentGroundTouchState = GroundTouchState.Sliding;
            //     return moveDir0 * hitDistance20;
            // }

            // var moveDirCross20 = Vector3.Cross(hit20.normal, hit21.normal).normalized;
            // var moveDirCross21 = Vector3.Cross(hit21.normal, hit20.normal).normalized;

            // var dot20aD = Vector3.Dot(moveDirCross20, moveDir0);
            // var dot20bD = Vector3.Dot(moveDirCross21, moveDir0);

            // var moveDir21 = Vector3.zero;
            // if (dot20aD > 0)
            // {
            //     moveDir21 = moveDirCross20;
            //     // SetLinePos(LineRendererDir, hit20TargetPos, hit20TargetPos + moveDir21 * 5);
            // }
            // else if (dot20bD > 0)
            // {
            //     moveDir21 = moveDirCross21;
            //     // SetLinePos(LineRendererDir, hit20TargetPos, hit20TargetPos + moveDir21 * 5);
            // }
            // else
            // {
            //     // SetLinePos(LineRendererDir, Vector3.zero, Vector3.up);
            //     moveDir21 = Vector3.zero;
            // }

            // FootGroundAngle = 90 - Vector3.Angle(Vector3.down, moveDir21);
            // if (FootGroundAngle < GROUND_SLIDE_DEGREE || FootGroundAngle == 90f)
            // {
            //     CurrentGroundTouchState = GroundTouchState.Stationary;
            //     return Vector3.zero;
            // }

            // var ray21 = new Ray(hit20TargetPos - moveDir21 * RAY_BACK_DISTANCE, moveDir21 * RAY_BACK_DISTANCE);
            // var hit21MoveResult = hit20MoveResult + moveDir21 * remainingDistance21;
            // var hit21TargetPos = curPos + hit21MoveResult;
            // // gizmo3 = hit20TargetPos;

            // if (!Physics.SphereCast(ray21, HALF_SIZE, out var hit22, MAX_CHECK_DISTANCE))
            // {
            //     // SetLinePos(normalLine2, Vector3.zero, Vector3.up);
            //     CurrentGroundTouchState = GroundTouchState.Sliding;
            //     return hit21MoveResult;
            // }

            // var hitDistance22 = hit22.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
            // var remainingDistance22 = remainingDistance21 - hitDistance22;
            // // SetLinePos(normalLine2, hit22.point, hit22.point + hit22.normal * 3);

            // if (remainingDistance22 < 0)
            // {
            //     CurrentGroundTouchState = GroundTouchState.Sliding;
            //     return hit21MoveResult;
            // }

            // hit21MoveResult = moveDir21 * hitDistance22;
            // hit21TargetPos = curPos + hit20MoveResult + hit21MoveResult;

            // // gizmo3 = hit21TargetPos;
            // CurrentGroundTouchState = GroundTouchState.Stationary;
            // return hit20MoveResult + hit21MoveResult;
        }


        if (!Physics.SphereCast(ray0, half_size, out var hit0, MAX_CHECK_DISTANCE))
        {
            // SetLinePos(normalLine0, Vector3.zero, Vector3.up);
            groundTouchState = GroundTouchState.Floating;
            return move;
        }

        groundTag = hit0.transform.gameObject.tag;
        var hitDistance0 = hit0.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance0 = moveDistance0 - hitDistance0;

        // gizmo1 = curPos + move;
        // SetLinePos(normalLine0, hit0.point, hit0.point + hit0.normal * 3);

        // ヒット距離がmove距離より長いので、そのまま進むことができる
        if (remaingDistance0 < 0)
        {
            groundTouchState = GroundTouchState.Floating;
            return move;
        }


        // そうでなければ、ヒットした手前まで移動する座標を作る
        var hit0MoveResult = moveDir0 * hitDistance0;
        var hit0TargetPos = curPos + hit0MoveResult;
        // gizmo1 = hit0TargetPos;

        // 横滑りの情報用意
        var moveDir1 = Vector3.ProjectOnPlane(moveDir0, hit0.normal).normalized;

        curFootDegree = 90 - Vector3.Angle(Vector3.down, moveDir1);

        if (curFootDegree < foot_silde_degree || curFootDegree == 90f)
        {
            groundTouchState = GroundTouchState.Stationary;
            return hit0MoveResult;
        }

        var moveDistance1 = remaingDistance0;
        var ray1 = new Ray(hit0TargetPos - moveDir1 * RAY_BACK_DISTANCE, moveDir1);

        // もしヒットしなかった時のResult用意
        var hit1MoveResult = hit0MoveResult + moveDir1 * moveDistance1;
        var hit1MoveResultPrev = hit1MoveResult;
        var hit1TargetPos = curPos + hit1MoveResult;
        // gizmo2 = hit1TargetPos;

        if (!Physics.SphereCast(ray1, half_size, out var hit1, MAX_CHECK_DISTANCE))
        {
            // SetLinePos(normalLine1, Vector3.zero, Vector3.up);
            groundTouchState = GroundTouchState.Sliding;
            return hit1MoveResult;
        }

        var hitDistance1 = hit1.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance1 = remaingDistance0 - hitDistance1;
        // SetLinePos(normalLine1, hit1.point, hit1.point + hit1.normal * 3);

        if (remaingDistance1 < 0)
        {
            groundTouchState = GroundTouchState.Sliding;
            return hit1MoveResult;
        }


        hit1MoveResult = moveDir1 * hitDistance1;
        hit1TargetPos = curPos + hit0MoveResult + hit1MoveResult;
        // gizmo2 = hit1TargetPos;

        var moveDir2a = Vector3.Cross(hit0.normal, hit1.normal).normalized;
        var moveDir2b = Vector3.Cross(hit1.normal, hit0.normal).normalized;

        var dot2aD = Vector3.Dot(moveDir2a, moveDir1);
        var dot2bD = Vector3.Dot(moveDir2b, moveDir1);

        var moveDir2 = Vector3.zero;
        if (dot2aD > 0)
        {
            moveDir2 = moveDir2a;
            // SetLinePos(LineRendererDir, hit1TargetPos, hit1TargetPos + moveDir2 * 5);
        }
        else if (dot2bD > 0)
        {
            moveDir2 = moveDir2b;
            // SetLinePos(LineRendererDir, hit1TargetPos, hit1TargetPos + moveDir2 * 5);
        }
        else
        {
            // SetLinePos(LineRendererDir, Vector3.zero, Vector3.up);
            moveDir2 = Vector3.zero;
        }

        curFootDegree = 90 - Vector3.Angle(Vector3.down, moveDir2);
        if (curFootDegree < foot_silde_degree || curFootDegree == 90f)
        {
            groundTouchState = GroundTouchState.Stationary;
            return hit0MoveResult + hit1MoveResult;
        }

        var ray2 = new Ray(hit1TargetPos - moveDir2 * RAY_BACK_DISTANCE, moveDir2 * RAY_BACK_DISTANCE);
        var hit2MoveResult = hit0MoveResult + hit1MoveResult + moveDir2 * remaingDistance1;
        var hit2TargetPos = curPos + hit2MoveResult;
        // gizmo3 = hit2TargetPos;

        if (!Physics.SphereCast(ray2, half_size, out var hit2, MAX_CHECK_DISTANCE))
        {
            // SetLinePos(normalLine2, Vector3.zero, Vector3.up);
            groundTouchState = GroundTouchState.Sliding;
            return hit2MoveResult;
        }

        var hitDistance2 = hit2.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance2 = remaingDistance1 - hitDistance2;
        // SetLinePos(normalLine2, hit2.point, hit2.point + hit2.normal * 3);

        if (remaingDistance2 < 0)
        {
            groundTouchState = GroundTouchState.Sliding;
            return hit2MoveResult;
        }

        //3点タッチしてあれば、もう計算しない

        hit2MoveResult = moveDir2 * hitDistance2;
        hit2TargetPos = curPos + hit1MoveResult + hit2MoveResult;

        // gizmo3 = hit2TargetPos;
        groundTouchState = GroundTouchState.Stationary;
        return hit1MoveResult + hit2MoveResult;
    }

}
