using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameCharaConttroller : MonoBehaviour
{
    public Transform CameraTransform;
    Rigidbody _rigidbody;
    SphereCollider _collider;
    float _distToGround;
    public float JumpForce = 5f;
    public float mouseSensitivity = 100f;  // マウス感度
    private float xRotation = 0f;

    public Vector3 InputMoveSpeed;
    public float InputForce = 1f;
    public bool IsInputing;

    public bool IsGrounded = false;
    public bool HasJumpInertia = false;
    public Vector3 JumpInertia;

    public float SpeedBuffMaxSpeed = 0;
    public List<SkillBase> Skills = new List<SkillBase>();
    public bool HasSpeedRunSKill => Skills.Any(x => x.SkillType == SkillType.SpeedRun);
    public int RaycastMask;
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();
        _distToGround = _collider.bounds.extents.y;

        // カーソルを画面中央に固定し、カーソルの表示を無効化
        Cursor.lockState = CursorLockMode.Locked;
        RaycastMask = 1 << LayerMask.NameToLayer("FieldIcon");
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
                Jump();
            }

            if (AutoJump)
            {
                if (JumpTimeLeft > 0)
                {
                    JumpTimeLeft -= Time.deltaTime;

                    if (JumpTimeLeft <= 0)
                    {
                        JumpTimeLeft = 2;
                        Jump();
                    }
                }
            }
        }

        foreach (var skill in Skills)
        {
            skill.OnSkillpdate();
        }

        var finshedList = Skills.Where(x => x.IsFinished).ToList();
        foreach (var skill in finshedList)
        {
            skill.OnSkillFinished();
        }
    }
    public float JumpTimeLeft = 3;
    public bool AutoJump = false;
    private void Jump()
    {
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y + JumpForce, _rigidbody.velocity.z);
        JumpInertia = _rigidbody.velocity;
    }

    void FixedUpdate()
    {
        IsInputing = false;
        if (Input.GetKey(KeyCode.W))
        {
            InputMoveSpeed.z = InputForce * Time.deltaTime;
            IsInputing = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            InputMoveSpeed.z = -InputForce * Time.deltaTime;
            IsInputing = true;
        }
        else
        {
            InputMoveSpeed.z = 0;
        }

        if (Input.GetKey(KeyCode.A))
        {
            InputMoveSpeed.x = -InputForce * Time.deltaTime;
            IsInputing = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {

            InputMoveSpeed.x = InputForce * Time.deltaTime;
            IsInputing = true;
        }
        else
        {
            InputMoveSpeed.x = 0;
        }

        if (IsInputing)
        {
            InputMoveSpeed.y = 0;
        }
        else
        {
            InputMoveSpeed = Vector3.zero;
        }

        RaycastHit hit;
        // オブジェクトの真下にRaycastを撃って地面の法線を取得
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.52f))
        {
            Vector3 groundNormal = hit.normal;
            // 法線ベクトルと上向きベクトルの角度を計算
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

            // 傾斜角が設定した最大角度を超えた場合のみ滑るようにする
            if (slopeAngle > maxSlopeAngle)
            {
                _rigidbody.useGravity = true;
            }
            else if(_rigidbody.velocity.y < 0)
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z); // 滑らないように速度をゼロにする
                _rigidbody.useGravity = false; // 重力をオフにする
            }
        }else
        {
            _rigidbody.useGravity = true;
        }

        if (InputMoveSpeed.x != 0 || InputMoveSpeed.z != 0)
        {
            velocity = _rigidbody.velocity;
            var inputCache = InputMoveSpeed;
            inputCache.y = 0;
            _rigidbody.AddRelativeForce(inputCache, ForceMode.Force);
            speed = Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
        }
    }
    public Vector3 velocity;
    public float speed;
    public float maxSlopeAngle = 30;

    bool CheckIsGrounded()
    {
        float radius = _collider.radius - 0.1f;

        // 指定した方向にCapsuleCastを実行
        IsGrounded = Physics.SphereCast(transform.position, radius, Vector3.down, out var hit, 0.2f) && hit.point.y < transform.position.y;

        if (IsGrounded)
        {
            JumpInertia = Vector3.zero;
            HasJumpInertia = false;
        }

        return IsGrounded;
    }
    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "DangerApplyIcon")
        {
            var FieldAppIcon = collider.gameObject.GetComponent<FieldAppIcon>();
            var settingData = FieldAppIcon.SkillSettingData;

            var skill = SkillCreator.CreateSkill(settingData);
            skill.Initialize(settingData, this, collider.transform);

            AddSkillPool(skill);
            Destroy(collider.gameObject);
        }
    }

    public bool AddSkillPool(SkillBase skillBase)
    {
        if (Skills.Any(x => x.SkillType == skillBase.SkillType
            && !x.CanStack))
        {
            return false;
        }
        Skills.Add(skillBase);

        if (skillBase.IsAutoFire)
        {
            FireSkill(skillBase.SkillType);
        }

        return true;
    }
    public void FireSkill(SkillType skillType)
    {
        var skill = Skills.FirstOrDefault(x => x.SkillType == skillType);

        if (skill != null)
        {
            skill.OnSkillFire();
            if (skill.TimeType == SkillTimeType.OneShot && skill.IsFinished)
            {
                skill.OnSkillFinished();
            }
        }
    }
    public void OnRemoveSkill(SkillBase skill)
    {
        Skills.Remove(skill);
    }
}
