using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class MoneyUI : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private GameObject moneyTextPrefab;
    [SerializeField] private RectTransform UIParent;
    
    [Header("ANIMATION")]
    [SerializeField] private float moveSpeed = 60f;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private float randomness = 12f;
    [SerializeField] private float spawnOffsetY = 12f;
    
    [Header("COLORS")]
    [SerializeField] private Color positiveColor = new Color(0.24f, 0.77f, 0.29f);
    [SerializeField] private Color negativeColor = new Color(0.96f, 0.27f, 0.30f);
    [SerializeField] private Color neutralColor = new Color(0.74f, 0.69f, 0.70f);

    public static MoneyUI Instance { get; private set; }

    void Awake(){
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowMoneyFeedback(int value, Vector3 screenPosition){
        if(moneyTextPrefab == null || UIParent == null) return;

        screenPosition.y += spawnOffsetY;
        GameObject textObj = Instantiate(moneyTextPrefab, UIParent);
        TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        
        Vector2 anchoredPosition;
        Canvas canvas = UIParent.GetComponentInParent<Canvas>();
        
        if(canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera){
            Vector2 localPoint;
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Camera canvasCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
            
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, canvasCamera, out localPoint)) anchoredPosition = localPoint;
            else anchoredPosition = Vector2.zero;
        }else anchoredPosition = screenPosition;
        
        rectTransform.anchoredPosition = anchoredPosition;
        
        string text = value > 0 ? $"+{value}" : value.ToString();
        Color textColor = value > 0 ? positiveColor : (value < 0 ? negativeColor : neutralColor);
        
        textComponent.text = text;
        textComponent.color = textColor;
        
        StartCoroutine(AnimateMoneyText(rectTransform, textComponent, anchoredPosition));
    }

    IEnumerator AnimateMoneyText(RectTransform rectTransform, TextMeshProUGUI text, Vector2 startPos){
        float elapsed = 0f;
        Color originalColor = text.color;

        float randomX = Random.Range(-randomness, randomness);
        float randomY = Random.Range(0f, randomness * 0.5f);

        while(elapsed < lifeTime){
            elapsed += Time.deltaTime;
            float t = elapsed / lifeTime;
            
            Vector2 offset = new Vector2(
                randomX * Mathf.Sin(t * Mathf.PI * 2f),
                (moveSpeed * elapsed) + randomY
            );

            rectTransform.anchoredPosition = startPos + offset;

            if(elapsed < fadeDuration){
                float scaleT = elapsed / fadeDuration;
                float scale = Mathf.Lerp(0.5f, 1f, scaleT);
                rectTransform.localScale = Vector3.one * scale;
            }

            if(elapsed > lifeTime * 0.4f){
                float fadeT = (elapsed - lifeTime * 0.4f) / (lifeTime * 0.6f);
                Color c = originalColor;
                c.a = Mathf.Lerp(1f, 0f, fadeT);
                text.color = c;
            }

            yield return null;
        }

        Destroy(rectTransform.gameObject);
    }
}