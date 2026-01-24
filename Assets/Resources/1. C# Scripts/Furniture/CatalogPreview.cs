using Nova;
using UnityEngine;

public class CatalogPreview : MonoBehaviour
{
    public static CatalogPreview Instance { get; private set; }
    public ItemView furnitureItemVisual;
    private FurnitureItemVisual visual;

    void Awake(){
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start(){
        visual = furnitureItemVisual.Visuals as FurnitureItemVisual;
    }

    public void ShowPreview(Vector3 itemPos, FurnitureData data){
        visual.furnitureIcon.SetImage(data.furnitureIcon);
        visual.furnitureName.Text = data.furnitureName; 
        visual.furniturePrice.Text = data.furnitureCost.ToString();
        visual.furnitureSize.Text = $"{data.furnitureSize.x} X {data.furnitureSize.y}"; 
    }
}