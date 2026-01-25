using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuildSystem : MonoBehaviour
{
    public static BuildSystem Instance { get; private set; }
    
    [Header("REFERENCES")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private RoomStats roomStats;
    [SerializeField] private MoneyManager moneyManager;
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
    private bool isPendingPlacementValid = false;
    
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
        
        if(!furnitureIsOnPending && placementPreview != null) UpdatePlacementPreviewPosition();
    }
    
    void HandleMouseInput(){
        if(Mouse.current.leftButton.wasPressedThisFrame){
            if(currentSelectedFurniture != null && !furnitureIsOnPending) ConfirmPosition();
            else if(furnitureIsOnPending && isPendingPlacementValid) PlaceFurniture();
        }
        
        if(Mouse.current.rightButton.wasPressedThisFrame) CancelPlacement();
        if(Keyboard.current.rKey.wasPressedThisFrame && currentSelectedFurniture != null) RotateFurniture();
        if(Keyboard.current.escapeKey.wasPressedThisFrame) ClearSelection();
    }
    
    public void SetFurnitureData(FurnitureData data){
        if(data == null || !data.IsValid()){
            Debug.LogWarning("Invalid furniture data provided");
            return;
        }
        
        CancelPlacement();
        
        currentSelectedFurniture = data;
        furnitureIsOnPending = false;
        
        Debug.Log($"Selected furniture: {data.FurnitureName}");
        
        CreatePlacementPreview();
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
    
    void UpdatePlacementPreviewPosition(){
        if(placementPreview == null || gridManager == null) return;
        
        Vector3? mouseGridPos = GetMouseGridPosition();
        if(!mouseGridPos.HasValue) return;
        
        Vector2Int gridPos = WorldToGridPosition(mouseGridPos.Value);
        bool isValid = IsPlacementValid(gridPos, currentSelectedFurniture.FurnitureSize);
        
        Vector3 worldPos = GetTileWorldPosition(gridPos, currentSelectedFurniture.FurnitureSize);
        
        placementPreview.transform.position = worldPos;
        
        Material currentMaterial = isValid ? validPlacementMaterial : invalidPlacementMaterial;
        SetPreviewMaterials(placementPreview, currentMaterial);
    }
    
    Vector3 GetTileWorldPosition(Vector2Int gridPos, Vector2Int size){
        if(gridManager == null) return Vector3.zero;
        
        GameObject centerTile = gridManager.GetTileAtLocalPosition(gridPos);
        if(centerTile == null) return CalculatePositionFromGrid(gridPos, size);
        
        Vector3 tilePos = centerTile.transform.position;
        
        if(size.x > 1 || size.y > 1){
            float offsetX = (size.x - 1) * gridManager.GridSpacing * 0.5f;
            float offsetZ = (size.y - 1) * gridManager.GridSpacing * 0.5f;
            
            Vector2Int startPos = gridPos - new Vector2Int(size.x / 2, size.y / 2);
            GameObject startTile = gridManager.GetTileAtLocalPosition(startPos);
            
            if(startTile != null){
                Vector3 startPosWorld = startTile.transform.position;
                tilePos = startPosWorld + new Vector3(offsetX, 0, offsetZ);
            }
        }
        
        Renderer tileRenderer = centerTile.GetComponent<Renderer>();
        if(tileRenderer != null){
            float tileHeight = tileRenderer.bounds.size.y;
            tilePos.y += tileHeight * 0.5f; // Bottom of furniture at tile top
            
            if(placementPreview != null){
                Renderer furnitureRenderer = placementPreview.GetComponent<Renderer>();
                if(furnitureRenderer != null){
                    float furnitureHeight = furnitureRenderer.bounds.size.y;
                    tilePos.y += furnitureHeight * 0.5f;
                }
            }
        }
        
        tilePos.y += placementYOffset;
        
        return tilePos;
    }
    
    Vector3 CalculatePositionFromGrid(Vector2Int gridPos, Vector2Int size){
        Vector3 center = gridManager.transform.position;
        float gridSpacing = gridManager.GridSpacing;
        
        float offsetX = (gridManager.GridWidth % 2 == 0) ? gridSpacing * 0.5f : 0f;
        float offsetZ = (gridManager.GridLength % 2 == 0) ? gridSpacing * 0.5f : 0f;
        
        Vector3 worldPos = center;
        worldPos.x += (gridPos.x * gridSpacing) + offsetX;
        worldPos.z += (gridPos.y * gridSpacing) + offsetZ;
        
        if(size.x > 1 || size.y > 1){
            float sizeOffsetX = (size.x - 1) * gridSpacing * 0.5f;
            float sizeOffsetZ = (size.y - 1) * gridSpacing * 0.5f;
            worldPos.x += sizeOffsetX;
            worldPos.z += sizeOffsetZ;
        }
        
        return worldPos;
    }
    
    void ConfirmPosition(){
        if(currentSelectedFurniture == null || placementPreview == null) return;
        
        Vector3? mouseGridPos = GetMouseGridPosition();
        if(!mouseGridPos.HasValue) return;
        
        pendingGridPosition = WorldToGridPosition(mouseGridPos.Value);
        isPendingPlacementValid = IsPlacementValid(pendingGridPosition, currentSelectedFurniture.FurnitureSize);
        
        Vector3 worldPos = GetTileWorldPosition(pendingGridPosition, currentSelectedFurniture.FurnitureSize);
        
        placementPreview.transform.position = worldPos;
        
        Material currentMaterial = isPendingPlacementValid ? validPlacementMaterial : invalidPlacementMaterial;
        SetPreviewMaterials(placementPreview, currentMaterial);
        
        //PlacementPreview, nanti ganti
        if(placementPreview != null && isPendingPlacementValid){
            placementPreview.transform.localScale = Vector3.one * 1.05f;
        }
        
        furnitureIsOnPending = true;
        
        Debug.Log($"Position confirmed at grid {pendingGridPosition}. Valid: {isPendingPlacementValid}");
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
                return ray.GetPoint(distance);
            }
        }
        
        return null;
    }
    
    Vector2Int WorldToGridPosition(Vector3 worldPosition){
        if(gridManager == null) return Vector2Int.zero;
        
        GameObject tile = gridManager.GetTileAtWorldPosition(worldPosition);
        if(tile != null){
            GridTileInfo tileInfo = tile.GetComponent<GridTileInfo>();
            if(tileInfo != null) return tileInfo.gridLocalPosition;
        }
        
        Vector3 center = gridManager.transform.position;
        float gridSpacing = gridManager.GridSpacing;
        
        float offsetX = (gridManager.GridWidth % 2 == 0) ? gridSpacing * 0.5f : 0f;
        float offsetZ = (gridManager.GridLength % 2 == 0) ? gridSpacing * 0.5f : 0f;
        
        int gridX = Mathf.RoundToInt((worldPosition.x - center.x - offsetX) / gridSpacing);
        int gridZ = Mathf.RoundToInt((worldPosition.z - center.z - offsetZ) / gridSpacing);
        
        return new Vector2Int(gridX, gridZ);
    }
    
    bool IsPlacementValid(Vector2Int gridPos, Vector2Int size){
        if(gridManager == null) return false;
        
        List<Vector2Int> cellsToCheck = GetOccupiedCells(gridPos, size);
        
        foreach(Vector2Int cell in cellsToCheck){
            GameObject tile = gridManager.GetTileAtLocalPosition(cell);
            if(tile == null) return false;
        }
        
        foreach(Vector2Int cell in cellsToCheck) if(occupiedCells.Contains(cell)) return false;
        return true;
    }
    
    List<Vector2Int> GetOccupiedCells(Vector2Int gridPos, Vector2Int size){
        List<Vector2Int> cells = new List<Vector2Int>();
        
        Vector2Int startPos = gridPos;
        startPos.x -= (size.x - 1) / 2;
        startPos.y -= (size.y - 1) / 2;
        
        for(int x = 0; x < size.x; x++){
            for(int y = 0; y < size.y; y++){
                cells.Add(startPos + new Vector2Int(x, y));
            }
        }
        
        return cells;
    }
    
    void PlaceFurniture(){
        if(!furnitureIsOnPending || !isPendingPlacementValid || currentSelectedFurniture == null) return;
        
        List<Vector2Int> occupied = GetOccupiedCells(pendingGridPosition, currentSelectedFurniture.FurnitureSize);
        Vector3 worldPos = GetTileWorldPosition(pendingGridPosition, currentSelectedFurniture.FurnitureSize);
        
        GameObject furnitureObj = Instantiate(currentSelectedFurniture.FurniturePrefab, worldPos, placementPreview.transform.rotation, furnitureParent);
        furnitureObj.name = currentSelectedFurniture.FurnitureName;

        FurnitureController furnitureController = furnitureObj.AddComponent<FurnitureController>();
        furnitureController.SetFurnitureData(currentSelectedFurniture);
        furnitureController.SetGridPosition(pendingGridPosition);
        
        FurnitureInstance instance = new FurnitureInstance{
            furnitureData = currentSelectedFurniture,
            gameObject = furnitureObj,
            gridPosition = pendingGridPosition,
            occupiedCells = new List<Vector2Int>(occupied),
            rotation = furnitureObj.transform.rotation
        };
        
        foreach(Vector2Int cell in occupied){
            occupiedCells.Add(cell);
            placedFurniture[cell] = instance;
        }
        
        if(roomStats != null) roomStats.AddFurniture(currentSelectedFurniture);
        if(moneyManager != null) moneyManager.AddMoney(-currentSelectedFurniture.FurnitureCost);
        if(taskManager != null) taskManager.OnFurniturePlaced();

        Debug.Log($"Placed {currentSelectedFurniture.FurnitureName} at grid {pendingGridPosition}");
        Debug.Log($"Money - {currentSelectedFurniture.FurnitureCost}");
        ClearSelection();
    }

    public bool RemoveFurniture(Vector3 worldPosition){
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        
        if(placedFurniture.ContainsKey(gridPos)){
            FurnitureInstance furniture = placedFurniture[gridPos];
            
            foreach(Vector2Int cell in furniture.occupiedCells){
                occupiedCells.Remove(cell);
                placedFurniture.Remove(cell);
            }
            
            if(furniture.gameObject != null) Destroy(furniture.gameObject);
            
            Debug.Log($"Removed {furniture.furnitureData.FurnitureName} from grid");
            return true;
        }
        
        return false;
    }
    
    void RotateFurniture(){
        if(placementPreview == null || currentSelectedFurniture == null) return;
        
        placementPreview.transform.Rotate(0, 90, 0);
        
        if(furnitureIsOnPending){
            isPendingPlacementValid = IsPlacementValid(pendingGridPosition, currentSelectedFurniture.FurnitureSize);
            
            Material currentMaterial = isPendingPlacementValid ? validPlacementMaterial : invalidPlacementMaterial;
            SetPreviewMaterials(placementPreview, currentMaterial);
        }
    }
    
    void CancelPlacement(){
        if(placementPreview != null){
            Destroy(placementPreview);
            placementPreview = null;
        }
        
        currentSelectedFurniture = null;
        furnitureIsOnPending = false;
        
        Debug.Log("Placement cancelled");
    }
    
    void ClearSelection(){
        CancelPlacement();
        Debug.Log("Selection cleared");
    }
    
    public void SelectFurniture(FurnitureData furnitureData){
        SetFurnitureData(furnitureData);
    }
    
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