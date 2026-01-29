using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    [Header("GRID CONFIGURATION")]
    [SerializeField] private int gridLength = 10;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private float gridSpacing = 1.1f;
    [SerializeField] private bool centerOnTransform = true;
    [SerializeField] private Vector3 customCenterPos = Vector3.zero;
    [Space(10)]
    [SerializeField] private Transform gridParent;

    public int GridWidth => gridWidth;
    public int GridLength => gridLength;
    public float GridSpacing => gridSpacing;
    
    [Header("GRID VISUAL")]
    [SerializeField] private GameObject gridPrefab;
    [SerializeField] private Color[] gridColors = new Color[2];
    
    [Header("INTERACTION COLORS")]
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color clickColor = Color.red;
    
    [Header("GRID ANIMATION")]
    [SerializeField] private bool enableAnimation = true;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float maxHeightOffset = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("CURSOR DETECTION")]
    [SerializeField] private LayerMask gridLayerMask = -1;
    [SerializeField] private GridHighlight gridHighlight;

    [Header("EDITOR CONTROLS")]
    [SerializeField] private bool initializeFirst = true;
    [SerializeField] private bool showGizmos = true;
    
    private Dictionary<Vector2Int, GridTileData> gridTiles = new Dictionary<Vector2Int, GridTileData>();
    private List<GameObject> currentlyHighlightedTiles = new List<GameObject>();
    private GameObject lastHoveredTile;
    
    [Header("INTERACTION SETTINGS")]
    [SerializeField] private bool enableHover = true;
    private bool isAnimating;
    private bool isBeingRotated;
    private float animationTimer;
    
    private Vector3 GridCenter{
        get{
            return centerOnTransform ? transform.position : customCenterPos;
        }
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Generate Grid")]
    void GenerateGridInEditor(){
        if(Application.isPlaying) return;
        
        ClearExistingGrid();
        InitGrid();
        
        GameObject editorGridContainer = GameObject.Find("EditorGridContainer");
        if(editorGridContainer == null) editorGridContainer = new GameObject("EditorGridContainer");
        
        List<Transform> tilesToMove = new List<Transform>();
        foreach(Transform child in transform){
            if(child.name.StartsWith("GridTile_")) tilesToMove.Add(child);
        }
        foreach(Transform tile in tilesToMove){
            tile.SetParent(editorGridContainer.transform);
        }
    }
    
    [ContextMenu("Clear Grid")]
    void ClearGridInEditor(){
        if(Application.isPlaying) return;
        
        GameObject editorGridContainer = GameObject.Find("EditorGridContainer");
        if(editorGridContainer != null) DestroyImmediate(editorGridContainer);
    }
    #endif
    
    void Start(){
        if(Application.isPlaying){
            ClearExistingGrid();
            InitGrid();
        }
    }
    
    void InitGrid(){
        if(!initializeFirst) return;

        if(gridPrefab == null){
            Debug.LogError("Prefab missing");
            return;
        }
        if(gridColors.Length != 2){
            Debug.LogError("Need 2 colors");
            return;
        }
        
        gridTiles.Clear();
        
        Vector3 centerPos = GridCenter;
        
        float offsetX = (gridWidth % 2 == 0) ? gridSpacing * 0.5f : 0f;
        float offsetZ = (gridLength % 2 == 0) ? gridSpacing * 0.5f : 0f;
        
        int halfWidth = gridWidth / 2;
        int halfLength = gridLength / 2;
        
        for(int x = 0; x < gridWidth; x++){
            for(int z = 0; z < gridLength; z++){
                int gridX = x - halfWidth;
                int gridZ = z - halfLength;
                
                Vector3 position = centerPos + new Vector3(
                    (gridX * gridSpacing) + offsetX,
                    0,
                    (gridZ * gridSpacing) + offsetZ
                );
                
                GameObject tile = Instantiate(gridPrefab, position, Quaternion.identity, gridParent);
                tile.name = $"GridTile_{x}_{z}";
                
                Vector3 originalScale = tile.transform.localScale;
                
                if(enableAnimation && Application.isPlaying) tile.transform.localScale = Vector3.zero;
                
                GridTileInfo tileInfo = tile.AddComponent<GridTileInfo>();
                tileInfo.gridLocalPosition = new Vector2Int(gridX, gridZ);
                tileInfo.gridIndexPosition = new Vector2Int(x, z);
                
                Renderer tileRenderer = tile.GetComponent<Renderer>();
                if(tileRenderer != null){
                    int colorIndex = (x + z) % 2; 
                    Color tileColor = gridColors[colorIndex];
                    
                    Material tileMaterial = new Material(tileRenderer.sharedMaterial);
                    tileMaterial.color = tileColor;
                    tileRenderer.material = tileMaterial;
                    
                    tileInfo.originalMaterial = tileMaterial;
                    tileInfo.originalColor = tileColor;
                    tileInfo.isDarkTile = colorIndex == 1;
                    tileInfo.tileRenderer = tileRenderer;
                }else Debug.LogWarning("Hidup J");
                
                if(tile.GetComponent<Collider>() == null) tile.AddComponent<BoxCollider>();
                
                GridTileData tileData = new GridTileData{
                    gameObject = tile,
                    renderer = tile.GetComponent<Renderer>(),
                    originalPosition = position,
                    originalScale = originalScale,
                    startDelay = CalculateAnimationDelay(gridX, gridZ),
                    localYOffset = 0f,
                    targetScale = Vector3.zero,
                    currentScale = Vector3.zero,
                    tileInfo = tileInfo
                };
                
                gridTiles[new Vector2Int(gridX, gridZ)] = tileData;
            }
        }
        
        if(enableAnimation && Application.isPlaying) StartAnimation();
    }
    
    float CalculateAnimationDelay(int gridX, int gridZ){
        if(!enableAnimation) return 0f;
        
        int halfWidth = gridWidth / 2;
        int halfLength = gridLength / 2;
        
        float normalizedX = (gridX + halfWidth) / (float)gridWidth;
        float normalizedZ = (gridZ + halfLength) / (float)gridLength;
        
        return (normalizedX + normalizedZ) * 0.2f;
    }
    
    void StartAnimation(){
        if(!enableAnimation || gridTiles.Count == 0) return;
        
        isAnimating = true;
        animationTimer = 0f;
        
        foreach(var tileData in gridTiles.Values){
            tileData.targetScale = tileData.originalScale;
            tileData.currentScale = Vector3.zero;
            tileData.localYOffset = 0f;
            tileData.startTime = animationTimer + tileData.startDelay;
            
            if(tileData.gameObject != null) tileData.gameObject.transform.localScale = Vector3.zero;
        }
    }
    
    void Update(){
        if(Application.isPlaying){
            DetectCursor();
            if(isAnimating) UpdateAnimation();
        }
    }
    
    void UpdateAnimation(){
        if(!isAnimating) return;
        
        animationTimer += Time.deltaTime;
        bool allAnimationsComplete = true;
        
        foreach(var tileData in gridTiles.Values){
            if(tileData.gameObject == null) continue;
            
            float timeSinceStart = animationTimer - tileData.startTime;
            
            if(timeSinceStart < 0){
                allAnimationsComplete = false;
                continue;
            }
            
            float normalizedTime = Mathf.Clamp01(timeSinceStart / animationDuration);
            
            if(normalizedTime < 1f){
                allAnimationsComplete = false;
                
                float scaleT = scaleCurve.Evaluate(normalizedTime);
                float heightT = heightCurve.Evaluate(normalizedTime);
                
                tileData.currentScale = Vector3.Lerp(Vector3.zero, tileData.targetScale, scaleT);
                tileData.gameObject.transform.localScale = tileData.currentScale;
                
                tileData.localYOffset = Mathf.Lerp(0f, maxHeightOffset, heightT);
                Vector3 localPos = tileData.gameObject.transform.localPosition;
                localPos.y = tileData.localYOffset;
                tileData.gameObject.transform.localPosition = localPos;
            }else{
                tileData.gameObject.transform.localScale = tileData.targetScale;
                
                Vector3 localPos = tileData.gameObject.transform.localPosition;
                localPos.y = 0f;
                tileData.gameObject.transform.localPosition = localPos;
            }
        }
        
        if(allAnimationsComplete){
            isAnimating = false;
            animationTimer = 0f;
        }
    }
    
    public bool IsGridBusy() => (isAnimating || isBeingRotated);
    
    public void SetRotationState(bool rotating){
        isBeingRotated = rotating;
        
        if(rotating && lastHoveredTile != null){
            ResetTileColor(lastHoveredTile);
            lastHoveredTile = null;
        }
    }

    public bool IsAnimating() => isAnimating;
    
    void DetectCursor(){
        if(isAnimating || isBeingRotated || !enableHover) return;
        if(gridHighlight.IsAnimating) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask)){
            GameObject hitTile = hit.collider.gameObject;

            if(lastHoveredTile != null && lastHoveredTile != hitTile) ResetTileColor(lastHoveredTile);
            
            if(hitTile != lastHoveredTile){
                lastHoveredTile = hitTile;
                SetTileInteractionColor(hitTile, hoverColor);
            }
            
            if(Mouse.current.leftButton.wasPressedThisFrame) FlashTile(hitTile, clickColor, 0.2f);
        }else{
            if(lastHoveredTile != null){
                ResetTileColor(lastHoveredTile);
                lastHoveredTile = null;
            }
        }
    }
    
    void SetTileInteractionColor(GameObject tile, Color interactionColor){
        Renderer renderer = tile.GetComponent<Renderer>();
        if(renderer != null){
            if(renderer.material != null) renderer.material.color = interactionColor;
        }
    }
    
    void ResetTileColor(GameObject tile){
        if(tile == null) return;
        GridTileInfo tileInfo = tile.GetComponent<GridTileInfo>();
        if(tileInfo != null && tileInfo.originalMaterial != null){
            Renderer renderer = tile.GetComponent<Renderer>();
            if(renderer != null) renderer.material.color = tileInfo.originalColor;
        }
    }
    
    async void FlashTile(GameObject tile, Color flashColor, float duration){
        Renderer renderer = tile.GetComponent<Renderer>();
        if(renderer == null) return;
        
        Color currentColor = renderer.material.color;
        renderer.material.color = flashColor;
        await System.Threading.Tasks.Task.Delay((int)(duration * 1000));
        
        if(tile != null && renderer != null){
            if(tile == lastHoveredTile) renderer.material.color = hoverColor;
            else ResetTileColor(tile);
        }
    }

    public void HighlightTiles(List<Vector2Int> tilePositions, bool isValid){
        ClearMultiTileHighlights();
        
        foreach(Vector2Int pos in tilePositions){
            GameObject tile = GetTileAtLocalPosition(pos);
            if(tile != null){
                Color highlightColor = (isValid) ? Color.green : Color.red;
                SetTileInteractionColor(tile, highlightColor);
                currentlyHighlightedTiles.Add(tile);
            }
        }
    }

    public void ClearMultiTileHighlights(){
        foreach(GameObject tile in currentlyHighlightedTiles){
            ResetTileColor(tile);
        }
        currentlyHighlightedTiles.Clear();
    }
    
    void ClearExistingGrid(){
        List<GameObject> tilesToDestroy = new List<GameObject>();
        
        foreach (Transform child in transform){
            if(child.name.StartsWith("GridTile_")) tilesToDestroy.Add(child.gameObject);
        }
        
        foreach (GameObject tile in tilesToDestroy){
            if(Application.isPlaying) Destroy(tile);
            else DestroyImmediate(tile);
        }
        
        gridTiles.Clear();
    }
    
    void OnDrawGizmosSelected(){
        if(!showGizmos) return;
        
        Gizmos.color = Color.green;
        
        Vector3 center = GridCenter;
        Vector3 size = new Vector3(
            gridWidth * gridSpacing,
            0.1f,
            gridLength * gridSpacing
        );
        
        Gizmos.DrawWireCube(center, size);
        
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        
        float offsetX = (gridWidth % 2 == 0) ? gridSpacing * 0.5f : 0f;
        float offsetZ = (gridLength % 2 == 0) ? gridSpacing * 0.5f : 0f;
        
        int halfWidth = gridWidth / 2;
        int halfLength = gridLength / 2;
        
        for(int x = -halfWidth; x <= halfWidth; x++){
            Vector3 lineStart = center + new Vector3(
                (x * gridSpacing) + offsetX,
                0,
                (-halfLength * gridSpacing) + offsetZ
            );
            Vector3 lineEnd = center + new Vector3(
                (x * gridSpacing) + offsetX,
                0,
                (halfLength * gridSpacing) + offsetZ
            );
            Gizmos.DrawLine(lineStart, lineEnd);
        }
        
        for(int z = -halfLength; z <= halfLength; z++){
            Vector3 lineStart = center + new Vector3(
                (-halfWidth * gridSpacing) + offsetX,
                0,
                (z * gridSpacing) + offsetZ
            );
            Vector3 lineEnd = center + new Vector3(
                (halfWidth * gridSpacing) + offsetX,
                0,
                (z * gridSpacing) + offsetZ
            );
            Gizmos.DrawLine(lineStart, lineEnd);
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, 0.2f);
    }
    
    public GameObject GetTileAtLocalPosition(Vector2Int localPos){
        return gridTiles.ContainsKey(localPos) ? gridTiles[localPos].gameObject : null;
    }
    
    public GameObject GetTileAtWorldPosition(Vector3 worldPos){
        Vector3 center = GridCenter;
        float offsetX = (gridWidth % 2 == 0) ? gridSpacing * 0.5f : 0f;
        float offsetZ = (gridLength % 2 == 0) ? gridSpacing * 0.5f : 0f;
        
        int localX = Mathf.RoundToInt((worldPos.x - center.x - offsetX) / gridSpacing);
        int localZ = Mathf.RoundToInt((worldPos.z - center.z - offsetZ) / gridSpacing);
        
        return GetTileAtLocalPosition(new Vector2Int(localX, localZ));
    }
}

public class GridTileData
{
    public GameObject gameObject;
    public Renderer renderer;
    public Vector3 originalPosition;
    public Vector3 originalScale;
    public float startDelay;
    public float startTime;
    public float localYOffset;
    public Vector3 targetScale;
    public Vector3 currentScale;
    public GridTileInfo tileInfo;
}

public class GridTileInfo : MonoBehaviour
{
    public Vector2Int gridLocalPosition;  
    public Vector2Int gridIndexPosition;  
    [HideInInspector] public Material originalMaterial;
    [HideInInspector] public Color originalColor;
    [HideInInspector] public bool isDarkTile;
    [HideInInspector] public Renderer tileRenderer;
}