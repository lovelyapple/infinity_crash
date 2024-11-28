using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameCharaConttrollerNew : MonoBehaviour
{
    [SerializeField] Animator CameraJumpShakeAnimator;
    private const int MAX_WATER_JUMP_RETURN_CHANCE = 2;
    private int _waterJumpReturnChanceLeft;
    [SerializeField] AnimatorAutoTurnOff TransportAuto;
    private FieldSkillSpawner _lastSkillSpanwer;
    public GameSettings CurGameSettings;
    public Camera mainCamera;
    public float maxDistance = 500f; // レイキャストの最大距離
    public Transform CameraTransform;
    SphereCollider _collider;

    private float xRotation = 0f;


    public bool IsInputing => InputtingVector.x != 0 || InputtingVector.z != 0;

    public bool CanJump = false;
    public bool CanWallJump = false;
    public List<SkillBase> Skills = new List<SkillBase>();
    public bool HasSpeedRunSKill => Skills.Any(x => x.SkillType == SkillType.SpeedRun);
    public int SuperJumpCount => Skills.Count(x => x.SkillType == SkillType.SuperJump);
    public int RaycastMask;
    public float JumpTimeLeft = 3;
    public bool AutoJump = false;
    public bool IsJumped = false;
    public bool CanMove = false;
    public static bool CanRotate = false;
    void Awake()
    {
        _collider = GetComponent<SphereCollider>();

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
        CanRotate = true;
        _waterJumpReturnChanceLeft = MAX_WATER_JUMP_RETURN_CHANCE;
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
        CanRotate = false;
    }
    private void OnGotoTitle()
    {
        Cursor.lockState = CursorLockMode.None;
        CanMove = false;
        CanRotate = false;
    }
    public Vector3Int InputtingVector;
    public Vector3 InputMoveSpeedLocal;
    public Vector3 InputMoveSpeedWorld;
    public float REVERSE_INPUT_FRICTION = 0.98f;
    public float REVERSE_INPUT_AIR_FRICTION = 0.98f;
    public float MEAN_INPUT_POWER = 0.00001f;
    public float GROUND_TOUCH_SE_PLAY_CONDITION_INTERVAL = 0.18f;
    public GroundTouchState CurrentGroundTouchState;
    public float DirectDownSpeed;
    public float FootGroundAngle;
    public float FloatingTime;


    private int frameCount = 0;
    private float elapsedTime = 0.0f;
    private float displayInterval = 0.5f; // FPSを更新する間隔（秒）
    int fps = 0;
    // Update is called once per frame
    void Update()
    {

        frameCount++;
        elapsedTime += Time.deltaTime;


        if (elapsedTime >= displayInterval)
        {
            fps = (int)(frameCount * (1f / elapsedTime));

            frameCount = 0;
            elapsedTime = 0.0f;
        }

        if (CanRotate)
        {
            // マウスのX軸とY軸の入力を取得
            var mouseSensitivity = CurGameSettings.CurCharacterSettings.MouseSensitivity;
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
        }

        if(!CanMove)
        {
            return;
        }

        var halfSize = _collider.radius - 0.1f;
        var jumpForce = CurGameSettings.CurCharacterSettings.JumpForce;
        var inputForce = CurGameSettings.CurCharacterSettings.InputForceVelocity * Time.deltaTime;
        var maxInputSpeedThisFrame = CurGameSettings.CurCharacterSettings.MaxSpeedNormal * Time.deltaTime;
        var maxBuddedSpeedThisFrame = CurGameSettings.CurCharacterSettings.MaxSpeedBuffed * Time.deltaTime;

        var friction = CurrentGroundTouchState != GroundTouchState.Floating ? REVERSE_INPUT_FRICTION : REVERSE_INPUT_AIR_FRICTION;

        GameHUDController.Instance.DebugText.text = $"{CurGameSettings.CurCharacterSettings.MaxSpeedNormal} - {fps} - {maxInputSpeedThisFrame.ToString("0.00000000")}";



        if (Input.GetKey(KeyCode.W))
        {
            InputtingVector.z = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            InputtingVector.z = -1;
        }
        else
        {
            InputtingVector.z = 0;
        }

        if (Input.GetKey(KeyCode.A))
        {
            InputtingVector.x = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            InputtingVector.x = 1;
        }
        else
        {
            InputtingVector.x = 0;
        }

        // 重力計算
        var prevState = CurrentGroundTouchState;
        DirectDownSpeed = Mathf.Min(CurGameSettings.CurCharacterSettings.MaxGravitySpeed,
        DirectDownSpeed + CurGameSettings.CurCharacterSettings.GravitySpeed * Time.deltaTime);
        var groundTag = "";

        var fall = CollisionUtility.FallCheck(Vector3.down * DirectDownSpeed * Time.deltaTime,
        transform.position, halfSize,
        CurGameSettings.CurCharacterSettings.FallFootSlideDegree,
        out FootGroundAngle, out CurrentGroundTouchState, out groundTag);

        // 地面にタッチした瞬間だけ、重力加速度を0にする（疑似反発力）
        if ((prevState == GroundTouchState.Floating && prevState != CurrentGroundTouchState) || 
            (prevState == GroundTouchState.Stationary && prevState == CurrentGroundTouchState))
        {
            DirectDownSpeed = 0;
        }

        if (prevState == GroundTouchState.Floating && prevState != CurrentGroundTouchState)
        {
            if(FloatingTime > GROUND_TOUCH_SE_PLAY_CONDITION_INTERVAL && !string.IsNullOrEmpty(groundTag) && groundTag != "WaterField")
            {
                SoundManager.Instance.PlayOneShot(OneShotSeName.Fall_Touching_Ground);

                CameraJumpShakeAnimator.SetTrigger("touch_ground");
                _waterJumpReturnChanceLeft = MAX_WATER_JUMP_RETURN_CHANCE;
            }
        }

        if (CurrentGroundTouchState == GroundTouchState.Floating)
        {
            FloatingTime += Time.deltaTime;
        }
        else
        {
            FloatingTime = 0;
        }

        CanJump = FloatingTime <= CurGameSettings.CurCharacterSettings.FloatingJumpTimeLimie;
        transform.position += fall;

        // InputMoveSpeedLocal
        if (IsInputing)
        {
            InputMoveSpeedLocal = transform.InverseTransformDirection(InputMoveSpeedWorld);
            if (InputtingVector.z != 0)
            {
                if (InputtingVector.z > 0)
                {
                    if (InputMoveSpeedLocal.z < 0)
                    {
                        InputMoveSpeedLocal.z *= friction;

                        if (InputMoveSpeedLocal.z > -MEAN_INPUT_POWER)
                        {
                            InputMoveSpeedLocal.z = 0;
                        }
                    }

                    InputMoveSpeedLocal.z += inputForce;
                }
                else
                {
                    if (InputMoveSpeedLocal.z > 0)
                    {
                        InputMoveSpeedLocal.z *= friction;

                        if (InputMoveSpeedLocal.z < MEAN_INPUT_POWER)
                        {
                            InputMoveSpeedLocal.z = 0;
                        }
                    }

                    InputMoveSpeedLocal.z -= inputForce;
                }
            }
            else
            {
                InputMoveSpeedLocal.z *= friction;

                if (InputMoveSpeedLocal.z * InputMoveSpeedLocal.z < MEAN_INPUT_POWER * MEAN_INPUT_POWER)
                {
                    InputMoveSpeedLocal.z = 0;
                }
            }

            if (InputtingVector.x != 0)
            {
                if (InputtingVector.x > 0)
                {
                    if (InputMoveSpeedLocal.x < 0)
                    {
                        InputMoveSpeedLocal.x *= friction;

                        if (InputMoveSpeedLocal.x > -MEAN_INPUT_POWER)
                        {
                            InputMoveSpeedLocal.x = 0;
                        }
                    }

                    InputMoveSpeedLocal.x += inputForce;
                }
                else
                {
                    if (InputMoveSpeedLocal.x > 0)
                    {
                        InputMoveSpeedLocal.x *= friction;

                        if (InputMoveSpeedLocal.x < MEAN_INPUT_POWER)
                        {
                            InputMoveSpeedLocal.x = 0;
                        }
                    }

                    InputMoveSpeedLocal.x -= inputForce;
                }
            }
            else
            {
                InputMoveSpeedLocal.x *= friction;

                if (InputMoveSpeedLocal.x * InputMoveSpeedLocal.x < MEAN_INPUT_POWER * MEAN_INPUT_POWER)
                {
                    InputMoveSpeedLocal.x = 0;
                }
            }
        }
        else
        {
            if (InputMoveSpeedLocal.x != 0 || InputMoveSpeedLocal.z != 0)
            {
                InputMoveSpeedLocal *= friction;

                if(InputMoveSpeedLocal.sqrMagnitude <= MEAN_INPUT_POWER * MEAN_INPUT_POWER)
                {
                    InputMoveSpeedLocal = Vector3.zero;
                }
            }
        }

        // Input加速度の最大限を求める
        if (InputMoveSpeedLocal.x != 0 || InputMoveSpeedLocal.z != 0)
        {
            InputMoveSpeedLocal.y = 0;
            CurrentInputSpeed = InputMoveSpeedLocal.sqrMagnitude;
            var maxSpeed = HasSpeedRunSKill ?  maxBuddedSpeedThisFrame : maxInputSpeedThisFrame;
            var powerScale = maxSpeed / CurrentInputSpeed;

            if (powerScale < 1)
            {
                InputMoveSpeedLocal.x *= powerScale;
                InputMoveSpeedLocal.z *= powerScale;
            }

            InputMoveSpeedWorld = transform.TransformDirection(InputMoveSpeedLocal);
            InputMoveSpeedWorld = CollisionUtility.MoveCheck(InputMoveSpeedWorld, transform.position, halfSize);
        }
        else
        {
            if (InputMoveSpeedWorld.x != 0 || InputMoveSpeedLocal.z != 0)
            {
                InputMoveSpeedWorld *= friction;

                if (InputMoveSpeedWorld.sqrMagnitude <= MEAN_INPUT_POWER * MEAN_INPUT_POWER)
                {
                    InputMoveSpeedWorld = Vector3.zero;
                }
            }
        }

        if (InputMoveSpeedWorld.x != 0 || InputMoveSpeedLocal.z != 0)
        {
            transform.position += InputMoveSpeedWorld;
        }


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
                        SoundManager.Instance.PlayOneShot(OneShotSeName.Super_jump);
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
    public float CurrentInputSpeed;
    void FixedUpdate()
    {

    }
    public void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "WaterField")
        {
            if (_waterJumpReturnChanceLeft > 0)
            {
                Jump(15);

                GameModel.Instance.DecreaseTime();
                ResourceManager.Instance.TurnOnEffect("Fx_WaterJump");
                _waterJumpReturnChanceLeft--;
            }
            else
            {
                GameModel.Instance.DecreaseTime();
                ResourceManager.Instance.TurnOnEffect("Fx_WaterJump");
                _waterJumpReturnChanceLeft = MAX_WATER_JUMP_RETURN_CHANCE;
                
                if(_lastSkillSpanwer == null)
                {
                    _lastSkillSpanwer = FieldObjectListController.Instance.FieldSkillSpawners.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
                }

                TransportAuto.OnFinsihed = () => { CanMove = true; };
                _lastSkillSpanwer.HoldingObject.RequestTouch();
                transform.position = _lastSkillSpanwer.HoldingObject.transform.position;
                ResourceManager.Instance.TurnOnEffect("Fx_transport");
                CanMove = false;
            }
        }
    }
    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "DangerApplyIcon")
        {
            ResourceManager.Instance.TurnOnEffect("Fx_up");
            var FieldAppIcon = collider.gameObject.GetComponent<FieldAppIcon>();

            FieldAppIcon.RequestTouch();
        }
        else if (collider.gameObject.tag == "JumpBoard")
        {
            var jumpPower = collider.gameObject.GetComponent<JumpBoard>().JumpPower;
            SoundManager.Instance.PlayOneShot(OneShotSeName.Super_jump);
            Jump(jumpPower);
        }
        else if (collider.gameObject.tag == "FieldSkill")
        {
            ResourceManager.Instance.TurnOnEffect("Fx_up");
            SoundManager.Instance.PlayOneShot(OneShotSeName.Skill_Get);
            _lastSkillSpanwer = collider.transform.parent.gameObject.GetComponent<FieldSkillSpawner>();
            var skillObj = collider.gameObject.GetComponent<FieldSkill>();
            skillObj.RequestTouch();

            var settingData = GameSkillChooser.Instance.GetOneSkillData();

            if(settingData == null)
            {
                return;
            }

            var skill = GameSkillChooser.CreateSkill(settingData);
            skill.Initialize(settingData, this, this.gameObject.transform);
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

    public bool IsCheckTouching;
    // bool CheckIsGrounded()
    // {
    //     float radius = _collider.radius - 0.1f;

    //     // 指定した方向にCapsuleCastを実行
    //     IsCheckTouching = Physics.SphereCast(transform.position, radius, Vector3.down, out var hit, 0.2f) && hit.point.y < transform.position.y;

    //     var hits = Physics.SphereCastAll(transform.position, _collider.radius, Vector3.down, 0.4f);
    //     IsCheckTouching = IsCheckTouching || hits.Any(x => x.transform != null && x.transform.gameObject.tag != "Player" && x.point.y < transform.position.y);
    //     // CanWallJump = IsGrounded || hits.Any(x => x.transform != null && x.transform.gameObject.tag != "Player" && x.point.y < transform.position.y);
    //     // CanWallJump = false;

    //     if (IsCheckTouching)
    //     {
    //         // JumpInertia = Vector3.zero;
    //         // HasJumpInertia = false;
    //     }

    //     return IsCheckTouching;
    // }
    private void Jump(float jumpForce)
    {
        IsJumped = true;
        DirectDownSpeed = -jumpForce;
    }
}
