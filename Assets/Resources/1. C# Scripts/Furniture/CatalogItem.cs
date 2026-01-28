using UnityEngine;
using Nova;

[RequireComponent(typeof(Interactable))]
public class CatalogItem : MonoBehaviour
{
    [Header("DATA")]
    public FurnitureData Data;

    private CatalogPreview preview;
    private UIBlock root;
    
    void Start(){
        root = GetComponent<UIBlock>();
        preview = CatalogPreview.Instance;

        root.AddGestureHandler<Gesture.OnPress>(OnClick);
        root.AddGestureHandler<Gesture.OnHover>(OnHover);
    }

    void OnClick(Gesture.OnPress evt){
        if(MoneyManager.Instance.CanAfford(Data.FurnitureCost)){
            BuildSystem.Instance.SetFurnitureData(Data);
            //OpenCatalogButton.Instance.ActivateCatalogPanel(false);
        }
    }
    void OnHover(Gesture.OnHover evt){
        preview.ShowPreview(transform.position, Data, MoneyManager.Instance.CanAfford(Data.FurnitureCost));
    }
}