using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameCharaConttroller : MonoBehaviour
{
    public GameSettings CurGameSettings;
    public Camera mainCamera;
    public float maxDistance = 500f; // レイキャストの最大距離
    public Transform CameraTransform;
    Rigidbody _rigidbody;
    SphereCollider _collider;
    float _distToGround;

    private float xRotation = 0f;

    public Vector3 InputMoveSpeed;
    public bool IsInputing;

    public bool IsGrounded = false;
    public bool CanJump = false;
    public bool CanWallJump = false;
    public bool HasJumpInertia = false;
    public Vector3 JumpInertia;
    public List<SkillBase> Skills = new List<SkillBase>();
    public bool HasSpeedRunSKill => Skills.Any(x => x.SkillType == SkillType.SpeedRun);
    public int SuperJumpCount => Skills.Count(x => x.SkillType == SkillType.SuperJump);
    public int RaycastMask;
    public Action<List<SkillBase>> OnUpdateSkills;
    public bool CanMove = false;
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();
        _distToGround = _collider.bounds.extents.y;

        RaycastMask = 1 << LayerMask.NameToLayer("FieldIcon");

        GameModel.Instance.OnStartGame += OnGameStart;
        GameModel.Instance.OnFinished += OnGameFinished;
        GameModel.Instance.OnGotoTitle += OnGotoTitle;
    }
    void OnDestory()
    {
        GameModel.Instance.OnStartGame -= OnGameStart;
        GameModel.Instance.OnFinished -= OnGameFinished;
        GameModel.Instance.OnGotoTitle -= OnGotoTitle;
    }
    private void OnGameStart()
    {
        Cursor.lockState = CursorLockMode.Locked;
        CanMove = true;
    }
    private void OnGameFinished()
    {
        Cursor.lockState = CursorLockMode.None;

        var skills = Skills.ToList();
        foreach (var skill in skills)
        {
            skill.OnSkillFinished();
        }
        CanMove = false;
    }
    private void OnGotoTitle()
    {
        Cursor.lockState = CursorLockMode.None;
        CanMove = false;
    }
    // Update is called once per frame
    void Update()
    {
        // マウスのX軸とY軸の入力を取得
        var mouseSensitivity = CurGameSettings.CurCharacterSettings.MouseSensitivity;
        var jumpForce  = CurGameSettings.CurCharacterSettings.JumpForce;
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

        if (CanMove)
        {
            if (CanJump)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (SuperJumpCount > 0 && Input.GetKeyDown(KeyCode.Space))
                    {
                        var skill = Skills.FirstOrDefault(x => x.SkillType == SkillType.SuperJump);
                        skill.OnSkillFire();
                        Jump(SkillSuperJump.SuperJumpPower);
                    }
                }
                else if (Input.GetButtonDown("Jump"))
                {
                    Jump(jumpForce);

                }


                if (AutoJump)
                {
                    if (JumpTimeLeft > 0)
                    {
                        JumpTimeLeft -= Time.deltaTime;

                        if (JumpTimeLeft <= 0)
                        {
                            JumpTimeLeft = 2;
                            Jump(jumpForce);
                        }
                    }
                }
            }
            else if (CanWallJump)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    Jump(jumpForce);
                }
            }
        }

        // や、今考えない
        if(IsHoldingWall)
        {

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

        // 右クリックを検出
        if (Input.GetMouseButtonDown(1))
        {
            // カメラの中央からレイを飛ばす
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            // レイが何かに当たった場合
            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                // ヒット位置と球体の中心のベクトルを計算
                Vector3 hitDirection = (hit.point - _collider.transform.position).normalized;

                // 球体の中心からコライダーの半径分だけ外側に移動した位置を計算
                Vector3 targetPosition = hit.point + hitDirection * _collider.radius;

                // オブジェクトを新しい位置に移動
                transform.position = targetPosition;
            }
        }
    }
    public float JumpTimeLeft = 3;
    public bool AutoJump = false;
    public bool IsJumped = false;
    public Slider WallPackPowerSlider;
    public float WallJumpNeedPower;
    public bool IsHoldingWall;
    public float WallPackPowerRecover = 20f;
    private void Jump(float jumpForce)
    {
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y + jumpForce, _rigidbody.velocity.z);
        JumpInertia = _rigidbody.velocity;

        IsJumped = true;
    }

    void FixedUpdate()
    {
        IsInputing = false;
        var inputForce = CurGameSettings.CurCharacterSettings.InputForceVelocity;
        if (Input.GetKey(KeyCode.W))
        {
            InputMoveSpeed.z = inputForce * Time.deltaTime;
            IsInputing = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            InputMoveSpeed.z = -inputForce * Time.deltaTime;
            IsInputing = true;
        }
        else
        {
            InputMoveSpeed.z = 0;
        }

        if (Input.GetKey(KeyCode.A))
        {
            InputMoveSpeed.x = -inputForce * Time.deltaTime;
            IsInputing = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {

            InputMoveSpeed.x = inputForce * Time.deltaTime;
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
                // _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z); // 滑らないように速度をゼロにする
                _rigidbody.useGravity = false; // 重力をオフにする
            }
        }else
        {
            _rigidbody.useGravity = true;
        }

        if (CanMove && (InputMoveSpeed.x != 0 || InputMoveSpeed.z != 0))
        {
            velocity = _rigidbody.velocity;
            var inputCache = InputMoveSpeed;
            inputCache.y = 0;

            if (HasSpeedRunSKill)
            {
                inputCache *= 2f;
            }

            _rigidbody.AddRelativeForce(inputCache, ForceMode.Force);

            curVelocityPower = velocity.x * velocity.x + velocity.z * velocity.z;

            var worldDorcetion = transform.TransformDirection(InputMoveSpeed);
  
            if(InputMoveSpeed.x == 0)
            {
                var localDirection = transform.InverseTransformDirection(velocity);
                if (localDirection.x > 0)
                {
                    localDirection.x -= ReFriction * Time.deltaTime;
                }
                else if(localDirection.x < 0)
                {
                    localDirection.x += ReFriction * Time.deltaTime;
                }
                velocity = transform.TransformDirection(localDirection);
                _rigidbody.velocity = velocity;
            }
            else
            {
                var localDirection = transform.InverseTransformDirection(velocity);
                if (InputMoveSpeed.x > 0  && localDirection.x < 0)
                {
                    localDirection.x += ReFriction * Time.deltaTime;
                    velocity = transform.TransformDirection(localDirection);
                    _rigidbody.velocity = velocity;
                }
                else if (InputMoveSpeed.x < 0 && localDirection.x > 0)
                {
                    localDirection.x -= ReFriction * Time.deltaTime;
                    velocity = transform.TransformDirection(localDirection);
                    _rigidbody.velocity = velocity;
                }
            }
            if (InputMoveSpeed.z == 0)
            {
                var localDirection = transform.InverseTransformDirection(velocity);
                if (localDirection.z > 0)
                {
                    localDirection.z -= ReFriction * Time.deltaTime;
                }
                else if (localDirection.z < 0)
                {
                    localDirection.z += ReFriction * Time.deltaTime;
                }
                velocity = transform.TransformDirection(localDirection);
            }
            else
            {
                var localDirection = transform.InverseTransformDirection(velocity);
                if (InputMoveSpeed.z > 0 && localDirection.z < 0)
                {
                    localDirection.z += ReFriction * Time.deltaTime;
                    velocity = transform.TransformDirection(localDirection);
                    _rigidbody.velocity = velocity;
                }
                else if (InputMoveSpeed.z < 0 && localDirection.z > 0)
                {
                    localDirection.z -= ReFriction * Time.deltaTime;
                    velocity = transform.TransformDirection(localDirection);
                    _rigidbody.velocity = velocity;
                }
            }

            var maxPower = 0f;
            if (HasSpeedRunSKill)
            {
                maxPower = CurGameSettings.CurCharacterSettings.MaxSpeedNormal * CurGameSettings.CurCharacterSettings.MaxSpeedNormal;
            }
            else
            {
                maxPower = CurGameSettings.CurCharacterSettings.MaxSpeedBuffed * CurGameSettings.CurCharacterSettings.MaxSpeedBuffed;
            }

            if (maxPower < curVelocityPower)
            {
                velocity.x = maxPower / curVelocityPower * velocity.x;
                velocity.z = maxPower / curVelocityPower * velocity.z;
                _rigidbody.velocity = velocity;
            }

            curVelocityPower = Mathf.Sqrt(curVelocityPower);

        }
        else if(IsGrounded)
        {
            if(IsJumped)
            {
                IsJumped = false;
                return;
            }

            _rigidbody.velocity *= 0.85f;
        }

        velocityLocal = transform.InverseTransformDirection(_rigidbody.velocity);
    }
    public Vector3 velocity;
    public Vector3 velocityLocal;
    public float curVelocityPower;
    
    public float ReFriction = 1f;
    public float maxSlopeAngle = 30;

    bool CheckIsGrounded()
    {
        float radius = _collider.radius - 0.1f;

        // 指定した方向にCapsuleCastを実行
        IsGrounded = Physics.SphereCast(transform.position, radius, Vector3.down, out var hit, 0.2f) && hit.point.y < transform.position.y;

        var hits = Physics.SphereCastAll(transform.position, _collider.radius, Vector3.down, 0.4f);
        CanJump = IsGrounded || hits.Any(x => x.transform != null && x.transform.gameObject.tag != "Player" && x.point.y < transform.position.y);
        CanWallJump = IsGrounded || hits.Any(x =>x.transform != null && x.transform.gameObject.tag != "Player" && x.point.y < transform.position.y);
        CanWallJump = false;

        if (IsGrounded)
        {
            JumpInertia = Vector3.zero;
            HasJumpInertia = false;
        }

        return IsGrounded;
    }
    public void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "WaterField")
        {
            Jump(15);

            GameModel.Instance.DecreaseTime();
            ResourceManager.Instance.TurnOnEffect("Fx_WaterJump");
        }
    }
    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "DangerApplyIcon")
        {
            var FieldAppIcon = collider.gameObject.GetComponent<FieldAppIcon>();

            FieldAppIcon.RequestTouch();
            ResourceManager.Instance.TurnOnEffect("Fx_up");
        }
        else if (collider.gameObject.tag == "JumpBoard")
        {
            var jumpPower = collider.gameObject.GetComponent<JumpBoard>().JumpPower;
            Jump(jumpPower);
        }
        else if (collider.gameObject.tag == "FieldSkill")
        {
            var skillObj = collider.gameObject.GetComponent<FieldSkill>();
            ResourceManager.Instance.TurnOnEffect("Fx_up");
            skillObj.RequestTouch();

            var settingData = GameSkillChooser.Instance.GetOneSkillData();

            if(settingData == null)
            {
                return;
            }

            var skill = GameSkillChooser.CreateSkill(settingData);
            // skill.Initialize(settingData, this, this.gameObject.transform);
            if (AddSkillPool(skill))
            {
                GameHUDController.Instance.UpdateSkills(Skills);
            }
        }
        else if(collider.gameObject.tag == "ScorePoint")
        {
            var scorePoint = collider.gameObject.GetComponent<ScorePoint>();
            scorePoint.OnTouched();
            ApplicationPressureManager.Instance.AddPressure(scorePoint.ScoreType);
            UITaskGetFxSpawner.CreateOne(scorePoint.ScoreType);
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
        GameHUDController.Instance.UpdateSkills(Skills);
    }
}
