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
    public enum GroundTouchState
    {
        Floating,
        Touching_1,
        Touching_2,
        Touching_2_sliding,
    }
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

    const float HIT_CHECK_FLOAT_DISTANCE = 0.05f;

    public GroundTouchState CurrentGroundTouchState;
    private bool _canJump => CurrentGroundTouchState >= GroundTouchState.Touching_1;

    public Vector3 UserInputDicreionLocal;
    public Vector3 CurrentInputMoveLocal;
    public float CurrentMoveSpeed;
    public Vector3 FinalInputVelocityWorld;
    public Vector3 FinalVelocityWorldApply;
    public Vector3 FallVelocityWorldApply;

    public Vector3 JumpVelocityWorldApply;
    public bool IsJumping;

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
        if (Physics.Raycast(transform.position, transform.forward, out var hit, maxDistance: 100))
        {
            frontDistance = hit.distance;
            hitNormalLine.SetPositions(new Vector3[] { hit.point, hit.point + hit.normal * 3f });
            moveLine.SetPositions(new Vector3[]{transform.position, transform.position + transform.forward * hit.distance});
        }

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

            // if (mouseX != 0 || mouseY != 0)
            {
                // 縦方向（Y軸）の回転を制限
                _xRotaition -= mouseY;
                _xRotaition = Mathf.Clamp(_xRotaition, -90f, 90f);  // 上下の視点回転を90度までに制限

                // カメラの回転を上下方向（X軸）に適用
                // _cmemraTransform.localRotation = Quaternion.Euler(_xRotaition, 0f, 0f);
                // // プレイヤーの体の回転を左右方向（Y軸）に適用
                // transform.Rotate(Vector3.up * mouseX);
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

        FinalVelocityWorldApply = FinalInputVelocityWorld * Time.deltaTime;

        if (FinalVelocityWorldApply.x != 0 || FinalVelocityWorldApply.z != 0)
        {
            FinalVelocityWorldApply = MoveAlongWall(FinalVelocityWorldApply);
            // FinalVelocityWorldApply = MoveAlongWall(FinalVelocityWorldApply);
            transform.position += FinalVelocityWorldApply;
            // var result = GetHitPlaneVect(FinalVelocityWorldApply);
            //
            // if(result.HasValue)
            // {
            //     FinalVelocityWorldApply = result.Value;
            // }
        }

        // CurrentGroundTouchState = CheckGroundTouch();
        // CheckJumpTouch();
        //
        // if (_canJump)
        // {
        //     if(Input.GetKeyDown(KeyCode.Space))
        //     {
        //         JumpVelocityWorldApply.y += JUMP_POWER;
        //         IsJumping = true;
        //     }
        // }
        //
        // // 反映
        // if (FinalVelocityWorldApply.x != 0 || FinalVelocityWorldApply.y != 0 || FinalVelocityWorldApply.z != 0)
        // {
        //     transform.position += FinalVelocityWorldApply;
        // }
        //
        // if(FallVelocityWorldApply != Vector3.zero)
        // {
        //     transform.position += FallVelocityWorldApply * Time.deltaTime;
        // }
        //
        // if(JumpVelocityWorldApply != Vector3.zero)
        // {
        //     transform.position += JumpVelocityWorldApply * Time.deltaTime;
        // }

        // OnEdtiroExecute();
    }
    [ContextMenu("AAAAA")]
    public void OnEdtiroExecute()
    {
        var ray = new Ray() { origin = transform.position, direction = transform.forward };
        if (Physics.SphereCast(ray, BODY_SIZE_HALF, out var hit))
        {
            moveLine.SetPositions(new Vector3[] { hit.point, hit.point + -ray.direction * hit.distance});
        }
    }
    private Vector3? GetHitPlaneVect(Vector3 moveDirection)
    {
        var moveDistance = Mathf.Sqrt(moveDirection.x * moveDirection.x + moveDirection.z * moveDirection.z);
        var direction = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;
        var castDistance = moveDistance + HIT_CHECK_FLOAT_DISTANCE;
        if (Physics.SphereCast(transform.position, BODY_SIZE_HALF, direction, out var hitInfo, castDistance))
        {
            var hitOriginPoint = hitInfo.point + hitInfo.normal * (HIT_CHECK_FLOAT_DISTANCE + BODY_SIZE_HALF);
            // hitNormalLine.SetPositions(new Vector3[] { hitOriginPoint, hitOriginPoint + hitInfo.normal * 5 });
            var horizontalHitMovedDistance = hitInfo.distance - HIT_CHECK_FLOAT_DISTANCE;
            // moveLine.SetPositions(new Vector3[] { transform.position, hitOriginPoint });

            // if (hitMovedDistance < moveDistance)
            {
                var newVelocityWorld = direction * horizontalHitMovedDistance;

                var invertedNormal = -hitInfo.normal;
                var directionNormalized = direction.normalized;
                Vector3 projectedDirection = directionNormalized - Vector3.Dot(directionNormalized, invertedNormal) * invertedNormal;
                var projectedDirectionDistance = moveDistance - horizontalHitMovedDistance;

                newVelocityWorld += projectedDirection * projectedDirectionDistance;
                // fixLine.SetPositions(new Vector3[] { hitOriginPoint, hitOriginPoint + projectedDirection * projectedDirectionDistance });
                return newVelocityWorld;
            }
        }

        return null;
    }

    public Vector3 MoveAlongWall(Vector3 move)
    {
        var curPos = transform.position;
        var requestMoveDistance = move.magnitude;
        var requestMoveDirNormalized = move.normalized;

        // SphereCastAllで衝突するすべての壁を取得dw
        var hits = Physics.SphereCastAll(new Ray(curPos, requestMoveDirNormalized), BODY_SIZE_HALF, requestMoveDistance + HIT_CHECK_FLOAT_DISTANCE);

        if (hits.Length == 0)
        {
            // 衝突がなければ通常の移動を返す
            return move;
        }

        Vector3 slideDirection = requestMoveDirNormalized;
        Vector3 intersectionDirection = Vector3.zero;
        hits = hits.Where(x => x.point != Vector3.zero).ToArray();

        if (hits.Length == 0)
        {
            // 衝突がなければ通常の移動を返す
            return move;
        }

        if (hits.Length >= 3)
        {
            // 3つ以上の壁にヒットしている場合、完全に動けないと判定してゼロ移動
            return Vector3.zero;
        }
        else if (hits.Length == 2)
        {
            // 最初の2つの壁法線を取得
            Vector3 normal1 = hits[0].normal;
            Vector3 normal2 = hits[1].normal;

            // 法線の向きに基づいて水平面と斜面を判別
            bool isGround1 = hits[0].point == Vector3.zero;
            bool isGround2 = hits[1].point == Vector3.zero;

            if (isGround1 || isGround2)
            {

                // 斜面の法線を取得
                var normalHit = isGround1 ? hits[1] : hits[0];
                // 2つの壁がどちらも垂直面の場合、交差方向でスライド
                intersectionDirection = Vector3.Cross(normalHit.normal, Vector3.up).normalized;
                slideDirection = Vector3.Project(requestMoveDirNormalized, intersectionDirection).normalized;

                var hit = normalHit;
                var slideDistance = (hit.distance - HIT_CHECK_FLOAT_DISTANCE);
                return slideDirection * slideDistance;
            }
            else
            {
                // 2つの壁がどちらも垂直面の場合、交差方向でスライド
                intersectionDirection = Vector3.Cross(normal1, normal2).normalized;
                slideDirection = Vector3.Project(requestMoveDirNormalized, intersectionDirection).normalized;

                var hit = hits.OrderBy(x => x.distance).First();
                var movedDistance = hit.distance - HIT_CHECK_FLOAT_DISTANCE;
                var slideDistance = requestMoveDistance - (hit.distance - HIT_CHECK_FLOAT_DISTANCE);
                return requestMoveDirNormalized * movedDistance + slideDirection * slideDistance;
            }
        }
        else
        {
            // 壁が1つだけの場合、その法線に基づいてスライド方向を決定
            var hit = hits[0];
            slideDirection = Vector3.ProjectOnPlane(requestMoveDirNormalized, hit.normal).normalized;
            var movedDistance = hit.distance - HIT_CHECK_FLOAT_DISTANCE;
            var slideDistance = requestMoveDistance - movedDistance;

            var requestMove = requestMoveDirNormalized * movedDistance + slideDirection * slideDistance;

            var newHits = Physics.SphereCastAll(new Ray(curPos, requestMove.normalized), BODY_SIZE_HALF, requestMove.magnitude )
                .OrderBy(x => x.distance).ToArray();

            if (newHits.Length >= 3 || newHits.Any(x => x.point == Vector3.zero))
            {
                // もう面倒いので、計算したくない
                return requestMoveDirNormalized * movedDistance;
            }

            var newHit = newHits.FirstOrDefault(x => x.transform != hit.transform);

            if (newHit.transform != null)
            {
                // 2つの壁がどちらも垂直面の場合、交差方向でスライド
                intersectionDirection = Vector3.Cross(hit.normal, newHit.normal).normalized;
                slideDirection = Vector3.Project(requestMoveDirNormalized, intersectionDirection).normalized;

                hit = hits.OrderBy(x => x.distance).First();
                movedDistance = hit.distance - HIT_CHECK_FLOAT_DISTANCE;
                slideDistance = requestMoveDistance - (hit.distance - HIT_CHECK_FLOAT_DISTANCE);
                return requestMoveDirNormalized * movedDistance + slideDirection * slideDistance;
            }

            return requestMove;
        }
    }
    public float frontDistance = 0;
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



    private GroundTouchState CheckGroundTouch()
    {
        if(IsJumping)
        {
            return GroundTouchState.Floating;
        }
        
        // moveLine.SetPositions(new Vector3[] { curPos, wallHitOriginPos});
        // hitNormalLine.SetPositions(new Vector3[] { wallHitOriginPos, hit.point + hit.normal * 5});
        // fixLine.SetPositions(new Vector3[] { wallHitOriginPos, wallHitOriginPos + slideDirection * 5});

        var gravityVelocity = FALL_GRAVITY * Time.deltaTime;
        var dropDistanceNextFrame = Mathf.Min(-FallVelocityWorldApply.y + gravityVelocity, MAX_GRAVITY_SPEED) * Time.deltaTime;
        var dropRay = new Ray() { origin = transform.position + Vector3.up * 0.001f, direction = Vector3.down };

        var groundTouchState = GroundTouchState.Floating;

        var distance = dropDistanceNextFrame + HIT_CHECK_FLOAT_DISTANCE;
        if (Physics.SphereCast(dropRay, BODY_SIZE_HALF, out var hitInfo2 , distance))
        {
            groundTouchState = GroundTouchState.Touching_2;
            FootGroundAngle = Vector3.Angle(Vector3.up, hitInfo2.normal);

            if (FootGroundAngle > GROUND_SLIDE_DEGREE)
            {
                var invertHitNormalNormalized = -hitInfo2.normal;
                var hitOriginPoint = hitInfo2.point + hitInfo2.normal * (HIT_CHECK_FLOAT_DISTANCE + BODY_SIZE_HALF);
                var hitDistance = Vector3.Distance(hitOriginPoint, transform.position);
                var newFallDirection = new Vector3(FallVelocityWorldApply.x, -1 * hitDistance, FallVelocityWorldApply.z);
                var slipDistance = gravityVelocity - hitDistance;
                var projectedDirection = Vector3.down - Vector3.Dot(Vector3.down, invertHitNormalNormalized) * invertHitNormalNormalized;
                newFallDirection += projectedDirection * slipDistance;
                FallVelocityWorldApply = newFallDirection;

                fixLine.SetPositions(new Vector3[] { hitOriginPoint, hitOriginPoint + projectedDirection * slipDistance });
                groundTouchState = GroundTouchState.Touching_2_sliding;
            }
            else
            {
                FallVelocityWorldApply = Vector3.zero;
            }
        }
        else if (Physics.SphereCast(dropRay, BODY_SIZE_HALF, out var hitInfo1, distance * 1.2f))
        {
            groundTouchState = GroundTouchState.Touching_1;
        }
        else
        {
            FallVelocityWorldApply.y -= gravityVelocity;
        }

        return groundTouchState;
    }
    private void CheckJumpTouch()
    {
        if(!IsJumping)
        {
            return;
        }

        JumpVelocityWorldApply.y -= JUMP_GRAVITY * Time.deltaTime;

        if (JumpVelocityWorldApply.y < 0)
        {
            IsJumping = false;
            JumpVelocityWorldApply = Vector3.zero;
            // ここ1フレームのJump失いがある
            return;
        }

        var jumpRay = new Ray() { origin = transform.position, direction = Vector3.up };
        var jumpDistanceNextFrame = JumpVelocityWorldApply.y * Time.deltaTime;
        var rayDistance = jumpDistanceNextFrame + HIT_CHECK_FLOAT_DISTANCE;

        if (Physics.SphereCast(jumpRay, BODY_SIZE_HALF, out var hitInfo, rayDistance))
        {
            headGroundAngle = Vector3.Angle(Vector3.down, hitInfo.normal);

            if (headGroundAngle > HEAD_SLIDE_DEEGREE)
            {
                var invertHitNormalNormalized = -hitInfo.normal;
                var hitOriginPoint = hitInfo.point + hitInfo.normal * (HIT_CHECK_FLOAT_DISTANCE + BODY_SIZE_HALF);
                var hitDistance = Vector3.Distance(hitOriginPoint, transform.position);
                var newJumpDirection = new Vector3(JumpVelocityWorldApply.x, hitDistance, JumpVelocityWorldApply.z);
                var slipDistance = jumpDistanceNextFrame - hitDistance;
                var projectedDirection = Vector3.up - Vector3.Dot(Vector3.up, invertHitNormalNormalized) * invertHitNormalNormalized;
                newJumpDirection += projectedDirection * slipDistance;
                JumpVelocityWorldApply = newJumpDirection;
            }
            else
            {
                JumpVelocityWorldApply.y = 0;
            }
        }
    }
    public float headGroundAngle;
}
