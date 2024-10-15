using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIResultController : MonoBehaviour
{
    public enum ResultType
    {
        Newbie = 40,
        Normal = 80,
        Leader = 120,
        Cheaf = 160,
        SpecialList = 220,
    }
    [SerializeField] List<UIResultScore> ScoreLabels;
    [SerializeField] UIResultScore FinalScoreLabel;
    [SerializeField] TextMeshProUGUI FinalScoreTitleLabel;
    [SerializeField] TextMeshProUGUI FinalScoreDiscriptionLabel;
    [SerializeField] GameObject GotoTitleObj;
    [SerializeField] TextMeshProUGUI BonustTimeCountLabel;

    private readonly Dictionary<ResultType, (string, string)> ResultDisplayDict = new Dictionary<ResultType, (string, string)>
    {
        {ResultType.Newbie , ("Newbie ","Getting started!")},
        {ResultType.Normal, ("Normal", " Nice performance, keep it up!")},
        {ResultType.Leader, ("Leader", "Impressive !!")},
        {ResultType.Cheaf, ("Cheaf", "Great job, you're a master!")},
        {ResultType.SpecialList, ("Specialist", "Outstanding! You're truly elite!")},
    };
    public float ScoreSinglePerformTime = 1.5f;
    public bool TappedSkip = false;
    Coroutine _coroutine;

    public void SetupScore()
    {
        TappedSkip = false;
        var scoreInfos = ApplicationPressureManager.Instance.ScoreInfos.OrderBy(x => x.ScoreType).ToList();

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
        ScoreLabels.ForEach(x => { x.BeforeSet(); x.SetScore(0); });
        FinalScoreLabel.BeforeSet();
        FinalScoreLabel.SetScore(0);
        FinalScoreTitleLabel.gameObject.SetActive(false);
        FinalScoreDiscriptionLabel.gameObject.SetActive(false);
        GotoTitleObj.SetActive(false);
        BonustTimeCountLabel.text = $"{GameModel.Instance.TimeAddedCount} x 10s";
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

        yield return new WaitForSeconds(0.5f);
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
        scoreLabels.BeforeSet();

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
        GameModel.Instance.GotoTitle();
    }
}
