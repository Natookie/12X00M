using UnityEngine;
using UnityEngine.InputSystem;

public class GridRotator : MonoBehaviour
{
    [Header("ROTATION SETTINGS")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float mouseRotationSpeed = 2f;
    [SerializeField] private float smoothTime = .2f;
    [SerializeField] private bool invertMouseRotation = false;
    
    [Header("INPUT SETTINGS")]
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.A;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.D;
    [SerializeField] private bool useInputSystem = true;
    
    [Header("REFERENCES")]
    [SerializeField] private GridManager gridManager;
    
    //Rotation state
    private float targetRotationY;
    private float currentRotationY;
    private float rotationVelocity;
    
    //Mouse drag state
    private bool isMouseDragging = false;
    private Vector2 lastMousePosition;
    
    //Control flags
    private bool rotationEnabled = true;
    private bool isRotating = false;
    
    void Start(){
        targetRotationY = transform.eulerAngles.y;
        currentRotationY = targetRotationY;
        
        if(gridManager == null) gridManager = GetComponent<GridManager>();
        CheckAnimationState();
    }
    
    void Update(){
        CheckAnimationState();
        
        if(rotationEnabled){
            HandleKeyboardInput();
            HandleMouseInput();
        }
        
        UpdateRotation();
        isRotating = Mathf.Abs(rotationVelocity) > .1f || isMouseDragging;
        
        UpdateGridManagerState();
    }
    
    void CheckAnimationState(){
        if(gridManager != null) rotationEnabled = !gridManager.IsAnimating();
    }
    
    void UpdateGridManagerState(){
        if(gridManager != null) gridManager.SetRotationState(isRotating || isMouseDragging);
    }
    
    void HandleKeyboardInput(){
        float rotationInput = 0f;
        
        if(useInputSystem){
            if(Keyboard.current.aKey.isPressed) rotationInput -= 1f;
            if(Keyboard.current.dKey.isPressed) rotationInput += 1f;
        }else{
            if(Input.GetKey(rotateLeftKey)) rotationInput -= 1f;
            if(Input.GetKey(rotateRightKey)) rotationInput += 1f;
        }
        
        if(rotationInput != 0f){
            float rotationDelta = rotationInput * rotationSpeed * Time.deltaTime;
            targetRotationY += rotationDelta;
        }
    }
    
    void HandleMouseInput(){
        if(useInputSystem) HandleMouseInputSystem();
        else HandleMouseLegacy();
    }
    
    void HandleMouseInputSystem(){
        if(Mouse.current.rightButton.wasPressedThisFrame) StartMouseDrag();
        if(Mouse.current.rightButton.wasReleasedThisFrame) EndMouseDrag();
        
        if(isMouseDragging){
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            
            if(mouseDelta.magnitude > .1f){
                float rotationDelta = mouseDelta.x * mouseRotationSpeed * Time.deltaTime;
                
                if(invertMouseRotation) rotationDelta *= -1f;
                targetRotationY += rotationDelta;
            }
            
            lastMousePosition = currentMousePosition;
        }
    }
    
    void HandleMouseLegacy(){
        if(Input.GetMouseButtonDown(1)) StartMouseDrag();
        if(Input.GetMouseButtonUp(1)) EndMouseDrag();
        
        if(isMouseDragging){
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            
            if(mouseDelta.magnitude > .1f){
                float rotationDelta = mouseDelta.x * mouseRotationSpeed * Time.deltaTime;
                
                if(invertMouseRotation) rotationDelta *= -1f;
                targetRotationY += rotationDelta;
            }
            
            lastMousePosition = currentMousePosition;
        }
    }
    
    void StartMouseDrag(){
        if(!rotationEnabled) return;
        
        isMouseDragging = true;
        lastMousePosition = (useInputSystem) ? 
            Mouse.current.position.ReadValue() : 
            Input.mousePosition;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }
    
    void EndMouseDrag(){
        isMouseDragging = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    void UpdateRotation(){
        currentRotationY = Mathf.SmoothDampAngle(
            currentRotationY, 
            targetRotationY, 
            ref rotationVelocity, 
            smoothTime
        );
        
        Vector3 euler = transform.eulerAngles;
        euler.y = currentRotationY;
        transform.eulerAngles = euler;
    }
    
    public void EnableRotation(bool enable){
        rotationEnabled = enable;
        
        if(!enable && isMouseDragging) EndMouseDrag();
    }
    
    public bool IsRotating() => isRotating || isMouseDragging;
    public float GetCurrentRotation() => currentRotationY;
    
    void OnDisable(){
        if(isMouseDragging){
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            isMouseDragging = false;
        }
    }
}