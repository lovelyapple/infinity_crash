using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
    public float FALL_GRAVITY;
    public float JUMP_GRAVITY;
    public float MAX_GRAVITY_SPEED;
    public float GROUND_SLIDE_DEGREE = 45;

    public float HEAD_SLIDE_DEEGREE = 75;


    public float JUMP_POWER;
    public float BODY_SIZE_HALF;
    public float HOR_INPUT_SPPED;
    public float MAX_HOR_INPUT_SPEED;
    public float DRAG_VALUE = 0.97f;
    public float MIN_MOVE_POWER = 0.1f;
    

    public Vector3 UserInputDicreionLocal;
    public Vector3 CurrentInputMoveLocal;
    public float CurrentMoveSpeed;
    public Vector3 FinalInputVelocityWorld;
    public Vector3 FinalVelocityWorldApply;

    public Transform _cmemraTransform;
    public bool AllowRotate;
    float _xRotaition;
    public float MouseSensitivity;
    private bool _isLoackedCursor;
    public float FootGroundAngle = 0;
    public LineRenderer moveLine;
    public LineRenderer fixLine;
    public LineRenderer hitNormalLine;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!_isLoackedCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                _isLoackedCursor = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                _isLoackedCursor = false;
            }
        }

        // 毎回初期化する
        FinalInputVelocityWorld = Vector3.zero;


        ///----- カメラ回転-----///
        if (AllowRotate)
        {
            // マウスのX軸とY軸の入力を取得
            var mouseSensitivity = MouseSensitivity;
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            if (mouseX != 0 || mouseY != 0)
            {
                // 縦方向（Y軸）の回転を制限
                _xRotaition -= mouseY;
                _xRotaition = Mathf.Clamp(_xRotaition, -90f, 90f); // 上下の視点回転を90度までに制限

                // カメラの回転を上下方向（X軸）に適用
                _cmemraTransform.localRotation = Quaternion.Euler(_xRotaition, 0f, 0f);
                // // プレイヤーの体の回転を左右方向（Y軸）に適用
                transform.Rotate(Vector3.up * mouseX);
            }
        }

        // Input受付
        if (Input.GetKey(KeyCode.W))
        {
            UserInputDicreionLocal.z = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            UserInputDicreionLocal.z = -1;
        }
        else
        {
            UserInputDicreionLocal.z = 0;
        }

        if (Input.GetKey(KeyCode.D))
        {
            UserInputDicreionLocal.x = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            UserInputDicreionLocal.x = -1;
        }
        else
        {
            UserInputDicreionLocal.x = 0;
        }

        // Inoutを加速度に変換
        if (UserInputDicreionLocal.x != 0)
        {
            if (CurrentInputMoveLocal.x * UserInputDicreionLocal.x < 0)
            {
                CurrentInputMoveLocal.x *= DRAG_VALUE;

                if (CurrentInputMoveLocal.x * CurrentInputMoveLocal.x <= MIN_MOVE_POWER * MIN_MOVE_POWER)
                {
                    CurrentInputMoveLocal.x = 0;
                }
            }

            CurrentInputMoveLocal.x += UserInputDicreionLocal.x * HOR_INPUT_SPPED * Time.deltaTime;
        }
        else
        {
            CurrentInputMoveLocal.x = 0;
        }

        if (UserInputDicreionLocal.z != 0)
        {
            if (CurrentInputMoveLocal.z * UserInputDicreionLocal.z < 0)
            {
                CurrentInputMoveLocal.z *= DRAG_VALUE;

                if (CurrentInputMoveLocal.z * CurrentInputMoveLocal.z <= MIN_MOVE_POWER * MIN_MOVE_POWER)
                {
                    CurrentInputMoveLocal.z = 0;
                }
            }

            CurrentInputMoveLocal.z += UserInputDicreionLocal.z * HOR_INPUT_SPPED * Time.deltaTime;
        }
        else
        {
            CurrentInputMoveLocal.z = 0;
        }

        // Input加速度の最大限を求める
        if (CurrentInputMoveLocal.x != 0 || CurrentInputMoveLocal.z != 0)
        {
            CurrentInputMoveLocal.y = 0;
            CurrentMoveSpeed = CurrentInputMoveLocal.sqrMagnitude;

            var powerScale = MAX_HOR_INPUT_SPEED / CurrentMoveSpeed;

            if (powerScale < 1)
            {
                CurrentInputMoveLocal.x *= powerScale;
                CurrentInputMoveLocal.z *= powerScale;
            }
        }
        else
        {
            CurrentMoveSpeed = 0;
        }

        // 自然運動中の摩擦力計算
        // TODO 地面判定
        if (UserInputDicreionLocal.x == 0 || (UserInputDicreionLocal.x * FinalInputVelocityWorld.x) < 0)
        {
            FinalInputVelocityWorld.x *= DRAG_VALUE;
        }

        if (UserInputDicreionLocal.z == 0 || (UserInputDicreionLocal.z * FinalInputVelocityWorld.z) < 0)
        {
            FinalInputVelocityWorld.z *= DRAG_VALUE;
        }


        // WorldDirecionに変換
        if (CurrentMoveSpeed != 0)
        {
            FinalInputVelocityWorld = transform.TransformDirection(CurrentInputMoveLocal);
        }
        else
        {
            // todo 摩擦力計算
            MovePerFrame = Vector3.zero;
        }

        FinalVelocityWorldApply = FinalInputVelocityWorld * Time.deltaTime;
        MovePerFrame = RequestAddSpeed(FinalVelocityWorldApply);

        var prevState = CurrentGroundTouchState;
        DirectDownSpeed = Mathf.Min(MAX_GRAVITY_SPEED, DirectDownSpeed + FALL_GRAVITY * Time.deltaTime);
        var vec = CheckGravity(Vector3.down * DirectDownSpeed * Time.deltaTime);

        // 字面にタッチした瞬間だけ、重力加速度を0にする（疑似反発力）
        if (prevState == GroundTouchState.Floating && prevState != CurrentGroundTouchState)
        {
            DirectDownSpeed = 0;
        }

        MovePerFrame += vec;

        if (MovePerFrame.magnitude != 0)
        {
            var frameMove = CheckPhysic1(MovePerFrame);
            transform.position += frameMove;
        }
    }

    public float DirectDownSpeed;
    public Vector3 MovePerFrame;
    public Vector3 RequestAddSpeed(Vector3 spdAddFrame)
    {
        var curFrameMaxSpeed = MAX_HOR_INPUT_SPEED * Time.deltaTime;
        var requestMove = MovePerFrame + spdAddFrame;

        var curSpeedLength = MovePerFrame.magnitude;
        var scale = curFrameMaxSpeed / curSpeedLength;

        if(scale < 1)
        {
            requestMove *= scale;
        }

        return requestMove;
    }
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

        if (hits.Count >= 3)
        {
            moveDistance0 = Mathf.Max(0, hits[0].distance - RAY_BACK_DISTANCE - FLOAT_AROUND);
            // gizmo1 = curPos + moveDir0 * moveDistance0;
            return moveDir0 * moveDistance0;
        }
        else if (hits.Count == 2)
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
                // SetLinePos(LineRendererDir, hit20TargetPos, hit20TargetPos + moveDir21 * 5);
            }
            else if (dot20bD > 0)
            {
                moveDir21 = moveDirCross21;
                // SetLinePos(LineRendererDir, hit20TargetPos, hit20TargetPos + moveDir21 * 5);
            }
            else
            {
                // SetLinePos(LineRendererDir, Vector3.zero, Vector3.up);
                moveDir21 = Vector3.zero;
            }

            var ray21 = new Ray(hit20TargetPos - moveDir21 * RAY_BACK_DISTANCE, moveDir21 * RAY_BACK_DISTANCE);
            var hit21MoveResult = hit20MoveResult + moveDir21 * remainingDistance21;
            var hit21TargetPos = curPos + hit21MoveResult;
            // gizmo3 = hit20TargetPos;

            if (!Physics.SphereCast(ray21, HALF_SIZE, out var hit22, MAX_CHECK_DISTANCE))
            {
                // SetLinePos(normalLine2, Vector3.zero, Vector3.up);
                return hit21MoveResult;
            }

            var hitDistance22 = hit22.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
            var remainingDistance22 = remainingDistance21 - hitDistance22;
            // SetLinePos(normalLine2, hit22.point, hit22.point + hit22.normal * 3);

            if (remainingDistance22 < 0)
            {
                return hit21MoveResult;
            }

            hit21MoveResult = moveDir21 * hitDistance22;
            hit21TargetPos = curPos + hit20MoveResult + hit21MoveResult;

            // gizmo3 = hit21TargetPos;
            return hit20MoveResult + hit21MoveResult;

        }


        if (!Physics.SphereCast(ray0, HALF_SIZE, out var hit0, MAX_CHECK_DISTANCE))
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
        var moveDistance1 = remaingDistance0;
        var ray1 = new Ray(hit0TargetPos - moveDir1 * RAY_BACK_DISTANCE, moveDir1);

        // もしヒットしなかった時のResult用意
        var hit1MoveResult = hit0MoveResult + moveDir1 * moveDistance1;
        var hit1TargetPos = curPos + hit1MoveResult;
        // gizmo2 = hit1TargetPos;

        if (!Physics.SphereCast(ray1, HALF_SIZE, out var hit1, MAX_CHECK_DISTANCE))
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

        if (!Physics.SphereCast(ray2, HALF_SIZE, out var hit2, MAX_CHECK_DISTANCE))
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
    public enum GroundTouchState
    {
        Floating,
        Sliding,
        Stationary,
    }

    public GroundTouchState CurrentGroundTouchState;
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

        if (hits.Count >= 3)
        {
            moveDistance0 = Mathf.Max(0, hits[0].distance - RAY_BACK_DISTANCE - FLOAT_AROUND);
            // gizmo1 = curPos + moveDir0 * moveDistance0;
            CurrentGroundTouchState = GroundTouchState.Stationary;
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


        if (!Physics.SphereCast(ray0, HALF_SIZE, out var hit0, MAX_CHECK_DISTANCE))
        {
            // SetLinePos(normalLine0, Vector3.zero, Vector3.up);
            CurrentGroundTouchState = GroundTouchState.Floating;
            return move;
        }

        var hitDistance0 = hit0.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance0 = moveDistance0 - hitDistance0;

        // gizmo1 = curPos + move;
        // SetLinePos(normalLine0, hit0.point, hit0.point + hit0.normal * 3);

        // ヒット距離がmove距離より長いので、そのまま進むことができる
        if (remaingDistance0 < 0)
        {
            CurrentGroundTouchState = GroundTouchState.Floating;
            return move;
        }


        // そうでなければ、ヒットした手前まで移動する座標を作る
        var hit0MoveResult = moveDir0 * hitDistance0;
        var hit0TargetPos = curPos + hit0MoveResult;
        // gizmo1 = hit0TargetPos;

        // 横滑りの情報用意
        var moveDir1 = Vector3.ProjectOnPlane(moveDir0, hit0.normal).normalized;

        FootGroundAngle = 90 - Vector3.Angle(Vector3.down, moveDir1);

        if (FootGroundAngle < GROUND_SLIDE_DEGREE || FootGroundAngle == 90f)
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
        // gizmo2 = hit1TargetPos;

        if (!Physics.SphereCast(ray1, HALF_SIZE, out var hit1, MAX_CHECK_DISTANCE))
        {
            // SetLinePos(normalLine1, Vector3.zero, Vector3.up);
            CurrentGroundTouchState = GroundTouchState.Sliding;
            return hit1MoveResult;
        }

        var hitDistance1 = hit1.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance1 = remaingDistance0 - hitDistance1;
        // SetLinePos(normalLine1, hit1.point, hit1.point + hit1.normal * 3);

        if (remaingDistance1 < 0)
        {
            CurrentGroundTouchState = GroundTouchState.Sliding;
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

        FootGroundAngle = 90 - Vector3.Angle(Vector3.down, moveDir2);
        if (FootGroundAngle < GROUND_SLIDE_DEGREE || FootGroundAngle == 90f)
        {
            CurrentGroundTouchState = GroundTouchState.Stationary;
            return hit0MoveResult + hit1MoveResult;
        }

        var ray2 = new Ray(hit1TargetPos - moveDir2 * RAY_BACK_DISTANCE, moveDir2 * RAY_BACK_DISTANCE);
        var hit2MoveResult = hit0MoveResult + hit1MoveResult + moveDir2 * remaingDistance1;
        var hit2TargetPos = curPos + hit2MoveResult;
        // gizmo3 = hit2TargetPos;

        if (!Physics.SphereCast(ray2, HALF_SIZE, out var hit2, MAX_CHECK_DISTANCE))
        {
            // SetLinePos(normalLine2, Vector3.zero, Vector3.up);
            CurrentGroundTouchState = GroundTouchState.Sliding;
            return hit2MoveResult;
        }

        var hitDistance2 = hit2.distance - RAY_BACK_DISTANCE - FLOAT_AROUND;
        var remaingDistance2 = remaingDistance1 - hitDistance2;
        // SetLinePos(normalLine2, hit2.point, hit2.point + hit2.normal * 3);

        if (remaingDistance2 < 0)
        {
            CurrentGroundTouchState = GroundTouchState.Sliding;
            return hit2MoveResult;
        }

        //3点タッチしてあれば、もう計算しない

        hit2MoveResult = moveDir2 * hitDistance2;
        hit2TargetPos = curPos + hit1MoveResult + hit2MoveResult;

        // gizmo3 = hit2TargetPos;
        CurrentGroundTouchState = GroundTouchState.Stationary;
        return hit1MoveResult + hit2MoveResult;
    }
    // public Vector3 ColliderCheck(Vector3 requestMove)
    // {
    //     var requestMoveDistance = requestMove.magnitude;
    //     var requestMoveDir = requestMove.normalized;
    //
    //     if (requestMoveDistance == 0)
    //     {
    //         return Vector3.zero;
    //     }
    //
    //     const float MOVE_FLOAT_DISTANCE = 0.001f;
    //     const float FLOAT_ROUND_OFF = 0.0001f;//　壁から絶対に離れている保証floatの丸目誤差補完用
    //     const float MAX_CHECK_DISTANCE = 3f;
    //
    //     var firstRay = new Ray(transform.position - requestMoveDir * MOVE_FLOAT_DISTANCE, requestMoveDir);
    //     if(Physics.SphereCast(firstRay, BODY_SIZE_HALF, out var hitInfo, MAX_CHECK_DISTANCE))
    //     {
    //         /*
    //         |--FLOAT--|-----HALF-----|--→|WALL
    //         */
    //         // hitInfo.distanceが0.00099994
    //         var hitDistance = hitInfo.distance - MOVE_FLOAT_DISTANCE;
    //         var firstMoveResult = requestMoveDir * (requestMoveDistance - FLOAT_ROUND_OFF); ;
    //
    //         // 壁の中に刺さっている
    //         if (hitDistance < 0)
    //         {
    //             
    //         }
    //
    //         if(hitDistance >= requestMoveDistance)
    //         {
    //             return firstMoveResult;
    //         }
    //         
    //
    //         var projectionDistance = requestMoveDistance - hitDistance - FLOAT_ROUND_OFF;
    //
    //         if(projectionDistance < 0)
    //         {
    //             return firstMoveResult;
    //         }
    //         var forwardMove = requestMoveDir * hitDistance;
    //         var forwardOrigin = transform.position + forwardMove;
    //         var projection = Vector3.ProjectOnPlane(requestMoveDir, hitInfo.normal).normalized;
    //         var projectionMove = forwardMove + projection * projectionDistance;
    //
    //         var secRay = new Ray(transform.position + forwardMove, projection);
    //
    //         if(Physics.SphereCast(secRay, BODY_SIZE_HALF, out var secHit, projectionDistance))
    //         {
    //             var secHitDistance = secHit.distance - MOVE_FLOAT_DISTANCE;
    //
    //             if (secHitDistance <= 0)
    //             {
    //                 return projectionMove;
    //             }
    //
    //             var crossDirection = Vector3.Cross(hitInfo.normal, secHit.normal).normalized;
    //             return projectionMove + crossDirection * (projectionDistance - secHitDistance);
    //         }
    //         else
    //         {
    //             return projectionMove;
    //         }
    //     }
    //
    //     return requestMove;
    //
    // }
    // public Vector3 MoveAlongWall(Vector3 move)
    // {
    //     var curPos = transform.position;
    //     var requestMoveDistance = move.magnitude;
    //     var requestMoveDirNormalized = move.normalized;

    //     // SphereCastAllで衝突するすべての壁を取得
    //     var hits = Physics.SphereCastAll(new Ray(curPos, requestMoveDirNormalized), BODY_SIZE_HALF, requestMoveDistance + HIT_CHECK_FLOAT_DISTANCE);

    //     if (hits.Length == 0)
    //     {
    //         // 衝突がなければ通常の移動を返す
    //         return move;
    //     }

    //     Vector3 slideDirection = requestMoveDirNormalized;
    //     Vector3 intersectionDirection = Vector3.zero;
    //     hits = hits.Where(x => x.point != Vector3.zero).ToArray();

    //     if (hits.Length == 0)
    //     {
    //         // 衝突がなければ通常の移動を返す
    //         return move;
    //     }

    //     if (hits.Length >= 3)
    //     {
    //         // 3つ以上の壁にヒットしている場合、完全に動けないと判定してゼロ移動
    //         return Vector3.zero;
    //     }
    //     // else if (hits.Length == 2)
    //     // {
    //     //     // 最初の2つの壁法線を取得
    //     //     Vector3 normal1 = hits[0].normal;
    //     //     Vector3 normal2 = hits[1].normal;

    //     //     // 法線の向きに基づいて水平面と斜面を判別
    //     //     bool isGround1 = hits[0].point == Vector3.zero;
    //     //     bool isGround2 = hits[1].point == Vector3.zero;

    //     //     if (isGround1 || isGround2)
    //     //     {
    //     //         // 斜面の法線を取得
    //     //         var normalHit = isGround1 ? hits[1] : hits[0];
    //     //         // 2つの壁がどちらも垂直面の場合、交差方向でスライド
    //     //         intersectionDirection = Vector3.Cross(normalHit.normal, Vector3.up).normalized;
    //     //         slideDirection = Vector3.Project(requestMoveDirNormalized, intersectionDirection).normalized;

    //     //         var hit = normalHit;
    //     //         var slideDistance = (hit.distance - HIT_CHECK_FLOAT_DISTANCE);
    //     //         return slideDirection * slideDistance;
    //     //     }
    //     //     else
    //     //     {
    //     //         // 2つの壁がどちらも垂直面の場合、交差方向でスライド
    //     //         intersectionDirection = Vector3.Cross(normal1, normal2).normalized;
    //     //         slideDirection = Vector3.Project(requestMoveDirNormalized, intersectionDirection).normalized;

    //     //         var hit = hits.OrderBy(x => x.distance).First();
    //     //         var movedDistance = hit.distance - HIT_CHECK_FLOAT_DISTANCE;
    //     //         var slideDistance = requestMoveDistance - (hit.distance - HIT_CHECK_FLOAT_DISTANCE);
    //     //         return requestMoveDirNormalized * movedDistance + slideDirection * slideDistance;
    //     //     }
    //     // }
    //     else
    //     {
    //         // 壁が1つだけの場合、その法線に基づいてスライド方向を決定
    //         var hit = hits[0];
    //         slideDirection = Vector3.ProjectOnPlane(requestMoveDirNormalized, hit.normal).normalized;
    //         var movedDistance = hit.distance - HIT_CHECK_FLOAT_DISTANCE;
    //         var slideDistance = requestMoveDistance - movedDistance;

    //         var requestMove = requestMoveDirNormalized * movedDistance + slideDirection * slideDistance;
    //         var nextOrigin = curPos + requestMoveDirNormalized * movedDistance;

    //         var newHits = Physics.SphereCastAll(new Ray(nextOrigin, slideDirection), BODY_SIZE_HALF, slideDistance)
    //             .OrderBy(x => x.distance).ToArray();

    //         if (newHits.Length >= 3 || newHits.Any(x => x.point == Vector3.zero))
    //         {
    //             // もう面倒いので、計算したくない
    //             return requestMoveDirNormalized * movedDistance;
    //         }

    //         var newHit = newHits.FirstOrDefault(x => x.transform != hit.transform);

    //         if (newHit.transform != null)
    //         {
    //             // 2つの壁がどちらも垂直面の場合、交差方向でスライド
    //             intersectionDirection = Vector3.Cross(hit.normal, newHit.normal).normalized;
    //             slideDirection = Vector3.Project(requestMoveDirNormalized, intersectionDirection).normalized;

    //             hit = hits.OrderBy(x => x.distance).First();
    //             movedDistance = hit.distance - HIT_CHECK_FLOAT_DISTANCE;
    //             slideDistance = requestMoveDistance - (hit.distance - HIT_CHECK_FLOAT_DISTANCE);
    //             return requestMoveDirNormalized * movedDistance + slideDirection * slideDistance;
    //         }

    //         return requestMove;
    //     }
    // }
    // public float frontDistance = 0;
    // public Vector3 MoveAlongWall(Vector3 move)
    // {
    //     var curPos = transform.position;
    //     var requestMoveDistance = move.magnitude;
    //     var requestMoveDirNormalized = move.normalized;
    //     var ray = new Ray() { origin = curPos, direction = requestMoveDirNormalized };

    //     if (Physics.SphereCast(ray, BODY_SIZE_HALF, out var hit, requestMoveDistance + HIT_CHECK_FLOAT_DISTANCE))
    //     {
    //         Vector3 wallNormal = hit.normal;
    //         Vector3 slideDirection = Vector3.ProjectOnPlane(requestMoveDirNormalized, wallNormal).normalized;

    //         var movedDistance = hit.distance - HIT_CHECK_FLOAT_DISTANCE;
    //         var slideDistance = requestMoveDistance - (hit.distance - HIT_CHECK_FLOAT_DISTANCE);

    //         return requestMoveDirNormalized * movedDistance + slideDirection * slideDistance;
    //     }

    //     return move;
    // }
}
