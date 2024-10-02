using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePlayerController : MonoBehaviour
{
    private ObjectGravityController _gravityController;
    public Vector3 FallSpeed;
    public float floatDistance = 0.01f;
    public float GravityValue = 9.8f;
    public float MaxFallSpeed = 9.8f;
    public bool IsTouchingGround = false;
    public bool IsJumping = false;
    public float JumpForce = 5.0f;
    public float Friction = 1.0f;  // 摩擦力の強さ
    public void Update()
    {
        if (IsJumping == false && Physics.SphereCast(new Ray(transform.position + Vector3.up * floatDistance, Vector3.down), 0.5f, out var footHit, floatDistance))
        {
            IsTouchingGround = true;
        }
        else{
            IsTouchingGround = false;
        }

        // ジャンプ処理
        if (IsTouchingGround && Input.GetButtonDown("Jump"))  // "Jump" はデフォルトでスペースキー
        {
            FallSpeed.y -= JumpForce;  // ジャンプ力を上向き（Y軸方向に）与える
            IsTouchingGround = false;  // ジャンプ中は接地していない
            IsJumping = true;
        }

        if (FallSpeed.y < 0)  // 上昇中のみ天井との衝突を確認
        {
            if (Physics.SphereCast(new Ray(transform.position + Vector3.down * floatDistance, Vector3.up), 0.5f, out var hit, floatDistance))
            {
                // 斜めの天井に対する法線に沿った速度の調整
                Vector3 slideDirection = Vector3.ProjectOnPlane(FallSpeed, hit.normal);
                FallSpeed = slideDirection;  // 法線に沿った方向に滑らせる

                // 摩擦力を適用
                Vector3 frictionForce = -slideDirection.normalized * Friction;
                FallSpeed += frictionForce * Time.deltaTime; // 摩擦力を速度に加える
            }
        }

        if (!IsTouchingGround)
        {
            // 重力による速度の変化を適用
            FallSpeed.y = Mathf.Min(MaxFallSpeed, FallSpeed.y + GravityValue * Time.deltaTime);

            // 速度に基づいてオブジェクトを移動
            transform.position -= FallSpeed * Time.deltaTime;

            if(FallSpeed.y >= 0)
            {
                IsJumping = false;
            }
        }
        else
        {
            FallSpeed = Vector3.zero;
            IsJumping = false;
        }
    }

    // 何かに衝突した際に呼ばれる
    void OnCollisionEnter(Collision collision)
    {
        if (!IsJumping && IsTouchingGround)
        {
            // 速度をゼロにして落下を停止
            FallSpeed = Vector3.zero;
            IsTouchingGround = true;
        }
    }
}
