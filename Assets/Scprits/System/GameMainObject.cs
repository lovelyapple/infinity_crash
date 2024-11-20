using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameMainObject : MonoSingletoneBase<GameMainObject>
{
    public static bool IsPausing;
    [SerializeField] private PostProcessVolume PostProcessVolume;
    private DepthOfField _depthOfField;
    public DepthOfField DepthOfField { get
        {
            if (_depthOfField == null)
            {
                PostProcessVolume.profile.TryGetSettings(out _depthOfField);
            }
            return _depthOfField;
        }
    }
    public GameCharaConttrollerNew GameCharaController;
    Coroutine _startGameCoroutine;
    public void Start()
    {
        Application.targetFrameRate = 60;
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            UIPauseController.Instance.gameObject.SetActive(true);
            IsPausing = true;
        }
    }
    public void RequestStartGame()
    {
        if(_startGameCoroutine != null)
        {
            return;
        }

        _startGameCoroutine = StartCoroutine(IERequestStartGame());
    }
    public IEnumerator IERequestStartGame()
    {
        var hudController = GameHUDController.Instance;
        hudController.OnCountDownStart();
        Cursor.lockState = CursorLockMode.Locked;
        SoundManager.Instance.PauseBGM(true);

        var countDown = 3f;
        var prevTest = hudController.CountDownController.CountDownLabel.text;
        GameCharaConttrollerNew.CanRotate = true;

        while (countDown > 0)
        {
            countDown -= Time.deltaTime;
            hudController.CountDownController.CountDownLabel.text = ((int)(countDown + 1)).ToString();

            if(prevTest != hudController.CountDownController.CountDownLabel.text)
            {
                
            }

            yield return null;
        }

        GameModel.Instance.StartGame();
        _startGameCoroutine = null;
        SoundManager.Instance.PauseBGM(false);
    }
    public void DoFOn()
    {
        DepthOfField.focalLength.value = 300f;
    }
    public void DoFOff()
    {
        DepthOfField.focalLength.value = 0;
    }
}
