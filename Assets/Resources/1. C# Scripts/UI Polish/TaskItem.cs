using UnityEngine;
using Nova;

[RequireComponent(typeof(Interactable))]
public class TaskItem : MonoBehaviour
{
    public ItemView taskItemView;
    private TaskItemVisual visual;
    public TaskData taskData;

    private UIBlock2D root;
    
    void Awake(){
        visual = taskItemView.Visuals as TaskItemVisual;
        if(visual == null) Debug.LogError("taskItemView.Visuals is not a TaskItemVisual!");
    }

    void Start(){
        root = visual.statusBlock.GetComponent<UIBlock2D>();
        if(root == null) return;

        root.AddGestureHandler<Gesture.OnPress>(TaskClick);
        root.AddGestureHandler<Gesture.OnHover>(TaskHover);
        root.AddGestureHandler<Gesture.OnUnhover>(TaskUnhover);
    }
    
    public void Initialize(TaskData task, bool isCompleted, int currentProgress, int targetProgress, bool isExtraTask = false){
        taskData = task;
        UpdateStatus(isCompleted, currentProgress, targetProgress, isExtraTask);
    }
    
    public void UpdateStatus(bool isCompleted, int currentProgress, int targetProgress, bool isExtraTask = false){
        if(visual == null) return;
        if(taskData == null) return; //Di initialize, ini bakal null, gw gatau cara fix nya, dan males buat fix, tapi keknya ga ngaruh

        visual.statusBlock.Color = (isCompleted) ? visual.completedColor : visual.pendingColor;
        visual.progressText.Text = $"[{currentProgress}/{targetProgress}]";
        
        string fullDesc = TaskManager.Instance.GetTaskDescription(taskData);
        int bracketEnd = fullDesc.IndexOf(']');
        string cleanDesc = (bracketEnd > 0) ? fullDesc.Substring(bracketEnd + 2) : fullDesc;
        
        if(isExtraTask){
            cleanDesc += $" \nBonus: <color=#3ec54b>+${taskData.rewardCoins}</color>";
            visual.descriptionText.Text = cleanDesc;
            return;
        }
        visual.descriptionText.Text = cleanDesc;
    }

    void TaskClick(Gesture.OnPress evt) => TaskPersuade.Instance.PersuadeGrandma(taskData);
    void TaskHover(Gesture.OnHover evt) => TaskPersuade.Instance.ShowPersuadeFeedback(taskData); 
    void TaskUnhover(Gesture.OnUnhover evt) => TaskPersuade.Instance.HidePersuadeFeedback();
}