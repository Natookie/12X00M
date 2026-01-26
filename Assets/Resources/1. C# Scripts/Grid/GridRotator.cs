using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class GridRotator : MonoBehaviour
{
    [Header("ROTATION SETTINGS")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float mouseRotationSensitivity = 0.5f;
    [SerializeField] private float smoothTime = .2f;
    [SerializeField] private bool invertMouseRotation = false;
    [SerializeField] private float mouseInertiaDuration = 0.3f;
    
    [Header("INPUT SETTINGS")]
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.E;
    [SerializeField] private bool useInputSystem = true;
    [SerializeField] private bool allowToUseMouse = false;
    
    [Header("REFERENCES")]
    [SerializeField] private GridManager gridManager;
    
    [Header("EVENTS")]
    public UnityEvent OnRotationStarted;
    public UnityEvent OnRotationStopped;
    
    //Rotation state
    private float targetRotationY;
    private float currentRotationY;
    private float rotationVelocity;
    
    //Mouse drag state
    private bool isMouseDragging = false;
    private Vector2 lastMousePosition;
    private float mouseRotationVelocity;
    private float inertiaTimeRemaining;
    
    //Control flags
    private bool rotationEnabled = true;
    private bool isRotating = false;
    private bool wasRotatingLastFrame = false;
    private bool keyboardRotating = false;
    
    void Start(){
        targetRotationY = transform.eulerAngles.y;
        currentRotationY = targetRotationY;
        
        if(gridManager == null) gridManager = GetComponent<GridManager>();
        CheckAnimationState();
    }
    
    void Update(){
        CheckAnimationState();
        
        wasRotatingLastFrame = isRotating;
        
        if(rotationEnabled){
            keyboardRotating = HandleKeyboardInput();
            if(allowToUseMouse) HandleMouseInput();
        }
        
        UpdateRotation();
        isRotating = Mathf.Abs(rotationVelocity) > .1f || isMouseDragging || keyboardRotating || inertiaTimeRemaining > 0f;
    
        UpdateGridManagerState();
        
        if(!wasRotatingLastFrame && isRotating) OnRotationStarted?.Invoke();
        else if(wasRotatingLastFrame && !isRotating) OnRotationStopped?.Invoke();
    }
    
    bool HandleKeyboardInput(){
        float rotationInput = 0f;
        
        if(useInputSystem){
            if(Keyboard.current.aKey.isPressed) rotationInput -= 1f;
            if(Keyboard.current.dKey.isPressed) rotationInput += 1f;
        }else{
            if(Input.GetKey(rotateLeftKey)) rotationInput -= 1f;
            if(Input.GetKey(rotateRightKey)) rotationInput += 1f;
        }
        
        bool isKeyboardRotating = rotationInput != 0f;
        
        if(isKeyboardRotating){
            float rotationDelta = rotationInput * rotationSpeed * Time.deltaTime;
            targetRotationY += rotationDelta;
        }
        
        return isKeyboardRotating;
    }
    
    void HandleMouseInput(){
        if(useInputSystem) HandleMouseInputSystem();
        else HandleMouseLegacy();
        
        ApplyInertia();
    }
    
    void HandleMouseInputSystem(){
        if(Mouse.current.rightButton.wasPressedThisFrame){
            StartMouseDrag();
        }
        
        if(Mouse.current.rightButton.wasReleasedThisFrame){
            EndMouseDrag();
        }
        
        if(isMouseDragging){
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            
            float rotationDelta = CalculateMouseRotationDelta(mouseDelta.x);
            targetRotationY += rotationDelta;
            
            mouseRotationVelocity = rotationDelta / Time.deltaTime;
            inertiaTimeRemaining = mouseInertiaDuration;
            
            lastMousePosition = currentMousePosition;
        }
    }
    
    void HandleMouseLegacy(){
        if(Input.GetMouseButtonDown(1)){
            StartMouseDrag();
        }
        
        if(Input.GetMouseButtonUp(1)){
            EndMouseDrag();
        }
        
        if(isMouseDragging){
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            
            float rotationDelta = CalculateMouseRotationDelta(mouseDelta.x);
            targetRotationY += rotationDelta;
            
            mouseRotationVelocity = rotationDelta / Time.deltaTime;
            inertiaTimeRemaining = mouseInertiaDuration;
            
            lastMousePosition = currentMousePosition;
        }
    }
    
    float CalculateMouseRotationDelta(float mouseDeltaX){
        float rotationDelta = mouseDeltaX * mouseRotationSensitivity;
        if(invertMouseRotation) rotationDelta *= -1f;
        return rotationDelta;
    }
    
    void ApplyInertia(){
        if(inertiaTimeRemaining > 0f && !isMouseDragging){
            float inertiaProgress = inertiaTimeRemaining / mouseInertiaDuration;
            float rotationDelta = mouseRotationVelocity * Time.deltaTime * inertiaProgress;
            targetRotationY += rotationDelta;
            
            inertiaTimeRemaining -= Time.deltaTime;
            mouseRotationVelocity *= Mathf.Exp(-Time.deltaTime * 5f);
        }
    }
    
    void StartMouseDrag(){
        if(!rotationEnabled) return;
        
        isMouseDragging = true;
        mouseRotationVelocity = 0f;
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
        currentRotationY = Mathf.LerpAngle(
            currentRotationY, 
            targetRotationY, 
            Time.deltaTime / smoothTime
        );
        
        rotationVelocity = Mathf.DeltaAngle(currentRotationY, targetRotationY) / Time.deltaTime;
        
        Vector3 euler = transform.eulerAngles;
        euler.y = currentRotationY;
        transform.eulerAngles = euler;
    }
    
    void CheckAnimationState(){
        if(gridManager != null) rotationEnabled = !gridManager.IsAnimating();
    }
    
    void UpdateGridManagerState(){
        if(gridManager != null) gridManager.SetRotationState(isRotating || isMouseDragging || inertiaTimeRemaining > 0f);
    }
    
    public void EnableRotation(bool enable){
        rotationEnabled = enable;
        
        if(!enable && isMouseDragging){
            EndMouseDrag();
            inertiaTimeRemaining = 0f;
        }
    }
    
    public bool IsRotating() => isRotating || isMouseDragging || keyboardRotating || inertiaTimeRemaining > 0f;
    public float GetCurrentRotation() => currentRotationY;
    public bool RotationStateChangedThisFrame() => wasRotatingLastFrame != isRotating;
    
    void OnDisable(){
        if(isMouseDragging){
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            isMouseDragging = false;
            inertiaTimeRemaining = 0f;
        }
    }
}