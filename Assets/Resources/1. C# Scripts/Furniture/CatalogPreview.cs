using Nova;
using UnityEngine;

public class CatalogPreview : MonoBehaviour
{
    public static CatalogPreview Instance { get; private set; }
    public ItemView furnitureItemVisual;
    private FurnitureItemVisual visual;

    private string redColor = "#f5464c";
    private string greenColor = "#3ec54b";

    void Awake(){
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start(){
        visual = furnitureItemVisual.Visuals as FurnitureItemVisual;
    }

    public void ShowPreview(Vector3 itemPos, FurnitureData data, bool isAffordable){
        string textColor = (isAffordable) ? greenColor : redColor;

        visual.furnitureIcon.SetImage(data.furnitureIcon);
        visual.furnitureName.Text = data.furnitureName; 
        visual.furniturePrice.Text = $"<color={textColor}>{data.furnitureCost.ToString()}</color>";
        visual.furnitureSize.Text = $"{data.furnitureSize.x} X {data.furnitureSize.y}"; 
    }
}