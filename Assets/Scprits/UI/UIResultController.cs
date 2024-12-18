using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIResultController : MonoBehaviour
{
    public enum ResultType
    {
        Newbie = 60,
        Normal = 90,
        Leader = 130,
        Cheaf = 160,
        SpecialList = 180,
    }
    [SerializeField] List<UIResultScore> ScoreLabels;
    [SerializeField] UIResultScore FinalScoreLabel;
    [SerializeField] TextMeshProUGUI FinalScoreTitleLabel;
    [SerializeField] TextMeshProUGUI FinalScoreDiscriptionLabel;
    [SerializeField] GameObject GotoTitleObj;
    [SerializeField] TextMeshProUGUI BonustTimeCountLabel;
    [SerializeField] AudioSource CountSe;
    [SerializeField] AudioSource CountStopSe;
    [SerializeField] AudioSource FinalResultPunchSe;
    [SerializeField] UIResultCrashedApp ResultCrashedApp;
    private readonly Dictionary<ResultType, (string, string)> ResultDisplayDict = new Dictionary<ResultType, (string, string)>
    {
        {ResultType.Newbie , ("Newbie ","Getting started!")},
        {ResultType.Normal, ("Normal", " Nice performance, keep it up!")},
        {ResultType.Leader, ("Leader", "Impressive !!")},
        {ResultType.Cheaf, ("Cheaf", "Great job, you're a master!")},
        {ResultType.SpecialList, ("Specialist", "Outstanding! You're a truly elite!")},
    };
    public float ScoreSinglePerformTime = 1.5f;
    public bool TappedSkip = false;
    Coroutine _coroutine;
    public void SetupScore()
    {
        TappedSkip = false;
        var scoreInfos = ApplicationPressureManager.Instance.ScoreInfos.OrderBy(x => x.ScoreType).ToList();
        SoundManager.Instance.PlayResultBGM();
        var result = GameModel.Instance.CurrentGameResult;

        if(result.ResultType != GameModel.GameResultType.Timeout)
        {
            ResultCrashedApp.Setup(result.CrashedAppType);
        }
        else
        {
            ResultCrashedApp.gameObject.SetActive(false);
        }

        if(_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        _coroutine = StartCoroutine(IESetScores(scoreInfos));
    }
    [ContextMenu("Exe")]
    public void DebugSet()
    {
        Cursor.lockState = CursorLockMode.None;
        var scoreInfos = new List<ScoreInfo>()
        {
            new ScoreInfo(){ScoreType = ScoreType.Enginer, Score = 34},
            new ScoreInfo(){ScoreType = ScoreType.Planner, Score = 34},
            new ScoreInfo(){ScoreType = ScoreType.Artist, Score = 34},
            new ScoreInfo(){ScoreType = ScoreType.Operator, Score = 34},
        };

        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        _coroutine = StartCoroutine(IESetScores(scoreInfos));
    }
    IEnumerator IESetScores(List<ScoreInfo> scoreInfos)
    {
        ScoreLabels.ForEach(x => { x.SetScore(0); });
        FinalScoreLabel.SetScore(0);
        FinalScoreTitleLabel.gameObject.SetActive(false);
        FinalScoreDiscriptionLabel.gameObject.SetActive(false);
        GotoTitleObj.SetActive(false);
        BonustTimeCountLabel.text = $"{GameModel.Instance.TimeAddedCount} x 10s";

        CountSe.Play();
        foreach (var scoreinfo in scoreInfos)
        {
            var label = ScoreLabels.FirstOrDefault(x => x.ScoreType == scoreinfo.ScoreType);

            var coroutine = SetScore(label, scoreinfo.Score);

            while (coroutine.MoveNext())
            {
                if(coroutine.Current)
                {
                    break;
                }

                yield return null;
            }

            CountStopSe.Play();
        }

        TappedSkip = false;

        var totalScore = scoreInfos.Sum(x => x.Score);
        var finalCoutine = SetScore(FinalScoreLabel, totalScore);

        while (finalCoutine.MoveNext())
        {
            if (finalCoutine.Current)
            {
                break;
            }

            yield return null;
        }
        CountStopSe.Play();
        CountSe.Stop();
        yield return new WaitForSeconds(0.5f);
        FinalResultPunchSe.Play();
        StartCoroutine(Shake(0.2f, 50f));

        ResultType resultType = ResultType.Newbie;
        if (totalScore > (long)ResultType.Newbie)
        {
            var scoreResult = ResultDisplayDict.Where(x => totalScore >= (long)x.Key)
            .OrderByDescending(x => (long)x.Key)
            .First();
            resultType = scoreResult.Key;
        }

        var resultString = ResultDisplayDict[resultType];

        FinalScoreTitleLabel.gameObject.SetActive(true);
        FinalScoreTitleLabel.text = resultString.Item1;

        yield return new WaitForSeconds(0.5f);

        FinalScoreDiscriptionLabel.gameObject.SetActive(true);
        FinalScoreDiscriptionLabel.text = resultString.Item2;

        GotoTitleObj.SetActive(true);
    }
    IEnumerator<bool> SetScore(UIResultScore scoreLabels, long score)
    {
        var elapsedTime = 0f;

        while (elapsedTime < ScoreSinglePerformTime || TappedSkip)
        {
            elapsedTime += Time.deltaTime;
            var displayScore = elapsedTime / ScoreSinglePerformTime * score;
            scoreLabels.SetScore((int)displayScore);
            yield return false;
        }

        scoreLabels.SetScore(score);
        scoreLabels.AfterSet();
        yield return true;
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + offsetX, originalPos.y + offsetY, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null; // 次のフレームまで待機
        }

        // カメラの位置を元に戻す
        transform.localPosition = originalPos;
    }
    public void OnClickSkip()
    {
        TappedSkip = true;
    }
    public void OnClickTitle()
    {
        SoundManager.Instance.PlayInGameBGM();
        GameModel.Instance.GotoTitle();
    }
}
