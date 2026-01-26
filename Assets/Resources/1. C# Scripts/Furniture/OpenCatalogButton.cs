using UnityEngine;
using UnityEngine.InputSystem;
using Nova;

[RequireComponent(typeof(Interactable))]
public class OpenCatalogButton : MonoBehaviour
{
    [SerializeField] private UIBlock2D root;
    [SerializeField] private GameObject catalogPanel;

    public static OpenCatalogButton Instance {get; private set;}

    void Awake(){
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start(){
        root.AddGestureHandler<Gesture.OnPress>(OnClick);
        root.AddGestureHandler<Gesture.OnHover>(OnHover);
        root.AddGestureHandler<Gesture.OnUnhover>(OnUnhover);
    }

    void OnClick(Gesture.OnPress evt){
        if(!catalogPanel.activeSelf) ActivateCatalogPanel(true);
    }
    void OnHover(Gesture.OnHover evt){

    }
    void OnUnhover(Gesture.OnUnhover evt){

    }

    public bool IsPanelActive() => catalogPanel.activeSelf;
    public void ActivateCatalogPanel(bool type) => catalogPanel.SetActive(type);   
}