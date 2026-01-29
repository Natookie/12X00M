using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Nova;

public class MoneyUI : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private GameObject moneyTextPrefab;
    [SerializeField] private RectTransform UIParent;
    [SerializeField] private TextBlock[] moneyDigitBlocks;
    [SerializeField] private TextBlock moneyIcon;
    [SerializeField] private UIBlock2D moneyIconBlock;

    [Header("ANIMATION")]
    [SerializeField] private float moveSpeed = 60f;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private float randomness = 12f;
    [SerializeField] private float spawnOffsetY = 12f;
    [SerializeField] private float digitScrollSpeed = 10f;
    [SerializeField] private float digitAnimationDuration = 0.8f;
    [SerializeField] private float iconBounceHeight = 20f;
    [SerializeField] private float iconBounceSpeed = 15f;
    [SerializeField] private float iconColorChangeSpeed = 5f;

    [Header("COLORS")]
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;
    [SerializeField] private Color neutralColor;
    [Space(10)]
    [SerializeField] private Color darkPositiveColor;
    [SerializeField] private Color darkNegativeColor;
    [SerializeField] private Color originalIconColor;

    private int currentDisplayedMoney = 0;
    private int targetMoney = 0;
    private Coroutine moneyUpdateCoroutine;
    private Coroutine iconAnimationCoroutine;
    private Color targetIconColor;

    private bool isAnimatingIcon = false;
    private float originalIconY;
    private bool originalPositionStored = false;
    
    public static MoneyUI Instance { get; private set; }

    void Awake(){
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if(moneyIconBlock != null && moneyIcon != null){
            originalIconColor = moneyIcon.Color;
            targetIconColor = originalIconColor;
        }
    }

    void Start(){
        if(moneyIconBlock == null && moneyIcon != null){
            moneyIconBlock = moneyIcon.GetComponent<UIBlock2D>();
            if(moneyIconBlock != null){
                originalIconColor = moneyIcon.Color;
                targetIconColor = originalIconColor;
            }
        }

        if(moneyIconBlock != null && !originalPositionStored){
            originalIconY = moneyIconBlock.Position.Y.Value;
            originalPositionStored = true;
        }
    }

    #region DIGIT DISPLAY SYSTEM
    public void UpdateMoneyDisplay(int newMoney){
        int previousMoney = currentDisplayedMoney;
        targetMoney = newMoney;
        
        if(moneyUpdateCoroutine != null) StopCoroutine(moneyUpdateCoroutine);
        moneyUpdateCoroutine = StartCoroutine(AnimateMoneyChange(previousMoney, targetMoney));
        
        if(moneyIconBlock != null){
            if(iconAnimationCoroutine != null) StopCoroutine(iconAnimationCoroutine);
            iconAnimationCoroutine = StartCoroutine(AnimateIcon(previousMoney, newMoney));
        }
    }
    
    IEnumerator AnimateMoneyChange(int startMoney, int endMoney){
        float elapsed = 0f;
        
        while(elapsed < digitAnimationDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / digitAnimationDuration;
            
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            int interpolatedMoney = Mathf.RoundToInt(Mathf.Lerp(startMoney, endMoney, easedT));
            
            UpdateDigitsWithScroll(startMoney, endMoney, interpolatedMoney, t);
            
            yield return null;
        }
        
        currentDisplayedMoney = endMoney;
        UpdateMoneyDisplayImmediate(endMoney);
    }
    
    void UpdateDigitsWithScroll(int startMoney, int endMoney, int currentMoney, float progress){
        string currentStr = Mathf.Abs(currentMoney).ToString();
        string startStr = Mathf.Abs(startMoney).ToString();
        string endStr = Mathf.Abs(endMoney).ToString();
        
        int digitCount = moneyDigitBlocks.Length;
        int currentDigits = currentStr.Length;
        
        for(int i = digitCount - 1; i >= 0; i--){
            int digitPlaceFromRight = digitCount - 1 - i;
            bool isAnimated = false;
            char displayChar = '0';
            
            if(digitPlaceFromRight < currentDigits){
                int charIndex = currentDigits - 1 - digitPlaceFromRight;
                displayChar = currentStr[charIndex];
                
                if(digitPlaceFromRight < startStr.Length && digitPlaceFromRight < endStr.Length){
                    int startCharIndex = startStr.Length - 1 - digitPlaceFromRight;
                    int endCharIndex = endStr.Length - 1 - digitPlaceFromRight;
                    
                    if(startStr[startCharIndex] != endStr[endCharIndex]) isAnimated = true;
                }
                else isAnimated = true;
            }
            
            if(isAnimated && progress < 0.8f){
                int randomDigit = Random.Range(1, 10);
                moneyDigitBlocks[i].Text = randomDigit.ToString();
            }else moneyDigitBlocks[i].Text = displayChar.ToString();
        }
        
        for(int i = 0; i < digitCount; i++) moneyDigitBlocks[i].Color = positiveColor;
        bool foundNonZero = false;
        for(int i = 0; i < digitCount; i++){
            if(moneyDigitBlocks[i].Text == "0" && !foundNonZero) moneyDigitBlocks[i].Color = neutralColor;
            else foundNonZero = true;
        }
    }
    
    IEnumerator AnimateIcon(int previousMoney, int newMoney){
        isAnimatingIcon = true;
        float elapsed = 0f;
        float bounceElapsed = 0f;
        
        targetIconColor = (newMoney > previousMoney) ? darkPositiveColor : 
                         (newMoney < previousMoney) ? darkNegativeColor : 
                         originalIconColor;
        
        int moneyDiff = Mathf.Abs(newMoney - previousMoney);
        float colorIntensity = Mathf.Clamp01(moneyDiff / 100f) * 0.5f + 0.5f;
        targetIconColor = Color.Lerp(originalIconColor, targetIconColor, colorIntensity);
        
        float animationStartY = originalIconY;
        
        while(elapsed < digitAnimationDuration){
            elapsed += Time.deltaTime;
            bounceElapsed += Time.deltaTime;
            
            float t = elapsed / digitAnimationDuration;
            
            float bounceT = bounceElapsed * iconBounceSpeed;
            float bounce = Mathf.Sin(bounceT * Mathf.PI * 2f) * 
                          iconBounceHeight * 
                          Mathf.Max(0, 1f - t * 2f);
            
            moneyIconBlock.Position.Y.Value = animationStartY + bounce;
            
            float colorT;
            if(t < 0.3f) colorT = t / 0.3f;
            else if(t < 0.8f) colorT = 1f;
            else colorT = 1f - ((t - 0.8f) / 0.2f);
            
            Color currentColor = Color.Lerp(originalIconColor, targetIconColor, colorT);
            moneyIcon.Color = Color.Lerp(moneyIcon.Color, currentColor, Time.deltaTime * iconColorChangeSpeed);
            
            float scale = 1f + Mathf.Sin(bounceT * Mathf.PI) * 0.1f * (1f - t);
            moneyIconBlock.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        float resetDuration = 0.2f;
        float resetElapsed = 0f;
        float endY = moneyIconBlock.Position.Y.Value;
        Color endColor = moneyIcon.Color;
        Vector3 endScale = moneyIconBlock.transform.localScale;
        
        while(resetElapsed < resetDuration){
            resetElapsed += Time.deltaTime;
            float resetT = resetElapsed / resetDuration;
            
            moneyIconBlock.Position.Y.Value = Mathf.Lerp(endY, originalIconY, resetT);
            moneyIcon.Color = Color.Lerp(endColor, originalIconColor, resetT);
            moneyIconBlock.transform.localScale = Vector3.Lerp(endScale, Vector3.one, resetT);
            
            yield return null;
        }
        
        moneyIconBlock.Position.Y.Value = originalIconY;
        moneyIcon.Color = originalIconColor;
        moneyIconBlock.transform.localScale = Vector3.one;
        
        isAnimatingIcon = false;
        iconAnimationCoroutine = null;
    }
    
    void UpdateMoneyDisplayImmediate(int money){
        string moneyStr = Mathf.Abs(money).ToString();
        int digitCount = moneyDigitBlocks.Length;
        
        for(int i = 0; i < digitCount; i++) moneyDigitBlocks[i].Color = positiveColor;
        bool foundNonZero = false;
        for(int i = 0; i < digitCount; i++){
            if(moneyDigitBlocks[i].Text == "0" && !foundNonZero) moneyDigitBlocks[i].Color = neutralColor;
            else foundNonZero = true;
        }
        
        currentDisplayedMoney = money;
    }
    #endregion

    #region CANVAS ANIMATION
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
    #endregion
    
    public void QuickIconBounce(bool positiveChange){
        if(moneyIconBlock == null || isAnimatingIcon) return;
        
        if(iconAnimationCoroutine != null) StopCoroutine(iconAnimationCoroutine);
        iconAnimationCoroutine = StartCoroutine(QuickBounceRoutine(positiveChange));
    }
    
    IEnumerator QuickBounceRoutine(bool positive){
        float elapsed = 0f;
        float duration = 0.4f;
        float animationStartY = originalIconY;
        Color targetColor = positive ? darkPositiveColor : darkNegativeColor;
        
        while(elapsed < duration){
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float bounce = Mathf.Sin(t * Mathf.PI * 4f) * iconBounceHeight * 0.5f * (1f - t);
            moneyIconBlock.Position.Y.Value = animationStartY + bounce;
            
            float colorT = Mathf.Sin(t * Mathf.PI);
            moneyIcon.Color = Color.Lerp(originalIconColor, targetColor, colorT);
            
            yield return null;
        }
        
        moneyIconBlock.Position.Y.Value = originalIconY;
        moneyIcon.Color = originalIconColor;
        
        iconAnimationCoroutine = null;
    }
}