using UnityEngine;
using UnityEngine.UI;

public class UIApplicationTrackingController : MonoBehaviour
{
    public ApplicationType ApplicationType;
    public RectTransform rectTransform;
    public FieldApplicationSpawner HoldingSpawner;
    public float fovInRadians;
    public RectTransform TargetRect;
    float halfWidth;
    public Image FillImage;
    public float MaxFlashAddSpeed = 3f;

    public float elapsedTime = 0;
    const float pid = 3.14f * 2f;
    private PressureInfo _pressureInfo;
    public float flashSpeedRate;
    public AudioSource audioSource;
    public void Start()
    {
        fovInRadians = (90 - Camera.main.fieldOfView / 2f) * Mathf.Deg2Rad;
        halfWidth = TargetRect.rect.width / 2f;
        audioSource.Stop();
    }
    public void Init(FieldApplicationSpawner spawner, PressureInfo pressureInfo)
    {
        HoldingSpawner = spawner;
        _pressureInfo = pressureInfo;
        gameObject.SetActive(true);
        audioSource.Stop();
    }
    public void Clear()
    {
        HoldingSpawner = null;
        audioSource.Stop();
    }
    public void Update()
    {
        if(_pressureInfo == null)
        {
            return;
        }
        
        var curPres = _pressureInfo.Pressure;
        var flashStartDiff = ApplicationPressureManager.Instance.PressureMax - curPres;
        var maxDiff = ApplicationPressureManager.Instance.PressureMax - ApplicationPressureManager.Instance.PressureCrashing;

        if (flashStartDiff > 0)
        {
            flashSpeedRate = 1 - flashStartDiff / maxDiff;
        }
        else
        {
            flashSpeedRate = 1f;
        }
        elapsedTime += (flashSpeedRate * MaxFlashAddSpeed *  Time.deltaTime);
        if (elapsedTime > pid)
        {
            elapsedTime -= pid;
        }

        var cosV = (Mathf.Sin(elapsedTime) + 1f) * 0.5f;

        var col = FillImage.color;
        col.a = cosV;
        FillImage.color = col;
        FillImage.fillAmount = flashSpeedRate;

        transform.localScale = flashSpeedRate > 0.7 ? Vector3.one * 2 : Vector3.one;// どうだろう、これで足りる？ 

        if(flashSpeedRate > 0.7 && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    public void FixedUpdate()
    {
        if(HoldingSpawner == null)
        {
            gameObject.SetActive(false);
            return;
        }

        var localPos = Camera.main.transform.InverseTransformPoint(HoldingSpawner.transform.position).normalized;
        // ワールド座標をスクリーン座標に変換
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(HoldingSpawner.transform.position);

        if (localPos.z < 0 || screenPosition.x < 0 || screenPosition.x > Screen.width || screenPosition.y < 0 || screenPosition.y > Screen.height)
        {
            if(localPos.x < 0)
            {
                rectTransform.anchoredPosition = new Vector2(-halfWidth, rectTransform.anchoredPosition.y);
            }
            else
            {
                rectTransform.anchoredPosition = new Vector2(halfWidth, rectTransform.anchoredPosition.y);
            }
        }
        else
        {
            Vector2 uiPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(TargetRect, screenPosition, null, out uiPosition);
            uiPosition.y = 0;
            rectTransform.anchoredPosition = uiPosition;
        }
    }
}
