using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TaskManager : MonoBehaviour
{
    [Header("TASK CONFIGURATION")]
    [SerializeField] private List<TaskData> allTasks;
    [SerializeField] private RoomStats roomStats;
    [SerializeField] private int maxMainTasks = 3;  //1 to 4
    [SerializeField] private int maxExtraTasks = 2; //0 to 2
    
    [Header("REFERENCES")]
    [SerializeField] private TaskUI taskUI;
    [SerializeField] private MoneyManager moneyManager;
    [Space(10)]
    [SerializeField] private int refreshCost = 50;

    private List<TaskData> mainTasks = new List<TaskData>();
    private List<TaskData> extraTasks = new List<TaskData>();

    public System.Action OnTasksUpdated;
    public static TaskManager Instance {get; private set;}

    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
    }

    void Start(){
        AssignRandomTasks();
    }

    public void OnFurnitureUpdated(){
        if(taskUI != null) taskUI.UpdateUI();
    }

    public void AssignRandomTasks(){
        mainTasks.Clear();
        extraTasks.Clear();
        
        List<TaskData> availableTasks = new List<TaskData>(allTasks);
        
        for(int i = 0; i < availableTasks.Count; i++){
            int randomIndex = Random.Range(i, availableTasks.Count);
            (availableTasks[i], availableTasks[randomIndex]) = (availableTasks[randomIndex], availableTasks[i]);
        }
        
        int mainCount = Mathf.Min(maxMainTasks, availableTasks.Count);
        for(int i = 0; i < mainCount; i++){
            availableTasks[i].InitializeProgress(roomStats);
            mainTasks.Add(availableTasks[i]);
        }
        
        if(availableTasks.Count > mainCount){
            int extraCount = Mathf.Min(maxExtraTasks, availableTasks.Count - mainCount);
            for(int i = mainCount; i < mainCount + extraCount; i++){
                availableTasks[i].InitializeProgress(roomStats);
                extraTasks.Add(availableTasks[i]);
            }
        }
        
        OnTasksUpdated?.Invoke();
        if(taskUI != null) taskUI.UpdateUI();
    }

    public void RefreshMainTasks(){
        if(moneyManager != null && !moneyManager.CanAfford(refreshCost)) return;
        if(moneyManager != null) moneyManager.AddMoney(-refreshCost);
        
        RefreshTaskList(mainTasks, maxMainTasks);
        
        OnTasksUpdated?.Invoke();
        if(taskUI != null) taskUI.UpdateUI();
    }

    public void RefreshExtraTasks(){
        if(moneyManager != null && !moneyManager.CanAfford(refreshCost)) return;
        if(moneyManager != null) moneyManager.AddMoney(-refreshCost);
        
        RefreshTaskList(extraTasks, maxExtraTasks);
        
        OnTasksUpdated?.Invoke();
        if(taskUI != null) taskUI.UpdateUI();
    }

    void RefreshTaskList(List<TaskData> taskList, int maxTasks){
        taskList.Clear();
        
        List<TaskData> availableTasks = new List<TaskData>();
        foreach(TaskData task in allTasks){
            if(!mainTasks.Contains(task) && !extraTasks.Contains(task)){
                availableTasks.Add(task);
            }
        }
        
        for(int i = 0; i < availableTasks.Count; i++){
            int randomIndex = Random.Range(i, availableTasks.Count);
            (availableTasks[i], availableTasks[randomIndex]) = (availableTasks[randomIndex], availableTasks[i]);
        }
        
        int count = Mathf.Min(maxTasks, availableTasks.Count);
        for(int i = 0; i < count; i++){
            availableTasks[i].InitializeProgress(roomStats);
            taskList.Add(availableTasks[i]);
        }
    }

    public void EvaluateAllTasksAtRoundEnd(){
        int completedCount = 0;
        int failedCount = 0;
        
        //Main task completion -> Add trust / Punish
        for(int i = mainTasks.Count - 1; i >= 0; i--){
            TaskData task = mainTasks[i];
            
            if(IsTaskCompleted(task)){
                TrustManager.Instance?.AddTrust(1);
                completedCount++;
            }else{
                TrustManager.Instance?.AddTrust(-1);
                failedCount++;
            }
            
            mainTasks.RemoveAt(i);
        }
        
        //Main task completion -> Add money / No punishment
        for(int i = extraTasks.Count - 1; i >= 0; i--){
            TaskData task = extraTasks[i];
            
            if(IsTaskCompleted(task)) MoneyManager.Instance?.AddMoney(task.rewardCoins);
            extraTasks.RemoveAt(i);
        }
        
        AssignRandomTasks();
        
        Debug.Log($"Round ended: {completedCount} completed (+{completedCount} trust), {failedCount} failed (-{failedCount} trust)");
        OnTasksUpdated?.Invoke();
    }

    public bool HasCompletableTasks(){
        foreach(TaskData task in mainTasks) if(IsTaskCompleted(task)) return true;
        foreach(TaskData task in extraTasks) if(IsTaskCompleted(task)) return true;
        
        return false;
    }

    void AutoRefillEmptySlots(){
        if(mainTasks.Count < maxMainTasks){
            int needed = maxMainTasks - mainTasks.Count;
            FillEmptySlots(mainTasks, needed);
        }
        
        if(extraTasks.Count < maxExtraTasks){
            int needed = maxExtraTasks - extraTasks.Count;
            FillEmptySlots(extraTasks, needed);
        }
        
        if(taskUI != null) taskUI.UpdateUI();
    }

    void FillEmptySlots(List<TaskData> taskList, int count){
        List<TaskData> availableTasks = new List<TaskData>();
        foreach(TaskData task in allTasks){
            if(!mainTasks.Contains(task) && !extraTasks.Contains(task)){
                availableTasks.Add(task);
            }
        }
        
        for(int i = 0; i < availableTasks.Count; i++){
            int randomIndex = Random.Range(i, availableTasks.Count);
            (availableTasks[i], availableTasks[randomIndex]) = (availableTasks[randomIndex], availableTasks[i]);
        }
        
        int addCount = Mathf.Min(count, availableTasks.Count);
        for(int i = 0; i < addCount; i++) taskList.Add(availableTasks[i]);
    }

    public List<TaskData> GetMainTasks() => mainTasks;
    public List<TaskData> GetExtraTasks() => extraTasks;
    public bool IsTaskCompleted(TaskData task) => task.IsCompleted(roomStats);
    public string GetTaskDescription(TaskData task) => task.GetDescription(roomStats);
    public int GetRefreshCost() => refreshCost;

    #region AUTO POPULATE
    #if UNITY_EDITOR
    [ContextMenu("Populate All Tasks")]
    public void PopulateAllTasksFromResources(){
        allTasks.Clear();
        
        string[] traitPaths = Directory.GetFiles("Assets/Resources/5. Task Data/Trait", "*.asset", SearchOption.AllDirectories);
        string[] furniturePaths = Directory.GetFiles("Assets/Resources/5. Task Data/Furniture Count", "*.asset", SearchOption.AllDirectories);
        
        Debug.Log($"Found {traitPaths.Length} trait tasks and {furniturePaths.Length} furniture tasks");
        foreach(string path in traitPaths){
            TaskData task = AssetDatabase.LoadAssetAtPath<TaskData>(path);
            if(task != null) allTasks.Add(task);
        }
        
        foreach(string path in furniturePaths){
            TaskData task = AssetDatabase.LoadAssetAtPath<TaskData>(path);
            if(task != null) allTasks.Add(task);
        }
        
        Debug.Log($"Total tasks loaded: {allTasks.Count}");
        EditorUtility.SetDirty(this);
    }

    [ContextMenu("Clear All Tasks")]
    public void ClearAllTasks(){
        allTasks.Clear();
        Debug.Log("Cleared all tasks");
        EditorUtility.SetDirty(this);
    }
    #endif
    #endregion
}