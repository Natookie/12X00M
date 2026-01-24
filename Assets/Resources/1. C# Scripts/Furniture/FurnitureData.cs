using UnityEngine;

[CreateAssetMenu(fileName = "New Furniture", menuName = "Furniture/Furniture Item", order = 1)]
public class FurnitureData : ScriptableObject
{
    [Header("BASIC PROPERTY")]
    public string furnitureName;
    public int furnitureCost;
    public Vector2Int furnitureSize = new Vector2Int(1, 1);
    [Space(10)]
    public FurnitureType furnitureType;

    [Header("TRAIT VALUE")]
    [Range(-10, 10)] [SerializeField] private int charismaContribution = 0;
    [Range(-10, 10)] [SerializeField] private int comfortContribution = 0;
    [Range(-10, 10)] [SerializeField] private int functionalityContribution = 0;

    [Header("VISUAL")]
    public Sprite furnitureIcon;
    public GameObject furniturePrefab;

    public string ItemName => furnitureName;
    public int Cost => furnitureCost;
    public Vector2Int Size => furnitureSize;
    public GameObject Prefab => furniturePrefab;
    
    public int CharismaContribution => charismaContribution;
    public int ComfortContribution => comfortContribution;
    public int FunctionalityContribution => functionalityContribution;

    public bool IsValid(){
        if(furniturePrefab == null){
            Debug.LogError($"Furniture '{furnitureName}' is missing furniturePrefab!");
            return false;
        }
        
        if(furnitureSize.x <= 0 || furnitureSize.y <= 0){
            Debug.LogWarning($"Furniture '{furnitureName}' has invalid furnitureSize ({furnitureSize})");
            return false;
        }
        
        return true;
    }
}