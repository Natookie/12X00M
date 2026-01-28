using UnityEngine;
using UnityEngine.InputSystem;

public class RefreshFeedbackUI : MonoBehaviour
{
    [SerializeField] private float yOffset = -50f;
    private RectTransform rectTransform;
    private Mouse mouse;
    
    void Start(){
        rectTransform = GetComponent<RectTransform>();
        if(rectTransform == null) Debug.LogError("Nein");
        
        mouse = Mouse.current;
    }
    
    void Update(){
        if(mouse == null) return;
        
        Vector2 mousePos = mouse.position.ReadValue();
        mousePos.y += yOffset;
        rectTransform.position = mousePos;
    }
}