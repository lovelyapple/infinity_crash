using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GamePlayerController : MonoBehaviour
{
    public Transform CameraTransform;
    public Vector3 FallSpeed;
    public float floatDistance = 0.01f;
    public float GravityValue = 9.8f;
    public float MaxFallSpeed = -9.8f;
    public bool IsTouchingGround = false;
    public bool IsJumping = false;
    public float JumpForce = 5.0f;
    public float Friction = 1.0f;  // 摩擦力の強さ


    public Vector3 InputMoveSpeed;
    public float InputAcceleration = 1f;
    public float MaxHorizontalMoveSpeed = 3f;
    public bool IsInputing;

    public float mouseSensitivity = 100f;  // マウス感度

    private float xRotation = 0f;

    void Start()
    {
        // カーソルを画面中央に固定し、カーソルの表示を無効化
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Update()
    {
        // マウスのX軸とY軸の入力を取得
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        if (mouseX != 0 || mouseY != 0)
        {
            // 縦方向（Y軸）の回転を制限
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // 上下の視点回転を90度までに制限

            // カメラの回転を上下方向（X軸）に適用
            CameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            // プレイヤーの体の回転を左右方向（Y軸）に適用
            transform.Rotate(Vector3.up * mouseX);
        }


        IsInputing = false;
        if (Input.GetKey(KeyCode.W))
        {
            if (!Input.GetKey(KeyCode.S) && InputMoveSpeed.z < 0)
            {
                InputMoveSpeed.z = 0;
            }

            InputMoveSpeed.z += InputAcceleration * Time.deltaTime;
            IsInputing = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (!Input.GetKey(KeyCode.W) && InputMoveSpeed.z > 0)
            {
                InputMoveSpeed.z = 0;
            }

            if (InputMoveSpeed.z > 0)
            {
                InputMoveSpeed.z = 0;
            }

            InputMoveSpeed.z -= InputAcceleration * Time.deltaTime;
            IsInputing = true;
        }
        else
        {
            InputMoveSpeed.z = 0;
        }

        if (Input.GetKey(KeyCode.A))
        {
            if (!Input.GetKey(KeyCode.D) && InputMoveSpeed.x > 0)
            {
                InputMoveSpeed.x = 0;
            }

            InputMoveSpeed.x -= InputAcceleration * Time.deltaTime;
            IsInputing = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (!Input.GetKey(KeyCode.A) && InputMoveSpeed.x < 0)
            {
                InputMoveSpeed.x = 0;
            }

            InputMoveSpeed.x += InputAcceleration * Time.deltaTime;
            IsInputing = true;
        }
        else
        {
            InputMoveSpeed.x = 0;
        }

        if (IsInputing)
        {
            InputMoveSpeed.y = 0;

            var strenght = InputMoveSpeed.x * InputMoveSpeed.x + InputMoveSpeed.z * InputMoveSpeed.z;

            var limitScale = strenght / (MaxHorizontalMoveSpeed * MaxHorizontalMoveSpeed);

            if (limitScale > 1)
            {
                InputMoveSpeed.x = InputMoveSpeed.x / limitScale;
                InputMoveSpeed.z = InputMoveSpeed.z / limitScale;
            }
        }
        else
        {
            InputMoveSpeed = Vector3.zero;
        }

        // 今は地面にいるかどうか
        if (IsJumping == false && Physics.BoxCast(transform.position + Vector3.up * floatDistance, Vector3.one * 0.5f, Vector3.down, out var hitInfo, Quaternion.identity, FallSpeed.y))
        {
            FallSpeed = Vector3.zero;
            IsTouchingGround = true;
        }
        else
        {
            IsTouchingGround = false;
        }

        // ジャンプ処理
        if (IsTouchingGround && Input.GetButtonDown("Jump"))  // "Jump" はデフォルトでスペースキー
        {
            FallSpeed.y -= JumpForce;  // ジャンプ力を上向き（Y軸方向に）与える
            IsTouchingGround = false;  // ジャンプ中は接地していない
            IsJumping = true;
        }

        if (!IsTouchingGround)
        {
            // 重力による速度の変化を適用
            FallSpeed.y = Mathf.Max(MaxFallSpeed, FallSpeed.y - GravityValue * Time.deltaTime);

            // 速度に基づいてオブジェクトを移動
            transform.position -= FallSpeed * Time.deltaTime;

            if (FallSpeed.y >= 0)
            {
                IsJumping = false;
            }
        }
        else
        {
            FallSpeed = Vector3.zero;
            IsJumping = false;
        }

        if (InputMoveSpeed.x != 0 || InputMoveSpeed.z != 0)
        {
            var worldDirection = transform.TransformDirection(InputMoveSpeed);
            worldDirection.y = 0;
            transform.position += worldDirection * Time.deltaTime;
        }
    }
}
