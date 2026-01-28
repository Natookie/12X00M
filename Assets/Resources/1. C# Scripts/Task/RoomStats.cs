using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoomStats : MonoBehaviour
{
    [Header("FURNITURE TYPE COUNTS")]
    [SerializeField] private List<FurnitureTypeCountEntry> furnitureTypeCounts = new List<FurnitureTypeCountEntry>();
    
    [Header("INDIVIDUAL FURNITURE COUNTS")]
    [SerializeField] private List<FurnitureCountEntry> furnitureCounts = new List<FurnitureCountEntry>();

    [Header("REFERENCES")]
    [SerializeField] CatalogUI catalogUI;

    [System.Serializable]
    public class FurnitureCountEntry{
        public FurnitureData furniture;
        public int count;
    }

    [System.Serializable]
    public class FurnitureTypeCountEntry{
        public FurnitureType type;
        public int count;
    }

    [Header("CURRENT STATS")]
    [SerializeField] private int charisma;
    [SerializeField] private int comfort;
    [SerializeField] private int functionality;

    public int Charisma => charisma;
    public int Comfort => comfort;
    public int Functionality => functionality;

    public static RoomStats Instance {get; private set;}

    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
        InitializeTypeCounts();
    }

    void InitializeTypeCounts(){
        furnitureTypeCounts.Clear();
        
        FurnitureType[] allTypes = (FurnitureType[])System.Enum.GetValues(typeof(FurnitureType));
        
        foreach(FurnitureType type in allTypes){
            if(type != FurnitureType.None) furnitureTypeCounts.Add(new FurnitureTypeCountEntry { type = type, count = 0 });
        }
    }

    #if UNITY_EDITOR
    [ContextMenu("Populate Furniture Data")]
    public void AutoAssignFurnitureData(){
        furnitureCounts.Clear();
        InitializeTypeCounts();

        string[] guids = AssetDatabase.FindAssets("t:FurnitureData", new[] { "Assets/Resources/4. Furniture Data" });
        
        foreach(string guid in guids){
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FurnitureData data = AssetDatabase.LoadAssetAtPath<FurnitureData>(path);
            if(data != null) furnitureCounts.Add(new FurnitureCountEntry { furniture = data, count = 0 });
        }

        Debug.Log($"Auto-assigned {furnitureCounts.Count} furniture types");
    }
    #endif

    public void AddFurniture(FurnitureData furniture){
        if(furniture == null) return;

        FurnitureType type = furniture.furnitureType;
        foreach(var entry in furnitureTypeCounts){
            if(entry.type == type){
                entry.count++;
                break;
            }
        }

        UpdateIndividualCount(furniture, +1);

        charisma += furniture.CharismaContribution;
        comfort += furniture.ComfortContribution;
        functionality += furniture.FunctionalityContribution;
        catalogUI.UpdateTraitDisplay(charisma, comfort, functionality);
    }

    public void RemoveFurniture(FurnitureData furniture){
        if(furniture == null) return;

        FurnitureType type = furniture.furnitureType;
        
        foreach(var entry in furnitureTypeCounts){
            if(entry.type == type){
                entry.count = Mathf.Max(0, entry.count - 1);
                break;
            }
        }

        UpdateIndividualCount(furniture, -1);

        charisma -= furniture.CharismaContribution;
        comfort -= furniture.ComfortContribution;
        functionality -= furniture.FunctionalityContribution;
        catalogUI.UpdateTraitDisplay(charisma, comfort, functionality);
    }

    void UpdateIndividualCount(FurnitureData furniture, int delta){
        foreach(var entry in furnitureCounts){
            if(entry.furniture == furniture){
                entry.count = Mathf.Max(0, entry.count + delta);
                return;
            }
        }

        furnitureCounts.Add(new FurnitureCountEntry { furniture = furniture, count = delta });
    }

    public int GetFurnitureCount(FurnitureData furniture){
        foreach(var entry in furnitureCounts) if(entry.furniture == furniture) return entry.count;
        return 0;
    }

    public int GetFurnitureCountByType(FurnitureType type){
        foreach(var entry in furnitureTypeCounts) if(entry.type == type) return entry.count;
        return 0;
    }

    public List<FurnitureTypeCountEntry> GetAllTypeCounts(){
        return new List<FurnitureTypeCountEntry>(furnitureTypeCounts);
    }

    public List<FurnitureType> GetPresentFurnitureTypes(){
        List<FurnitureType> presentTypes = new List<FurnitureType>();
        foreach(var entry in furnitureTypeCounts) if(entry.count > 0 && entry.type != FurnitureType.None) presentTypes.Add(entry.type);
        
        return presentTypes;
    }

    public int GetTotalFurnitureCount(){
        int total = 0;
        foreach(var entry in furnitureTypeCounts) if(entry.type != FurnitureType.None) total += entry.count;
        
        return total;
    }

    public void ResetAllStats(){
        charisma = 0;
        comfort = 0;
        functionality = 0;
        
        furnitureCounts.Clear();
        InitializeTypeCounts();
    }
}