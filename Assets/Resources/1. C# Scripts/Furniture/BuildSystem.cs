using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class BuildSystem : MonoBehaviour
{
    public static BuildSystem Instance { get; private set; }
    
    [Header("REFERENCES")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridHighlight gridHighlight;
    [SerializeField] private RoomStats roomStats;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private MoneyUI moneyUI;
    [SerializeField] private TaskManager taskManager;
    [Space(10)]
    [SerializeField] private Transform furnitureParent;

    [Header("VISUAL")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    [SerializeField] private float placementYOffset = 0f;
    
    private FurnitureData currentSelectedFurniture;
    private GameObject placementPreview;
    private bool furnitureIsOnPending = false;
    private Vector2Int pendingGridPosition;
    private Vector3 lastClickedMousePosition;

    private bool isPendingPlacementValid = false;
    private bool isMouseOverGrid = false;
    private Vector2Int lastValidGridPosition;
    private bool hasValidLastPosition = false;
    
    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, FurnitureInstance> placedFurniture = new Dictionary<Vector2Int, FurnitureInstance>();
    
    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
        
        if(gridManager == null) gridManager = FindAnyObjectByType<GridManager>();
    }
    
    void Update(){
        if(gridManager == null || gridManager.IsGridBusy()) return;
        
        HandleMouseInput();
        
        if(!furnitureIsOnPending && placementPreview != null){
            UpdatePlacementPreviewPosition();
        }
    }
    
    void HandleMouseInput(){
        if(Mouse.current.leftButton.wasPressedThisFrame){
            if(currentSelectedFurniture != null && !furnitureIsOnPending && isMouseOverGrid) ConfirmPosition();
            else if(furnitureIsOnPending && isPendingPlacementValid) PlaceFurniture();
        }
        
        if(Mouse.current.rightButton.wasPressedThisFrame) CancelPlacement();
        if(Keyboard.current.rKey.wasPressedThisFrame && currentSelectedFurniture != null) RotateFurniture();
        if(Keyboard.current.escapeKey.wasPressedThisFrame) CancelPlacement();
    }
    
    public void SetFurnitureData(FurnitureData data){
        if(data == null || !data.IsValid()){
            Debug.LogWarning("Invalid furniture data provided");
            return;
        }
        
        CancelPlacement();
        
        currentSelectedFurniture = data;
        furnitureIsOnPending = false;
        hasValidLastPosition = false;
        
        CreatePlacementPreview();
        gridHighlight.HighlightUnoccupiedTiles();
    }
    
    void CreatePlacementPreview(){
        if(currentSelectedFurniture?.FurniturePrefab == null) return;
        
        if(placementPreview != null) Destroy(placementPreview);
        placementPreview = Instantiate(currentSelectedFurniture.FurniturePrefab, furnitureParent);
        placementPreview.name = $"{currentSelectedFurniture.FurnitureName}_Preview";
        
        SetPreviewMaterials(placementPreview, validPlacementMaterial);
        
        foreach(Collider collider in placementPreview.GetComponentsInChildren<Collider>()){
            collider.enabled = false;
        }
        foreach(MonoBehaviour script in placementPreview.GetComponentsInChildren<MonoBehaviour>()){
            script.enabled = false;
        }
    }
    
    void SetPreviewMaterials(GameObject obj, Material material){
        foreach(Renderer renderer in obj.GetComponentsInChildren<Renderer>()){
            Material[] materials = new Material[renderer.materials.Length];
            for(int i = 0; i < materials.Length; i++) materials[i] = material;
            renderer.materials = materials;
        }
    }

    Vector2Int GetRotatedSize(Quaternion rotation, Vector2Int originalSize){
        int rotationAngle = Mathf.RoundToInt(rotation.eulerAngles.y) % 360;
        if(rotationAngle == 90 || rotationAngle == 270){
            return new Vector2Int(originalSize.y, originalSize.x);
        }
        return originalSize;
    }

    void UpdatePlacementPreviewPosition(){
        if(placementPreview == null || gridManager == null) return;
        
        Vector3? mouseGridPos = GetMouseGridPosition();
        isMouseOverGrid = mouseGridPos.HasValue;
        
        if(isMouseOverGrid){
            Vector2Int gridPos = WorldToGridPosition(mouseGridPos.Value);
            Vector2Int adjustedPos = AdjustPositionForFurnitureSize(gridPos);
            
            Vector2Int currentSize = GetRotatedSize(placementPreview.transform.rotation, currentSelectedFurniture.FurnitureSize);
            bool isValid = IsPlacementValid(adjustedPos, currentSize);
            List<Vector2Int> cellsToHighlight = GetOccupiedCells(adjustedPos, currentSize);
            gridManager.HighlightTiles(cellsToHighlight, isValid);

            Vector3 worldPos = GetTileWorldPosition(adjustedPos, currentSize);
            placementPreview.transform.position = worldPos;
            
            lastValidGridPosition = adjustedPos;
            hasValidLastPosition = true;
            
            Material currentMaterial = (isValid) ? validPlacementMaterial : invalidPlacementMaterial;
            SetPreviewMaterials(placementPreview, currentMaterial);
        }else if(hasValidLastPosition){
            Vector2Int currentSize = GetRotatedSize(placementPreview.transform.rotation, currentSelectedFurniture.FurnitureSize);
            Vector3 worldPos = GetTileWorldPosition(lastValidGridPosition, currentSize);
            placementPreview.transform.position = worldPos;
            
            SetPreviewMaterials(placementPreview, invalidPlacementMaterial);
        }else placementPreview.transform.position = Vector3.zero;
    }

    Vector2Int AdjustPositionForFurnitureSize(Vector2Int gridPos){
        if(currentSelectedFurniture == null) return gridPos;
        
        Vector2Int size = GetRotatedSize(
            (placementPreview != null) ? placementPreview.transform.rotation : Quaternion.identity, 
            currentSelectedFurniture.FurnitureSize
        );
        
        int halfWidth = gridManager.GridWidth / 2;
        int halfLength = gridManager.GridLength / 2;
        
        int maxOffsetX = (size.x % 2 == 0) ? (size.x / 2) - 1 : (size.x / 2);
        int maxOffsetY = (size.y % 2 == 0) ? (size.y / 2) - 1 : (size.y / 2);
        
        int minX = -halfWidth + maxOffsetX;
        int maxX = halfWidth - maxOffsetX;
        int minY = -halfLength + maxOffsetY;
        int maxY = halfLength - maxOffsetY;
        
        Vector2Int adjustedPos = gridPos;
        adjustedPos.x = Mathf.Clamp(adjustedPos.x, minX, maxX);
        adjustedPos.y = Mathf.Clamp(adjustedPos.y, minY, maxY);
        
        return adjustedPos;
    }
    
    Vector3 GetTileWorldPosition(Vector2Int gridPos, Vector2Int size, GameObject furnitureObject = null){
        if(gridManager == null) return Vector3.zero;
        
        Vector3 worldPos = CalculatePositionFromGrid(gridPos, size);
        
        GameObject anyTile = gridManager.GetTileAtLocalPosition(gridPos);
        if(anyTile != null){
            Renderer tileRenderer = anyTile.GetComponent<Renderer>();
            if(tileRenderer != null){
                float tileTopY = anyTile.transform.position.y + (tileRenderer.bounds.size.y * 0.5f);
                worldPos.y = tileTopY;
                
                if(furnitureObject != null){
                    Renderer furnitureRenderer = furnitureObject.GetComponent<Renderer>();
                    if(furnitureRenderer == null) furnitureRenderer = furnitureObject.GetComponentInChildren<Renderer>();
                    
                    if(furnitureRenderer != null){
                        float furnitureHeight = furnitureRenderer.bounds.size.y;
                        worldPos.y += furnitureHeight * 0.5f;
                    }
                }
            }
        }
        
        worldPos.y += placementYOffset;
        
        return worldPos;
    }

    Vector3 CalculatePositionFromGrid(Vector2Int gridPos, Vector2Int size){
        Vector3 center = gridManager.transform.position;
        float gridSpacing = gridManager.GridSpacing;
        
        float offsetX = (gridManager.GridWidth % 2 == 0) ? gridSpacing * 0.5f : 0f;
        float offsetZ = (gridManager.GridLength % 2 == 0) ? gridSpacing * 0.5f : 0f;
        
        Vector3 worldPos = center;
        worldPos.x += (gridPos.x * gridSpacing) + offsetX;
        worldPos.z += (gridPos.y * gridSpacing) + offsetZ;
        
        float sizeOffsetX = 0f;
        float sizeOffsetZ = 0f;
        
        if(size.x > 1){
            sizeOffsetX = (size.x % 2 == 0) ? (size.x / 2 - 0.5f) * gridSpacing : (size.x - 1) * gridSpacing * 0.5f;
        }
        if(size.y > 1){
            sizeOffsetZ = (size.y % 2 == 0) ? (size.y / 2 - 0.5f) * gridSpacing : (size.y - 1) * gridSpacing * 0.5f;
        }
        
        worldPos.x += sizeOffsetX;
        worldPos.z += sizeOffsetZ;
        
        return worldPos;
    }
    
    void ConfirmPosition(){
        if(currentSelectedFurniture == null || placementPreview == null) return;
        
        Vector3? mouseGridPos = GetMouseGridPosition();
        if(!mouseGridPos.HasValue) return;
        
        pendingGridPosition = WorldToGridPosition(mouseGridPos.Value);
        
        Vector2Int currentSize = GetRotatedSize(placementPreview.transform.rotation, currentSelectedFurniture.FurnitureSize);
        isPendingPlacementValid = IsPlacementValid(pendingGridPosition, currentSize);
        if(!isPendingPlacementValid) return;

        Vector3 worldPos = GetTileWorldPosition(pendingGridPosition, currentSize);
        
        placementPreview.transform.position = worldPos;
        
        Material currentMaterial = isPendingPlacementValid ? validPlacementMaterial : invalidPlacementMaterial;
        SetPreviewMaterials(placementPreview, currentMaterial);
        
        lastClickedMousePosition = Mouse.current.position.ReadValue();
        
        furnitureIsOnPending = true;
    }
    
    Vector3? GetMouseGridPosition(){
        if(Camera.main == null) return null;
        
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        
        if(Physics.Raycast(ray, out hit, Mathf.Infinity)){
            if(hit.collider.GetComponent<GridTileInfo>() != null){
                return hit.point;
            }
        }
        
        if(gridManager != null){
            Plane gridPlane = new Plane(Vector3.up, gridManager.transform.position);
            float distance;
            if(gridPlane.Raycast(ray, out distance)){
                Vector3 point = ray.GetPoint(distance);
                
                Vector2Int gridPos = WorldToGridPosition(point);
                int halfWidth = gridManager.GridWidth / 2;
                int halfLength = gridManager.GridLength / 2;
                
                if(Mathf.Abs(gridPos.x) <= halfWidth && Mathf.Abs(gridPos.y) <= halfLength) return point;
            }
        }
        
        return null;
    }
    
    Vector2Int WorldToGridPosition(Vector3 worldPosition){
        if(gridManager == null) return Vector2Int.zero;
        
        Quaternion gridRotation = gridManager.transform.rotation;
        Quaternion inverseRotation = Quaternion.Inverse(gridRotation);
        
        Vector3 localPos = inverseRotation * (worldPosition - gridManager.transform.position);
        
        float gridSpacing = gridManager.GridSpacing;
        
        float offsetX = (gridManager.GridWidth % 2 == 0) ? gridSpacing * 0.5f : 0f;
        float offsetZ = (gridManager.GridLength % 2 == 0) ? gridSpacing * 0.5f : 0f;
        
        int gridX = Mathf.RoundToInt((localPos.x - offsetX) / gridSpacing);
        int gridZ = Mathf.RoundToInt((localPos.z - offsetZ) / gridSpacing);
        
        return new Vector2Int(gridX, gridZ);
    }
    
    bool IsPlacementValid(Vector2Int gridPos, Vector2Int size){
        if(gridManager == null) return false;
        
        List<Vector2Int> cellsToCheck = GetOccupiedCells(gridPos, size);
        
        foreach(Vector2Int cell in cellsToCheck){
            int halfWidth = gridManager.GridWidth / 2;
            int halfLength = gridManager.GridLength / 2;
            
            if(Mathf.Abs(cell.x) > halfWidth || Mathf.Abs(cell.y) > halfLength) return false;
            
            GameObject tile = gridManager.GetTileAtLocalPosition(cell);
            if(tile == null) return false;
        }
        
        foreach(Vector2Int cell in cellsToCheck) if(occupiedCells.Contains(cell)) return false;
        return true;
    }
    
    List<Vector2Int> GetOccupiedCells(Vector2Int gridPos, Vector2Int size){
        List<Vector2Int> cells = new List<Vector2Int>();
        
        Vector2Int startPos = gridPos;
        
        if(size.x % 2 == 0) startPos.x -= size.x / 2 - 1;
        else startPos.x -= size.x / 2;
        
        if(size.y % 2 == 0) startPos.y -= size.y / 2 - 1;
        else startPos.y -= size.y / 2;
        
        for(int x = 0; x < size.x; x++){
            for(int y = 0; y < size.y; y++){
                cells.Add(startPos + new Vector2Int(x, y));
            }
        }
        
        return cells;
    }
    
    void PlaceFurniture(){
        if(!furnitureIsOnPending || !isPendingPlacementValid || currentSelectedFurniture == null) return;
        
        Quaternion finalRotation = placementPreview.transform.rotation;
        Vector2Int finalSize = GetRotatedSize(finalRotation, currentSelectedFurniture.FurnitureSize);
        
        List<Vector2Int> occupied = GetOccupiedCells(pendingGridPosition, finalSize);
        Vector3 worldPos = GetTileWorldPosition(pendingGridPosition, finalSize);
        
        GameObject furnitureObj = Instantiate(
            currentSelectedFurniture.FurniturePrefab, 
            worldPos, 
            finalRotation,
            furnitureParent
        );
        furnitureObj.name = currentSelectedFurniture.FurnitureName;

        FurnitureController furnitureController = furnitureObj.AddComponent<FurnitureController>();
        furnitureController.SetFurnitureData(currentSelectedFurniture);
        furnitureController.SetGridPosition(pendingGridPosition);
        
        FurnitureInstance instance = new FurnitureInstance{
            furnitureData = currentSelectedFurniture,
            gameObject = furnitureObj,
            gridPosition = pendingGridPosition,
            occupiedCells = new List<Vector2Int>(occupied),
            rotation = finalRotation
        };

        foreach(Vector2Int cell in occupied){
            occupiedCells.Add(cell);
            placedFurniture[cell] = instance;
        }
        
        if(roomStats != null) roomStats.AddFurniture(currentSelectedFurniture);
        if(moneyManager != null) moneyManager.AddMoney(-currentSelectedFurniture.FurnitureCost);
        if(moneyUI != null) moneyUI.ShowMoneyFeedback(-currentSelectedFurniture.FurnitureCost, lastClickedMousePosition);
        if(taskManager != null) taskManager.OnFurnitureUpdated();

        StartCoroutine(DelayCancelPlacement());
    }

    void RotateFurniture(){
        if(placementPreview == null || currentSelectedFurniture == null) return;
        
        placementPreview.transform.Rotate(0, 90, 0, Space.Self);
        
        Vector2Int rotatedSize = GetRotatedSize(placementPreview.transform.rotation, currentSelectedFurniture.FurnitureSize);
        
        if(furnitureIsOnPending){
            isPendingPlacementValid = IsPlacementValid(pendingGridPosition, rotatedSize);
            
            Material currentMaterial = (isPendingPlacementValid) ? validPlacementMaterial : invalidPlacementMaterial;
            SetPreviewMaterials(placementPreview, currentMaterial);
        }
    }

    public bool RemoveFurniture(Vector2Int worldPosition){
        if(placedFurniture.ContainsKey(worldPosition)){
            FurnitureInstance furniture = placedFurniture[worldPosition];
            
            foreach(Vector2Int cell in furniture.occupiedCells){
                occupiedCells.Remove(cell);
                placedFurniture.Remove(cell);
            }
            return true;
        }
        
        return false;
    }
    
    void CancelPlacement(){
        gridHighlight.StopAllAnimations();
        if(placementPreview != null){
            Destroy(placementPreview);
            placementPreview = null;
        }
        
        currentSelectedFurniture = null;
        furnitureIsOnPending = false;
        hasValidLastPosition = false;
    }

    IEnumerator DelayCancelPlacement(){
        yield return new WaitForSeconds(0.1f);
        CancelPlacement();
    }
    
    public void SelectFurniture(FurnitureData furnitureData){
        SetFurnitureData(furnitureData);
    }

    public List<Vector2Int> GetAllUnoccupiedTilePositions(){
        List<Vector2Int> unoccupiedPositions = new List<Vector2Int>();
        
        if(gridManager == null) return unoccupiedPositions;
        
        int halfWidth = gridManager.GridWidth / 2;
        int halfLength = gridManager.GridLength / 2;
        
        for(int x = -halfWidth; x <= halfWidth; x++){
            for(int y = -halfLength; y <= halfLength; y++){
                Vector2Int gridPos = new Vector2Int(x, y);
                
                GameObject tile = gridManager.GetTileAtLocalPosition(gridPos);
                if(tile == null) continue;
                
                if(!occupiedCells.Contains(gridPos)) unoccupiedPositions.Add(gridPos);
            }
        }
        
        return unoccupiedPositions;
    }

    public bool IsInBuildMode => currentSelectedFurniture != null;

    void OnDrawGizmos(){
        if(!Application.isPlaying || gridManager == null) return;
        
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        float cellSize = gridManager.GridSpacing;
        
        foreach(Vector2Int cell in occupiedCells){
            Vector3 worldPos = CalculatePositionFromGrid(cell, new Vector2Int(1, 1));
            Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
        }
    }
}

[System.Serializable]
public class FurnitureInstance
{
    public FurnitureData furnitureData;
    public GameObject gameObject;
    public Vector2Int gridPosition;
    public List<Vector2Int> occupiedCells;
    public Quaternion rotation;
}