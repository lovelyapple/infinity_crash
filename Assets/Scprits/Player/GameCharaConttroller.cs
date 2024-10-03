using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharaConttroller : MonoBehaviour
{
    public Transform CameraTransform;
    Rigidbody _rigidbody;
    CapsuleCollider _collider;
    float _distToGround;
    public float JumpForce = 5f;
    public float mouseSensitivity = 100f;  // マウス感度
    private float xRotation = 0f;

    public Vector3 InputMoveSpeed;
    public float InputAcceleration = 1f;
    public float MaxHorizontalMoveSpeed = 3f;
    public bool IsInputing;

    public bool IsGrounded = false;
    public bool HasJumpInertia = false;
    public Vector3 JumpInertia;
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _distToGround = _collider.bounds.extents.y;

        // カーソルを画面中央に固定し、カーソルの表示を無効化
        Cursor.lockState = CursorLockMode.Locked;
    }
    // Update is called once per frame
    void Update()
    {
        // マウスのX軸とY軸の入力を取得
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // if (mouseX != 0 || mouseY != 0)
        {
            // 縦方向（Y軸）の回転を制限
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // 上下の視点回転を90度までに制限

            // カメラの回転を上下方向（X軸）に適用
            CameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            // プレイヤーの体の回転を左右方向（Y軸）に適用
            transform.Rotate(Vector3.up * mouseX);
        }

        CheckIsGrounded();

        if (IsGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                _rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
                JumpInertia = _rigidbody.velocity;
            }
        }
    }

    void FixedUpdate()
    {
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

        if (InputMoveSpeed.x != 0 || InputMoveSpeed.z != 0)
        {
            var curInputSpeed = InputMoveSpeed;
            if (!IsGrounded)
            {
                curInputSpeed.x = curInputSpeed.x * 0.5f;
                curInputSpeed.z = curInputSpeed.z * 0.5f;
            }

            worldDirection = transform.TransformDirection(curInputSpeed);

            if (!IsGrounded)
            {
                Vector3 start = transform.position + Vector3.up * (_collider.height / 2 - _collider.radius); // 上部
                Vector3 end = transform.position - Vector3.up * (_collider.height / 2 - _collider.radius); // 下部
                float radius = _collider.radius - 0.1f;
                if (Physics.CapsuleCast(start, end, radius, worldDirection, out var hit, 0.2f))
                {
                    // 法線ベクトルを取得して、それに基づいて進行方向を調整
                    Vector3 reflectDirection = Vector3.Reflect(worldDirection, hit.normal);
                    worldDirection = new Vector3(reflectDirection.x, _rigidbody.velocity.y, reflectDirection.z);
                }
            }

            if (HasJumpInertia)
            {
                _rigidbody.velocity = JumpInertia + new Vector3(worldDirection.x, _rigidbody.velocity.y, worldDirection.z);
            }
            else
            {
                _rigidbody.velocity = new Vector3(worldDirection.x, _rigidbody.velocity.y, worldDirection.z);
            }
        }
        else
        {
            if (!IsGrounded && HasJumpInertia)
            {
                var prev = _rigidbody.velocity;
                _rigidbody.velocity = JumpInertia;
                JumpInertia = prev;
            }
        }

        _rigidbodyvelocity = _rigidbody.velocity;
    }
    public Vector3 _rigidbodyvelocity;
    public Vector3 worldDirection;

    bool CheckIsGrounded()
    {
        Vector3 start = transform.position + Vector3.up * (_collider.height / 2 - _collider.radius); // 上部
        Vector3 end = transform.position - Vector3.up * (_collider.height / 2 - _collider.radius); // 下部
        float radius = _collider.radius - 0.1f;

        // 指定した方向にCapsuleCastを実行
        IsGrounded = Physics.CapsuleCast(start, end, radius, Vector3.down, out RaycastHit hit, 0.2f) && hit.point.y < transform.position.y;

        if(IsGrounded)
        {
            JumpInertia = Vector3.zero;
            HasJumpInertia = false;
        }
        // var isFootRayGrounded = Physics.CapsuleCast(transform.position, Vector3.down, _distToGround + 0.1f);
        return IsGrounded;
    }
}
