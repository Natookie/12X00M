using UnityEngine;

[CreateAssetMenu(fileName = "New Furniture", menuName = "Furniture/Furniture Item", order = 1)]
public class FurnitureData : ScriptableObject
{
    [Header("BASIC PROPERTY")]
    public string itemName;
    public int cost;
    public Vector2Int size = new Vector2Int(1, 1);
    public GameObject prefab;

    [Header("TRAIT VALUE")]
    [Range(-10, 10)] [SerializeField] private int charismaContribution = 0;
    [Range(-10, 10)] [SerializeField] private int comfortContribution = 0;
    [Range(-10, 10)] [SerializeField] private int functionalityContribution = 0;

    [Header("VISUAL")]
    public Sprite itemIcon;

    public string ItemName => itemName;
    public int Cost => cost;
    public Vector2Int Size => size;
    public GameObject Prefab => prefab;
    public int CharismaContribution => charismaContribution;
    public int ComfortContribution => comfortContribution;
    public int FunctionalityContribution => functionalityContribution;

    public bool IsValid(){
        if(prefab == null){
            Debug.LogError($"Furniture '{itemName}' is missing prefab!");
            return false;
        }
        
        if(size.x <= 0 || size.y <= 0){
            Debug.LogWarning($"Furniture '{itemName}' has invalid size ({size})");
            return false;
        }
        
        return true;
    }
}