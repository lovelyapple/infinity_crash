using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMainObject : MonoSingletoneBase<GameMainObject>
{
    public GameCharaConttroller GameCharaController;
    Coroutine _startGameCoroutine;
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

        var countDown = 3f;

        while(countDown > 0)
        {
            countDown -= Time.deltaTime;
            hudController.CountDownController.CountDownLabel.text = ((int)(countDown + 1)).ToString();

            yield return null;
        }

        GameModel.Instance.StartGame();
        _startGameCoroutine = null;
    }
}
