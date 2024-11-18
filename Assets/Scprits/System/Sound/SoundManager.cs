using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum OneShotSeName
{
    TaskGet,
    Time_Recover,
    Touch_Water,
    Speed_Buff,
    Super_jump,
    Explo,
    Skill_Get,
    App_Crashing,
    Gmae_Start,
    Fall_Touching_Ground,
    App_Recover,
}
public class SoundManager : MonoSingletoneBase<SoundManager>
{
    [SerializeField] AudioClip BGMInGame;
    [SerializeField] AudioClip BGMResult;
    [SerializeField] AudioSource BGMPlayer;
    [SerializeField] List<SoundOneShotSeController> SoundPrefabs;
    [SerializeField] Transform PlayingTran;
    [SerializeField] Transform PoolTran;
    public List<SoundOneShotSeController> SePoolControllers = new List<SoundOneShotSeController>();
    public List<SoundOneShotSeController> SeRequestPoolControllers = new List<SoundOneShotSeController>();
    public List<SoundOneShotSeController> PlayingPoolControllers = new List<SoundOneShotSeController>();

    public void Update()
    {
        if(!SeRequestPoolControllers.Any())
        {
            return;
        }

        foreach(var ct in SeRequestPoolControllers)
        {
            ct.transform.SetParent(PoolTran);
            SePoolControllers.Add(ct);
        }

        SeRequestPoolControllers.Clear();
    }
    public void PauseBGM(bool pause)
    {
        if(pause)
        {
            BGMPlayer.Pause();
        }
        else
        {
            BGMPlayer.UnPause();
        }
    }
    public void PlayInGameBGM()
    {
        BGMPlayer.clip = BGMInGame;
        BGMPlayer.Play();
    }
    public void PlayResultBGM()
    {
        BGMPlayer.clip = BGMResult;
        BGMPlayer.Play();
    }
    public void PlayOneShot(OneShotSeName name)
    {
        var oneShot = SePoolControllers.FirstOrDefault(x => x.OneShotSeName == name);

        if(oneShot == null)
        {
            oneShot = Instantiate<SoundOneShotSeController>(SoundPrefabs.FirstOrDefault(x => x.OneShotSeName == name));
            oneShot.transform.SetParent(PlayingTran);
            oneShot.OnPlayEnd = (ctl) =>
            {
                ctl.gameObject.SetActive(false);
                SeRequestPoolControllers.Add(ctl);
            };
        }
        else
        {
            SePoolControllers.Remove(oneShot);
        }

        StartCoroutine(RequestPlay(oneShot));
    }
    IEnumerator RequestPlay(SoundOneShotSeController ctrl)
    {
        PlayingPoolControllers.Add(ctrl);
        ctrl.transform.SetParent(PlayingTran);

        yield return null;

        ctrl.Play();
    }
    private float _BGMPitchTarget;
    Coroutine _bgmSpeedCoroutine;
    public void RequestAddBGMSpeed(float duration)
    {
        if(_bgmSpeedCoroutine != null)
        {
            StopCoroutine(_bgmSpeedCoroutine);
            BGMPlayer.pitch = _BGMPitchTarget;
        }

        _bgmSpeedCoroutine = StartCoroutine(ChangeSpeedBGMIe(1.2f, duration));
    }
    public void RequestNormalBGMSpeed(float duration)
    {
        if (_bgmSpeedCoroutine != null)
        {
            StopCoroutine(_bgmSpeedCoroutine);
            BGMPlayer.pitch = _BGMPitchTarget;
        }

        _bgmSpeedCoroutine = StartCoroutine(ChangeSpeedBGMIe(1f, duration));
    }
    public IEnumerator ChangeSpeedBGMIe(float target, float duration)
    {
        _BGMPitchTarget = target;
        var prev = BGMPlayer.pitch;
        var diff = prev - _BGMPitchTarget;

        var elapseTime = 0f;
        while (elapseTime < duration)
        {
            BGMPlayer.pitch = prev - diff * (elapseTime / duration);
            elapseTime += Time.deltaTime;
            yield return null;
        }

        BGMPlayer.pitch = _BGMPitchTarget;
        _bgmSpeedCoroutine = null;
    }
}
