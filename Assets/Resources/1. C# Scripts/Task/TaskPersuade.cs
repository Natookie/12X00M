using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Nova;

public class TaskPersuade : MonoBehaviour
{
    public static TaskPersuade Instance {get; private set;}

    [Header("REFERENCES")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TrustManager trustManager;
    [Space(10)]
    [SerializeField] private CanvasGroup persuadeFeedback;
    [SerializeField] private TextMeshProUGUI feedbackTextTMP;

    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
    }

    public void PersuadeGrandma(TaskData currentTask){
        if(currentTask == null || taskManager == null || trustManager == null) return;
        
        int trustCost = currentTask.GetTrustCost(taskManager.RStats);
        currentTask.ResetCumulativeProgress(taskManager.RStats);
        if(trustCost > 0){
            trustManager.AddTrust(-trustCost);
            taskManager.OnTasksUpdated?.Invoke();
            Debug.Log($"Lost {trustCost} trust.");
        }else Debug.Log("No progress to reset");
    }

    public void ShowPersuadeFeedback(TaskData currentTask){
        persuadeFeedback.alpha = 1f;

        if(currentTask == null || taskManager == null || trustManager == null) return;
        int trustCost = currentTask.GetTrustCost(taskManager.RStats);
        string textHeader = "<color=#d86830>[Persuade Grandma]</color>";
        string text = (trustCost > 0 ) ? 
        $"{textHeader}\nReset for: <color=#f5464c>-{trustCost} Trust</color>" : 
        $"{textHeader}\nNo progress to reset";

        if(feedbackTextTMP != null) feedbackTextTMP.text = text;
    }
    public void HidePersuadeFeedback() => persuadeFeedback.alpha = 0f;
}