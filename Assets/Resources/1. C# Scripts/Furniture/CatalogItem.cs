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
        root.AddGestureHandler<Gesture.OnUnhover>(OnUnhover);
    }

    void OnClick(Gesture.OnPress evt){
    }
    void OnHover(Gesture.OnHover evt){
        preview.ShowPreview(transform.position, Data);
    }
    void OnUnhover(Gesture.OnUnhover evt){
        
    }
}