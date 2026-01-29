using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FurnitureController : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private FurnitureData furnitureData;
    [SerializeField] private Renderer furnitureRenderer;
    [SerializeField] private Collider furnitureCollider;
    
    [Header("ANIMATION SETTINGS")]
    [SerializeField] private float spawnScaleDuration = 0.3f;
    [SerializeField] private AnimationCurve spawnScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float spawnHeightOffset = 1f;
    [SerializeField] private float hoverScaleAmount = 1.05f;
    [SerializeField] private float hoverScaleDuration = 0.2f;
    
    [Header("SELECTION SETTINGS")]
    [SerializeField] private Color selectedColor = Color.red;
    [SerializeField] private Color hoverColor = Color.yellow;

    [Header("AUDIO SETTINGS")]
    [SerializeField] private string furnitureSpawnSFXKey = "furniturePlace";
    
    private bool isSelected = false;
    private Color originalColor;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 lastClickPosition;
    private Material furnitureMaterial;

    private Vector2Int gridPos;
    
    void Start(){
        InitializeComponents();
        PlaySpawnAnimation();

        gameObject.layer = LayerMask.NameToLayer("Furniture");
    }
    
    void InitializeComponents(){
        if(furnitureRenderer == null) furnitureRenderer = GetComponent<Renderer>();
        if(furnitureCollider == null) furnitureCollider = GetComponent<Collider>();
        
        if(furnitureRenderer != null){
            furnitureMaterial = furnitureRenderer.material;
            originalColor = furnitureMaterial.color;
        }
        
        originalScale = transform.localScale;
        originalPosition = transform.position;
        
        transform.localScale = Vector3.zero;
    }
    
    void PlaySpawnAnimation(){
        StartCoroutine(SpawnAnimationCoroutine());
    }
    
    IEnumerator SpawnAnimationCoroutine(){
        AudioManager.Instance.PlaySFX(furnitureSpawnSFXKey);

        Vector3 startPos = originalPosition;
        startPos.y += spawnHeightOffset;
        transform.position = startPos;
        
        float elapsedTime = 0f;
        
        while(elapsedTime < spawnScaleDuration){
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / spawnScaleDuration;
            float curveValue = spawnScaleCurve.Evaluate(t);
            
            transform.localScale = originalScale * curveValue;
            
            Vector3 currentPos = Vector3.Lerp(startPos, originalPosition, curveValue);
            transform.position = currentPos;
            
            yield return null;
        }
        
        transform.localScale = originalScale;
        transform.position = originalPosition;
    }
    
    void Update(){
        if(!BuildSystem.Instance.IsInBuildMode) HandleSelectionInput();
    }
    
    void HandleSelectionInput(){
        if(Mouse.current.leftButton.wasPressedThisFrame){
            lastClickPosition = Mouse.current.position.ReadValue();

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            
            int furnitureLayerMask = 1 << 7;
            
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, furnitureLayerMask)){
                if(hit.collider.gameObject == gameObject){
                    if(!isSelected) SelectFurniture();
                    else SellFurniture();
                }else DeselectFurniture();
            }else DeselectFurniture();
        }
        
        if(Mouse.current.rightButton.wasPressedThisFrame && isSelected) DeselectFurniture();
        if(Keyboard.current.escapeKey.wasPressedThisFrame && isSelected) DeselectFurniture();
    }
    
    void OnMouseEnter(){
        if(isSelected) return;

        GridTileInfo[] allGridTiles = FindObjectsByType<GridTileInfo>(FindObjectsSortMode.None);
        foreach(GridTileInfo tile in allGridTiles){
            Collider col = tile.GetComponent<Collider>();
            if(col != null) col.enabled = false;
        }

        StartCoroutine(HoverAnimation(true));
    }
    
    void OnMouseExit(){
        if(isSelected) return;

        GridTileInfo[] allGridTiles = FindObjectsByType<GridTileInfo>(FindObjectsSortMode.None);
        foreach(GridTileInfo tile in allGridTiles){
            Collider col = tile.GetComponent<Collider>();
            if(col != null) col.enabled = true;
        }

        StartCoroutine(HoverAnimation(false));
    }
    
    IEnumerator HoverAnimation(bool hover){
        float startScale = transform.localScale.x / originalScale.x;
        float targetScale = hover ? hoverScaleAmount : 1f;
        float elapsedTime = 0f;
        
        while(elapsedTime < hoverScaleDuration){
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / hoverScaleDuration;
            float currentScale = Mathf.Lerp(startScale, targetScale, t);
            transform.localScale = originalScale * currentScale;
            yield return null;
        }
        
        transform.localScale = originalScale * targetScale;
    }
    
    void SelectFurniture(){
        isSelected = true;
        if(furnitureMaterial != null) furnitureMaterial.color = selectedColor;
        transform.localScale = originalScale * 1.1f;
    }
    
    void DeselectFurniture(){
        isSelected = false;
        
        if(furnitureMaterial != null) furnitureMaterial.color = originalColor;
        transform.localScale = originalScale;
    }
    
    void SellFurniture(){
        if(furnitureData == null) return;
        
        int sellPrice = CalculateSellPrice();
        if(MoneyManager.Instance != null) MoneyManager.Instance.AddMoney(sellPrice);
        if(MoneyUI.Instance != null) MoneyUI.Instance.ShowMoneyFeedback(sellPrice, lastClickPosition);

        if(BuildSystem.Instance != null) BuildSystem.Instance.RemoveFurniture(gridPos);
        if(RoomStats.Instance != null) RoomStats.Instance.RemoveFurniture(furnitureData);
        if(TaskManager.Instance != null) TaskManager.Instance.OnFurnitureUpdated();
        Destroy(gameObject);
    }
    
    int CalculateSellPrice(){
        if(furnitureData == null) return 0;
        return Mathf.RoundToInt(furnitureData.FurnitureCost * 0.4f);
    }
    
    public void SetFurnitureData(FurnitureData data) => furnitureData = data;
    public void SetGridPosition(Vector2Int pos) => gridPos = pos;
    public FurnitureData GetFurnitureData() => furnitureData;
    public bool IsSelected() => isSelected;
    
}