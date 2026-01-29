using Nova;
using UnityEngine;

public class CatalogPreview : MonoBehaviour
{
    public static CatalogPreview Instance { get; private set; }
    [SerializeField] private UIBlock2D catalogBlockArea;
    [SerializeField] private Transform dialogueBlockArea;
    public ItemView furnitureItemVisual;
    
    private FurnitureItemVisual visual;
    private Vector3 originalPos;

    private string redColor = "#f5464c";
    private string greenColor = "#3ec54b";

    void Awake(){
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start(){
        visual = furnitureItemVisual.Visuals as FurnitureItemVisual;
        originalPos = transform.position;

        // catalogBlockArea.AddGestureHandler<Gesture.OnUnhover>(CatalogUnhover);
        // catalogBlockArea.AddGestureHandler<Gesture.OnHover>(CatalogHover);
    }

    void Update(){
        if(OpenCatalogButton.Instance.IsPanelActive()){
            transform.position = originalPos;
            dialogueBlockArea.position = new Vector3(0, -1000, 0);
        }else{
            transform.position = new Vector3(0, -1000, 0);
            dialogueBlockArea.position = originalPos;
        }
    }

    // void CatalogHover(Gesture.OnHover evt){}
    // void CatalogUnhover(Gesture.OnUnhover evt){}

    public void ShowPreview(Vector3 itemPos, FurnitureData data, bool isAffordable){
        if(BuildSystem.Instance.IsInBuildMode) return;

        visual.furnitureIcon.SetImage(data.furnitureIcon);
        visual.furnitureName.Text = data.furnitureName; 
        visual.furniturePrice.Text = $"<color={(isAffordable ? greenColor : redColor)}>{data.furnitureCost}</color>";
        visual.furnitureSize.Text = $"{data.furnitureSize.x} X {data.furnitureSize.y}"; 
        
        visual.furnitureCharisma.Text = FormatTraitValue(data.CharismaContribution);
        visual.furnitureComfort.Text = FormatTraitValue(data.ComfortContribution);
        visual.furnitureFunctionality.Text = FormatTraitValue(data.FunctionalityContribution);
    }

    private string FormatTraitValue(int value){
        string sign = value > 0 ? "+" : value < 0 ? "-" : "+";
        string color = value < 0 ? redColor : greenColor;
        return $"<color={color}>{sign}{Mathf.Abs(value)}</color>";
    }
}