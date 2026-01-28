using UnityEngine;
using UnityEngine.UI;
using Nova;
using System.Text;
using System.Collections.Generic;
using System.Collections;

public class TaskUI : MonoBehaviour
{
    [Header("UI ELEMENTS")]
    [SerializeField] private UIBlock2D refreshMainBtn;
    [SerializeField] private UIBlock2D refreshExtraBtn;
    [Space(5)]
    [SerializeField] private UIBlock2D endRoundBtn;
    [SerializeField] private UIBlock2D endRoundBtn_Container;
    [Space(10)]
    [SerializeField] private TextBlock mainTaskText;
    [SerializeField] private TextBlock extraTaskText;
    [SerializeField] private TextBlock refreshCostText;
    [SerializeField] private TextBlock completeRewardText;
    [Space(10)]
    [SerializeField] private TextBlock endRoundText;
    [SerializeField] private UIBlock2D endRoundIcon;
    [SerializeField] private Sprite[] endRoundSprite;
    [Space(10)]
    [SerializeField] private CanvasGroup refreshFeedback;

    [Header("COLORS")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color unavailableColor = Color.gray;
    [Space(10)]
    [SerializeField] private Color endRoundHover;
    [SerializeField] private Color endRoundUnhover;
    [Space(10)]
    [SerializeField] private Color refreshIconAvailableColor = new Color32(73, 149, 243, 255);
    [SerializeField] private Color refreshIconUnavailableColor = new Color32(100, 100, 100, 255);

    [Header("HOVER ANIMATION")]
    [SerializeField] private float hoverLiftAmount = 20f;
    [SerializeField] private float hoverAnimationSpeed = 15f;
    [SerializeField] private float shineAnimationSpeed = 1.5f;
    [SerializeField] private float shiningMinX;
    [SerializeField] private float shiningMaxX;

    [Header("REFRESH ANIMATION")]
    [SerializeField] private float refreshScaleAmount = 1.1f;
    [SerializeField] private float refreshAnimationDuration = 0.3f;

    [Header("REFERENCES")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private GridHighlight gridHighlight;

    private UIBlock2D refreshIconMain;
    private UIBlock2D refreshIconExtra;
    private Coroutine endRoundHoverCoroutine;
    private Coroutine shineCoroutine;
    private Coroutine refreshMainAnimationRoutine;
    private Coroutine refreshExtraAnimationRoutine;
    private float originalEndRoundY;
    private bool isEndRoundAnimating = false;
    private bool isShinePlaying = false;

    void Start(){
        if(refreshMainBtn != null && refreshMainBtn.transform.childCount > 0) refreshIconMain = refreshMainBtn.transform.GetChild(0).GetComponent<UIBlock2D>();
        if(refreshExtraBtn != null && refreshExtraBtn.transform.childCount > 0) refreshIconExtra = refreshExtraBtn.transform.GetChild(0).GetComponent<UIBlock2D>();
        
        if(endRoundBtn != null){
            originalEndRoundY = endRoundBtn.Position.Y.Value;
            if(endRoundBtn.Gradient != null){
                endRoundBtn.Gradient.Enabled = false;
                endRoundBtn.Gradient.Center.X.Value = -115f;
            }
        }
        
        if(refreshMainBtn != null){
            refreshMainBtn.AddGestureHandler<Gesture.OnPress>(RefreshMainClick);
            refreshMainBtn.AddGestureHandler<Gesture.OnHover>(RefreshHover);
            refreshMainBtn.AddGestureHandler<Gesture.OnUnhover>(RefreshUnhover);
        }
        
        if(refreshExtraBtn != null){
            refreshExtraBtn.AddGestureHandler<Gesture.OnPress>(RefreshExtraClick);
            refreshExtraBtn.AddGestureHandler<Gesture.OnHover>(RefreshHover);
            refreshExtraBtn.AddGestureHandler<Gesture.OnUnhover>(RefreshUnhover);
        }
        
        if(endRoundBtn_Container != null){
            endRoundBtn_Container.AddGestureHandler<Gesture.OnPress>(EndRoundClick);
            endRoundBtn_Container.AddGestureHandler<Gesture.OnHover>(EndRoundHover);
            endRoundBtn_Container.AddGestureHandler<Gesture.OnUnhover>(EndRoundUnhover);
        }
        
        if(taskManager != null) taskManager.OnTasksUpdated += UpdateUI;
        
        UpdateUI();
    }

    void OnDestroy(){
        if(taskManager != null) taskManager.OnTasksUpdated -= UpdateUI;
        StopAllCoroutines();
    }

    void Update(){
        if(CanClick()){
            endRoundText.Text = "End Round";
            endRoundIcon.SetImage(endRoundSprite[0]);
        }else{
            endRoundText.Text = "Interrupted";
            endRoundIcon.SetImage(endRoundSprite[1]);
        }
    }

    public void UpdateUI(){
        UpdateMainTaskDisplay();
        UpdateExtraTaskDisplay();
        UpdateButtonStates();
        UpdateCostDisplay();
        UpdateRewardDisplay();
    }

    public void UpdateMainTaskDisplay(){
        if(mainTaskText == null || taskManager == null) return;
        
        List<TaskData> mainTasks = taskManager.GetMainTasks();
        if(mainTasks.Count == 0){
            mainTaskText.Text = "No main tasks assigned";
            mainTaskText.Color = unavailableColor;
            return;
        }
        
        StringBuilder sb = new StringBuilder();
        foreach(TaskData task in mainTasks){
            string description = taskManager.GetTaskDescription(task);
            bool completed = taskManager.IsTaskCompleted(task);
            
            string colorTag = completed ? "<color=#3ec54b>" : "<color=#3c2f52>";
            string statusIcon = completed ? "V " : "- ";
            
            sb.AppendLine($"{colorTag}{statusIcon}{description}</color>");
        }
        
        mainTaskText.Text = sb.ToString();
    }

    void UpdateExtraTaskDisplay(){
        if(extraTaskText == null || taskManager == null) return;
        
        List<TaskData> extraTasks = taskManager.GetExtraTasks();
        if(extraTasks.Count == 0){
            mainTaskText.Text = "No extra tasks assigned";
            mainTaskText.Color = unavailableColor;
            return;
        }
        
        extraTaskText.gameObject.SetActive(true);
        
        StringBuilder sb = new StringBuilder();
        foreach(TaskData task in extraTasks){
            string description = taskManager.GetTaskDescription(task);
            bool completed = taskManager.IsTaskCompleted(task);
            
            string colorTag = completed ? "<color=#3ec54b>" : "<color=#3c2f52>";
            string statusIcon = completed ? "V " : "- ";
            
            sb.AppendLine($"{colorTag}{statusIcon}{description}</color>");
        }
        
        extraTaskText.Text = sb.ToString();
    }

    string GetFormattedTaskLine(string taskName, string progress, bool completed){
        string colorTag = completed ? "<color=green>" : "<color=yellow>";
        string statusIcon = completed ? "V " : "- ";
        
        return $"{colorTag}{statusIcon}{taskName}: {progress}</color>";
    }

    void UpdateButtonStates(){
        bool canAfford = moneyManager != null && moneyManager.CanAfford(taskManager.GetRefreshCost());
        
        if(refreshMainBtn != null) refreshMainBtn.Color = canAfford ? normalColor : unavailableColor;
        if(refreshExtraBtn != null) refreshExtraBtn.Color = canAfford ? normalColor : unavailableColor;
        
        Color iconColor = canAfford ? refreshIconAvailableColor : refreshIconUnavailableColor;
        if(refreshIconMain != null) refreshIconMain.Color = iconColor;
        if(refreshIconExtra != null) refreshIconExtra.Color = iconColor;

        if(endRoundBtn_Container != null) endRoundBtn.Color = CanClick() ? endRoundUnhover : unavailableColor;
    }

    void UpdateCostDisplay(){
        if(refreshCostText != null && taskManager != null) refreshCostText.Text = $"Refresh: ${taskManager.GetRefreshCost()}";
    }

    void UpdateRewardDisplay(){
        if(completeRewardText != null && taskManager != null){
            int totalReward = CalculateTotalCompletedReward();
            completeRewardText.Text = $"Reward: ${totalReward}";
        }
    }

    int CalculateTotalCompletedReward(){
        int totalReward = 0;
        
        foreach(TaskData task in taskManager.GetMainTasks()){
            if(taskManager.IsTaskCompleted(task)) totalReward += task.rewardCoins * 2;
        }
        
        foreach(TaskData task in taskManager.GetExtraTasks()){
            if(taskManager.IsTaskCompleted(task)) totalReward += task.rewardCoins;
        }
        
        return totalReward;
    }

    #region BUTTON HANDLERS
    void RefreshMainClick(Gesture.OnPress evt){
        if(taskManager != null && moneyManager != null){
            if(!moneyManager.CanAfford(taskManager.GetRefreshCost())) return;
            
            taskManager.RefreshMainTasks();
            
            if(refreshMainAnimationRoutine != null) StopCoroutine(refreshMainAnimationRoutine);
            refreshMainAnimationRoutine = StartCoroutine(RefreshAnimation(refreshIconMain));
        }
    }
    
    void RefreshExtraClick(Gesture.OnPress evt){
        if(taskManager != null && moneyManager != null){
            if(!moneyManager.CanAfford(taskManager.GetRefreshCost())) return;
            
            taskManager.RefreshExtraTasks();
            
            if(refreshExtraAnimationRoutine != null) StopCoroutine(refreshExtraAnimationRoutine);
            refreshExtraAnimationRoutine = StartCoroutine(RefreshAnimation(refreshIconExtra));
        }
    }
    
    void RefreshHover(Gesture.OnHover evt) => refreshFeedback.alpha = 1f;
    void RefreshUnhover(Gesture.OnUnhover evt) => refreshFeedback.alpha = 0f;
    
    bool CanClick() => gridHighlight != null && !gridHighlight.IsAnimating;
    void EndRoundClick(Gesture.OnPress evt){
        if(endRoundBtn == null) return;
        if(!CanClick()) return;
        
        if(endRoundHoverCoroutine != null) StopCoroutine(endRoundHoverCoroutine);
        if(shineCoroutine != null){
            StopCoroutine(shineCoroutine);
            if(endRoundBtn.Gradient != null) endRoundBtn.Gradient.Enabled = false;
        }
        
        StartCoroutine(PopAnimation());
        
        if(taskManager != null) taskManager.EvaluateAllTasksAtRoundEnd();
        if(roundManager != null) roundManager.CompleteRound();
        if(moneyManager != null) moneyManager.AddMoneyForUnoccupiedTiles();
        if(gridHighlight != null) gridHighlight.AnimateWaveToBlue();
    }
    
    void EndRoundHover(Gesture.OnHover evt){
        if(endRoundBtn == null || isEndRoundAnimating) return;
        
        if(endRoundHoverCoroutine != null) StopCoroutine(endRoundHoverCoroutine);
        endRoundHoverCoroutine = StartCoroutine(EndRoundHoverAnimation(true, endRoundHover));
        
        if(!isShinePlaying && endRoundBtn.Gradient != null){
            if(shineCoroutine != null) StopCoroutine(shineCoroutine);
            shineCoroutine = StartCoroutine(ShineAnimation());
        }
    }

    void EndRoundUnhover(Gesture.OnUnhover evt){
        if(endRoundBtn == null) return;
        
        if(endRoundHoverCoroutine != null) StopCoroutine(endRoundHoverCoroutine);
        endRoundHoverCoroutine = StartCoroutine(EndRoundHoverAnimation(false, endRoundUnhover));
    }

    IEnumerator PopAnimation(){
        Vector3 originalScale = endRoundBtn.transform.localScale;
        float popScale = 1.15f;
        float popDuration = 0.1f;
        float returnDuration = 0.05f;
        
        float elapsed = 0f;
        while(elapsed < popDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            float scale = Mathf.Lerp(1f, popScale, t);
            endRoundBtn.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        elapsed = 0f;
        while(elapsed < returnDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            float scale = Mathf.Lerp(popScale, 1f, t);
            endRoundBtn.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        endRoundBtn.transform.localScale = originalScale;
    }
    
    IEnumerator EndRoundHoverAnimation(bool lifting, Color targetColor){
        isEndRoundAnimating = true;
        
        float startY = endRoundBtn.Position.Y.Value;
        float targetY = lifting ? originalEndRoundY + hoverLiftAmount : originalEndRoundY;
        
        Color startColor = endRoundBtn.Color;
        
        float elapsed = 0f;
        
        while(elapsed < 1f / hoverAnimationSpeed){
            elapsed += Time.deltaTime;
            float t = elapsed * hoverAnimationSpeed;
            
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            float newY = Mathf.Lerp(startY, targetY, easedT);
            endRoundBtn.Position.Y.Value = newY;
            
            Color newColor = Color.Lerp(startColor, targetColor, easedT);
            endRoundBtn.Color = newColor;
            
            yield return null;
        }
        
        endRoundBtn.Position.Y.Value = targetY;
        endRoundBtn.Color = targetColor;
        
        isEndRoundAnimating = false;
        endRoundHoverCoroutine = null;
    }
    
    IEnumerator ShineAnimation(){
        if(endRoundBtn == null || endRoundBtn.Gradient == null) yield break;
        
        isShinePlaying = true;
        endRoundBtn.Gradient.Enabled = true;
        
        float elapsed = 0f;
        float startX = shiningMinX;
        float endX = shiningMaxX;
        
        while(elapsed < 1f / shineAnimationSpeed){
            elapsed += Time.deltaTime;
            float t = elapsed * shineAnimationSpeed;
            
            float currentX = Mathf.Lerp(startX, endX, t);
            endRoundBtn.Gradient.Center.X.Value = currentX;
            
            yield return null;
        }
        
        endRoundBtn.Gradient.Center.X.Value = endX;
        
        endRoundBtn.Gradient.Enabled = false;
        endRoundBtn.Gradient.Center.X.Value = startX;
        
        isShinePlaying = false;
        shineCoroutine = null;
    }

    IEnumerator RefreshAnimation(UIBlock2D icon){
        if(icon == null) yield break;
        
        float elapsed = 0f;
        Vector3 originalScale = icon.transform.localScale;
        Vector3 targetScale = originalScale * refreshScaleAmount;
        Quaternion originalRotation = icon.transform.localRotation;
        
        while(elapsed < refreshAnimationDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / refreshAnimationDuration;
            
            float scaleT = Mathf.Sin(t * Mathf.PI);
            icon.transform.localScale = Vector3.Lerp(originalScale, targetScale, scaleT);
            
            float rotationAmount = 360f * t;
            icon.transform.localRotation = Quaternion.Euler(0, 0, rotationAmount);
            
            yield return null;
        }
        
        icon.transform.localScale = originalScale;
        icon.transform.localRotation = originalRotation;
        
        if(icon == refreshIconMain) refreshMainAnimationRoutine = null;
        else if(icon == refreshIconExtra) refreshExtraAnimationRoutine = null;
    }
    #endregion
}