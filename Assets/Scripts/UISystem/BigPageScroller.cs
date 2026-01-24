using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class BigPageScroller : MonoBehaviour
{
    public RectTransform bigPage;
    public RectTransform viewport;
    public bool isHorizontal = true;
    public bool smooth = true;
    public float smoothSpeed = 10f;

    private Slider slider;
    private float maxScroll;
    private Vector2 targetPos;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void Start()
    {
        CalculateMaxScroll();
        targetPos = bigPage.anchoredPosition;
    }

    void CalculateMaxScroll()
    {
        if (isHorizontal)
        {
            maxScroll = bigPage.rect.width - viewport.rect.width;
        }
        else
        {
            maxScroll = bigPage.rect.height - viewport.rect.height;
        }

        // 防止出现负数
        maxScroll = Mathf.Max(0, maxScroll);
    }

    void OnSliderValueChanged(float value)
    {
        float pos = value * maxScroll;

        if (isHorizontal)
        {
            targetPos.x = pos;
        }
        else
        {
            targetPos.y = pos;
        }
    }

    void Update()
    {
        if (smooth)
        {
            bigPage.anchoredPosition = Vector2.Lerp(bigPage.anchoredPosition, targetPos, smoothSpeed * Time.deltaTime);
        }
        else
        {
            bigPage.anchoredPosition = targetPos;
        }
    }

}
