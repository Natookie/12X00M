using UnityEngine;
using UnityEngine.InputSystem;
using Nova;

[RequireComponent(typeof(Interactable))]
public class OpenCatalogButton : MonoBehaviour
{
    [SerializeField] private UIBlock2D root;
    [SerializeField] private GameObject catalogPanel;

    void Start(){
        root.AddGestureHandler<Gesture.OnPress>(OnClick);
        root.AddGestureHandler<Gesture.OnHover>(OnHover);
        root.AddGestureHandler<Gesture.OnUnhover>(OnUnhover);
    }

    void Update(){
        if(Keyboard.current.escapeKey.wasPressedThisFrame && catalogPanel.activeSelf) catalogPanel.SetActive(false);
    }

    void OnClick(Gesture.OnPress evt){
        if(!catalogPanel.activeSelf) catalogPanel.SetActive(true);
    }
    void OnHover(Gesture.OnHover evt){

    }
    void OnUnhover(Gesture.OnUnhover evt){

    }
}