using UnityEngine;
using Nova;
using System.Collections;
using NovaSamples.UIControls;
using UnityEngine.SceneManagement;

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
    [SerializeField] private UIBlock2D restartButton;
    [SerializeField] private UIBlock2D menuButton;
    [SerializeField] private UIBlock2D quitButton;
    [SerializeField] private Color32 rebornHover;
    [SerializeField] private Color32 rebornUnhover;
    [Space(10)]
    [SerializeField] private float buttonHoverScale = 1.1f;
    [SerializeField] private float buttonAnimationSpeed = 8f;

    [Header("PANEL ANIMATION")]
    [SerializeField] private float panelPopDuration = 0.3f;
    [SerializeField] private float panelPopScale = 1.1f;
    [SerializeField] private AnimationCurve panelPopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 buttonOriginalScale;
    private Coroutine restartButtonHoverCoroutine;
    private Coroutine menuButtonHoverCoroutine;
    private Coroutine quitButtonHoverCoroutine;
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

        if(restartButton != null){
            buttonOriginalScale = restartButton.transform.localScale;
            restartButton.AddGestureHandler<Gesture.OnPress>(RestartClick);
            restartButton.AddGestureHandler<Gesture.OnHover>(RestartHover);
            restartButton.AddGestureHandler<Gesture.OnUnhover>(RestartUnhover);
        }

        if(menuButton != null){
            buttonOriginalScale = menuButton.transform.localScale;
            menuButton.AddGestureHandler<Gesture.OnPress>(MenuClick);
            menuButton.AddGestureHandler<Gesture.OnHover>(MenuHover);
            menuButton.AddGestureHandler<Gesture.OnUnhover>(MenuUnhover);
        }

        if(quitButton != null){
            buttonOriginalScale = quitButton.transform.localScale;
            quitButton.AddGestureHandler<Gesture.OnPress>(QuitClick);
            quitButton.AddGestureHandler<Gesture.OnHover>(QuitHover);
            quitButton.AddGestureHandler<Gesture.OnUnhover>(QuitUnhover);
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

    void RestartClick(Gesture.OnPress evt){
        SceneManager.LoadScene("Main Scene");
    }

    void RestartHover(Gesture.OnHover evt){
        if(restartButton == null || isPanelAnimating) return;
        
        if(restartButtonHoverCoroutine != null) StopCoroutine(restartButtonHoverCoroutine);
        restartButtonHoverCoroutine = StartCoroutine(ButtonHoverAnimation(restartButton, true));
    }

    void RestartUnhover(Gesture.OnUnhover evt){
        if(restartButton == null) return;
        
        if(restartButtonHoverCoroutine != null) StopCoroutine(restartButtonHoverCoroutine);
        restartButtonHoverCoroutine = StartCoroutine(ButtonHoverAnimation(restartButton, false));
    }

    void MenuClick(Gesture.OnPress evt){
        SceneManager.LoadScene("Menu Scene");
    }

    void MenuHover(Gesture.OnHover evt){
        if(menuButton == null || isPanelAnimating) return;
        
        if(menuButtonHoverCoroutine != null) StopCoroutine(menuButtonHoverCoroutine);
        menuButtonHoverCoroutine = StartCoroutine(ButtonHoverAnimation(menuButton, true));
    }

    void MenuUnhover(Gesture.OnUnhover evt){
        if(menuButton == null) return;
        
        if(menuButtonHoverCoroutine != null) StopCoroutine(menuButtonHoverCoroutine);
        menuButtonHoverCoroutine = StartCoroutine(ButtonHoverAnimation(menuButton, false));
    }

    void QuitClick(Gesture.OnPress evt){
        StartCoroutine(SelfDestruct());
        //GameManager.Instance.StartCoroutine(GameManager.Instance.StartGame());
        //ResetGameOver();
    }

    void QuitHover(Gesture.OnHover evt){
        if(quitButton == null || isPanelAnimating) return;
        
        if(quitButtonHoverCoroutine != null) StopCoroutine(quitButtonHoverCoroutine);
        quitButtonHoverCoroutine = StartCoroutine(ButtonHoverAnimation(quitButton, true));
    }

    void QuitUnhover(Gesture.OnUnhover evt){
        if(quitButton == null) return;
        
        if(quitButtonHoverCoroutine != null) StopCoroutine(quitButtonHoverCoroutine);
        quitButtonHoverCoroutine = StartCoroutine(ButtonHoverAnimation(quitButton, false));
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
    

    IEnumerator ButtonHoverAnimation(UIBlock2D button, bool hovering){
        if(button == null) yield break;
        
        Vector3 startScale = button.transform.localScale;
        Vector3 targetScale = hovering ? buttonOriginalScale * buttonHoverScale : buttonOriginalScale;
        
        Color32 startColor = button.Color;
        Color32 targetColor = hovering ? rebornHover : rebornUnhover;
        
        float elapsed = 0f;
        
        while(elapsed < 1f / buttonAnimationSpeed){
            elapsed += Time.deltaTime;
            float t = elapsed * buttonAnimationSpeed;
            
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            Vector3 newScale = Vector3.Lerp(startScale, targetScale, easedT);
            button.transform.localScale = newScale;
            
            Color32 newColor = Color32.Lerp(startColor, targetColor, easedT);
            button.Color = newColor;
            
            yield return null;
        }
        
        button.transform.localScale = targetScale;
        button.Color = targetColor;
        
        if(button == restartButton) restartButtonHoverCoroutine = null;
        if(button == restartButton) menuButtonHoverCoroutine = null;
        if(button == restartButton) quitButtonHoverCoroutine = null;
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

        // Restart Button
        if(restartButton != null){
            restartButton.transform.localScale = buttonOriginalScale;
            restartButton.Color = rebornUnhover;
        }
        
        if(restartButtonHoverCoroutine != null){
            StopCoroutine(restartButtonHoverCoroutine);
            restartButtonHoverCoroutine = null;
        }

        // Menu button
        if(menuButton != null){
            menuButton.transform.localScale = buttonOriginalScale;
            menuButton.Color = rebornUnhover;
        }
        
        if(menuButtonHoverCoroutine != null){
            StopCoroutine(menuButtonHoverCoroutine);
            menuButtonHoverCoroutine = null;
        }

        // Quit button
        if(quitButton != null){
            quitButton.transform.localScale = buttonOriginalScale;
            quitButton.Color = rebornUnhover;
        }
        
        if(quitButtonHoverCoroutine != null){
            StopCoroutine(quitButtonHoverCoroutine);
            quitButtonHoverCoroutine = null;
        }
        
        
        if(panelAnimationCoroutine != null){
            StopCoroutine(panelAnimationCoroutine);
            panelAnimationCoroutine = null;
        }
        
        isPanelAnimating = false;
    }
}