using UnityEngine;
using Nova;
using System.Text;
using System.Collections.Generic;

public class TaskUI : MonoBehaviour
{
    [Header("UI ELEMENTS")]
    [SerializeField] private UIBlock2D refreshMainBtn;
    [SerializeField] private UIBlock2D refreshExtraBtn;
    [SerializeField] private UIBlock2D completeBtn;
    [Space(10)]
    [SerializeField] private TextBlock mainTaskText;
    [SerializeField] private TextBlock extraTaskText;
    [SerializeField] private TextBlock refreshCostText;
    [SerializeField] private TextBlock completeRewardText;

    [Header("COLORS")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color inProgressColor = Color.yellow;
    [SerializeField] private Color unavailableColor = Color.gray;

    [Header("REFERENCES")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private MoneyManager moneyManager;

    void Start(){
        //Main refresh button
        if(refreshMainBtn != null){
            refreshMainBtn.AddGestureHandler<Gesture.OnPress>(RefreshMainClick);
            refreshMainBtn.AddGestureHandler<Gesture.OnHover>(RefreshHover);
            refreshMainBtn.AddGestureHandler<Gesture.OnUnhover>(RefreshUnhover);
        }
        
        //Extra refresh button
        if(refreshExtraBtn != null){
            refreshExtraBtn.AddGestureHandler<Gesture.OnPress>(RefreshExtraClick);
            refreshExtraBtn.AddGestureHandler<Gesture.OnHover>(RefreshHover);
            refreshExtraBtn.AddGestureHandler<Gesture.OnUnhover>(RefreshUnhover);
        }
        
        //Complete button
        if(completeBtn != null){
            completeBtn.AddGestureHandler<Gesture.OnPress>(CompleteClick);
            completeBtn.AddGestureHandler<Gesture.OnHover>(CompleteHover);
            completeBtn.AddGestureHandler<Gesture.OnUnhover>(CompleteUnhover);
        }
        
        if(taskManager != null) taskManager.OnTasksUpdated += UpdateUI;
        
        UpdateUI();
    }

    void OnDestroy(){
        if(taskManager != null) taskManager.OnTasksUpdated -= UpdateUI;
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
        sb.AppendLine("<size=+100><b>MAIN TASKS</b></size>");

        foreach(TaskData task in mainTasks){
            string description = taskManager.GetTaskDescription(task);
            bool completed = taskManager.IsTaskCompleted(task);
            
            string colorTag = completed ? "<color=green>" : "<color=yellow>";
            string statusIcon = completed ? "V " : "- ";
            
            sb.AppendLine($"{colorTag}{statusIcon}{description}</color>");
        }
        
        mainTaskText.Text = sb.ToString();
    }

    void UpdateExtraTaskDisplay(){
        if(extraTaskText == null || taskManager == null) return;
        
        List<TaskData> extraTasks = taskManager.GetExtraTasks();
        if(extraTasks.Count == 0){
            extraTaskText.Text = "";
            extraTaskText.gameObject.SetActive(false);
            return;
        }
        
        extraTaskText.gameObject.SetActive(true);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<size=+100><b>EXTRA TASKS</b></size>");
        
        foreach(TaskData task in extraTasks){
            string description = taskManager.GetTaskDescription(task);
            bool completed = taskManager.IsTaskCompleted(task);
            
            string colorTag = completed ? "<color=green>" : "<color=yellow>";
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
        if(refreshMainBtn != null){
            bool canAfford = moneyManager != null && moneyManager.CanAfford(taskManager.GetRefreshCost());
            refreshMainBtn.Color = (canAfford) ? normalColor : unavailableColor;
        }
        
        if(refreshExtraBtn != null){
            bool canAfford = moneyManager != null && moneyManager.CanAfford(taskManager.GetRefreshCost());
            refreshExtraBtn.Color = (canAfford) ? normalColor : unavailableColor;
        }
        
        if(completeBtn != null){
            bool anyCompleted = false;
            foreach(TaskData task in taskManager.GetMainTasks()){
                if(taskManager.IsTaskCompleted(task)){
                    anyCompleted = true;
                    break;
                }
            }
            if(!anyCompleted){
                foreach(TaskData task in taskManager.GetExtraTasks()){
                    if(taskManager.IsTaskCompleted(task)){
                        anyCompleted = true;
                        break;
                    }
                }
            }
            
            completeBtn.Color = anyCompleted ? normalColor : unavailableColor;
        }
    }

    void UpdateCostDisplay(){
        if(refreshCostText != null && taskManager != null){
            refreshCostText.Text = $"Refresh: ${taskManager.GetRefreshCost()}";
        }
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
        if(taskManager != null) taskManager.RefreshMainTasks();
    }
    
    void RefreshExtraClick(Gesture.OnPress evt){
        if(taskManager != null) taskManager.RefreshExtraTasks();
    }
    
    void RefreshHover(Gesture.OnHover evt){
    }
    
    void RefreshUnhover(Gesture.OnUnhover evt){
    }
    
    void CompleteClick(Gesture.OnPress evt){
        if(taskManager != null) taskManager.CheckCompletedTask();
    }
    
    void CompleteHover(Gesture.OnHover evt){
    }
    
    void CompleteUnhover(Gesture.OnUnhover evt){
    }
    #endregion
}