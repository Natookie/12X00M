using UnityEngine;
using Nova;
using System.Collections;

public class GameOver : MonoBehaviour
{   
    [Header("VIGNETTE")]
    [SerializeField] private UIBlock2D vignetteBlock;
    [SerializeField] private Color32 outerColor;
    [SerializeField] private Color32 innerColor;
    [Space(10)]
    [SerializeField] private float vignetteDuration = 1.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("PANEL")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextBlock moneyText;
    [SerializeField] private TextBlock roundText;

    [Header("BUTTON")]
    [SerializeField] private UIBlock2D rebornButton;
    [SerializeField] private Color32 rebornHover;
    [SerializeField] private Color32 rebornUnhover;
    [Space(10)]
    [SerializeField] private float buttonHoverScale = 1.1f;
    [SerializeField] private float buttonAnimationSpeed = 8f;

    [Header("PANEL ANIMATION")]
    [SerializeField] private float panelPopDuration = 0.3f;
    [SerializeField] private float panelPopScale = 1.1f;
    [SerializeField] private AnimationCurve panelPopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 rebornButtonOriginalScale;
    private Coroutine rebornHoverCoroutine;
    private Coroutine panelAnimationCoroutine;
    private bool isPanelAnimating = false;

    void Start(){
        if(vignetteBlock != null){
            Color32 transparentInner = innerColor;
            transparentInner.a = 0;
            vignetteBlock.Color = transparentInner;
            
            if(vignetteBlock.Gradient != null){
                Color32 transparentOuter = outerColor;
                transparentOuter.a = 0;
                vignetteBlock.Gradient.Color = transparentOuter;
            }
        }

        if(rebornButton != null){
            rebornButtonOriginalScale = rebornButton.transform.localScale;
            rebornButton.AddGestureHandler<Gesture.OnPress>(RebornClick);
            rebornButton.AddGestureHandler<Gesture.OnHover>(RebornHover);
            rebornButton.AddGestureHandler<Gesture.OnUnhover>(RebornUnhover);
        }
        
        resultPanel.SetActive(false);
    }

    public void PlayVignette(){
        if(vignetteBlock == null) return;

        vignetteBlock.GetComponent<Interactable>().enabled = true;
        StartCoroutine(VignetteAnimation());
    }

    IEnumerator VignetteAnimation(){
        Color32 startInner = innerColor;
        startInner.a = 0;
        
        Color32 startOuter = outerColor;
        startOuter.a = 0;
        
        Color32 targetInner = innerColor;
        targetInner.a = 204;
        
        Color32 targetOuter = outerColor;
        targetOuter.a = 128;
        
        float elapsed = 0f;
        
        while(elapsed < vignetteDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / vignetteDuration;
            float easedT = fadeCurve.Evaluate(t);
            
            Color32 currentInner = Color32.Lerp(startInner, targetInner, easedT);
            vignetteBlock.Color = currentInner;
            
            if(vignetteBlock.Gradient != null){
                Color32 currentOuter = Color32.Lerp(startOuter, targetOuter, easedT);
                vignetteBlock.Gradient.Color = currentOuter;
            }
            
            yield return null;
        }
        
        vignetteBlock.Color = targetInner;
        if(vignetteBlock.Gradient != null) vignetteBlock.Gradient.Color = targetOuter;
    }

    public void ShowResult(){
        if(resultPanel != null){
            resultPanel.SetActive(true);
            
            if(moneyText != null) moneyText.Text = $"{MoneyManager.Instance.Money}";
            if(roundText != null) roundText.Text = $"{RoundManager.Instance.CheckCurrentRound()}";
            
            if(panelAnimationCoroutine != null) StopCoroutine(panelAnimationCoroutine);
            panelAnimationCoroutine = StartCoroutine(PanelPopAnimation());
        }
    }

    IEnumerator PanelPopAnimation(){
        if(resultPanel == null) yield break;
        
        isPanelAnimating = true;
        
        Vector3 originalScale = resultPanel.transform.localScale;
        Vector3 targetScale = originalScale * panelPopScale;
        
        float elapsed = 0f;
        
        while(elapsed < panelPopDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / panelPopDuration;
            float curvedT = panelPopCurve.Evaluate(t);
            
            float scale = Mathf.Lerp(0f, panelPopScale, curvedT);
            resultPanel.transform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        resultPanel.transform.localScale = originalScale * panelPopScale;
        
        elapsed = 0f;
        while(elapsed < panelPopDuration * 0.5f){
            elapsed += Time.deltaTime;
            float t = elapsed / (panelPopDuration * 0.5f);
            
            float scale = Mathf.Lerp(panelPopScale, 1f, t);
            resultPanel.transform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        resultPanel.transform.localScale = originalScale;
        
        isPanelAnimating = false;
        panelAnimationCoroutine = null;
    }

    void RebornClick(Gesture.OnPress evt){
        StartCoroutine(SelfDestruct());
        //GameManager.Instance.StartCoroutine(GameManager.Instance.StartGame());
        //ResetGameOver();
    }

    IEnumerator SelfDestruct(){
        DialogueManager.Instance.SetDialogue(
            DialogueManager.CharacterType.Player,
            "Initiating self destruct in\n~3. ~2. ~1"
        );
        yield return new WaitWhile(() => DialogueManager.Instance.IsTypingActive());
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void RebornHover(Gesture.OnHover evt){
        if(rebornButton == null || isPanelAnimating) return;
        
        if(rebornHoverCoroutine != null) StopCoroutine(rebornHoverCoroutine);
        rebornHoverCoroutine = StartCoroutine(RebornHoverAnimation(true));
    }

    void RebornUnhover(Gesture.OnUnhover evt){
        if(rebornButton == null) return;
        
        if(rebornHoverCoroutine != null) StopCoroutine(rebornHoverCoroutine);
        rebornHoverCoroutine = StartCoroutine(RebornHoverAnimation(false));
    }

    IEnumerator RebornHoverAnimation(bool hovering){
        if(rebornButton == null) yield break;
        
        Vector3 startScale = rebornButton.transform.localScale;
        Vector3 targetScale = hovering ? rebornButtonOriginalScale * buttonHoverScale : rebornButtonOriginalScale;
        
        Color32 startColor = rebornButton.Color;
        Color32 targetColor = hovering ? rebornHover : rebornUnhover;
        
        float elapsed = 0f;
        
        while(elapsed < 1f / buttonAnimationSpeed){
            elapsed += Time.deltaTime;
            float t = elapsed * buttonAnimationSpeed;
            
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            Vector3 newScale = Vector3.Lerp(startScale, targetScale, easedT);
            rebornButton.transform.localScale = newScale;
            
            Color32 newColor = Color32.Lerp(startColor, targetColor, easedT);
            rebornButton.Color = newColor;
            
            yield return null;
        }
        
        rebornButton.transform.localScale = targetScale;
        rebornButton.Color = targetColor;
        
        rebornHoverCoroutine = null;
    }

    public void ResetGameOver(){
        if(vignetteBlock != null){
            Color32 transparentInner = innerColor;
            transparentInner.a = 0;
            vignetteBlock.Color = transparentInner;
            
            if(vignetteBlock.Gradient != null){
                Color32 transparentOuter = outerColor;
                transparentOuter.a = 0;
                vignetteBlock.Gradient.Color = transparentOuter;
            }
        }
        
        if(resultPanel != null){
            resultPanel.SetActive(false);
            resultPanel.transform.localScale = Vector3.one;
        }
        
        if(rebornButton != null){
            rebornButton.transform.localScale = rebornButtonOriginalScale;
            rebornButton.Color = rebornUnhover;
        }
        
        if(rebornHoverCoroutine != null){
            StopCoroutine(rebornHoverCoroutine);
            rebornHoverCoroutine = null;
        }
        
        if(panelAnimationCoroutine != null){
            StopCoroutine(panelAnimationCoroutine);
            panelAnimationCoroutine = null;
        }
        
        isPanelAnimating = false;
    }
}