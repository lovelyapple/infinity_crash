using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIResultScore : MonoBehaviour
{
    public ScoreType ScoreType;
    [SerializeField] private TextMeshProUGUI ScoreText;
    Vector3 _defaultScale;
    float perFormeScale = 1.2f;
    public void BeforeSet()
    {
        _defaultScale = ScoreText.rectTransform.localScale;
    }
    public void SetScore(long score)
    {
        ScoreText.text = score.ToString();
        ScoreText.rectTransform.localScale = perFormeScale * _defaultScale;
    }
    public void AfterSet()
    {
        ScoreText.rectTransform.localScale = _defaultScale;
    }
}
