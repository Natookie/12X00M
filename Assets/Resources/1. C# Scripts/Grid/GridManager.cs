using UnityEngine;
using System.Collections;
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
    
    [Header("EDITOR CONTROLS")]
    [SerializeField] private bool initializeFirst = true;
    [SerializeField] private bool showGizmos = true;
    
    private Dictionary<Vector2Int, GameObject> gridTiles = new Dictionary<Vector2Int, GameObject>();
    private GameObject lastHoveredTile;
    
    [Header("INTERACTION SETTINGS")]
    [SerializeField] private bool enableHover = true;
    [SerializeField] private bool enableRotation = true;
    private bool isAnimating;
    private bool isBeingRotated;
    
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
            if(enableAnimation){
                StartCoroutine(AnimateGridWave());
            }
        }
    }
    
    void InitGrid(){
        if(!initializeFirst) return;

        if(gridPrefab == null){
            Debug.LogError("Prefab missing");
            return;
        }
        
        if(gridColors.Length != 2){
            Debug.LogError("Need 2 colors for checkerboard pattern");
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
                
                GameObject tile = Instantiate(gridPrefab, position, Quaternion.identity, transform);
                tile.name = $"GridTile_{x}_{z}";
                
                GridTileInfo tileInfo = tile.AddComponent<GridTileInfo>();
                tileInfo.gridLocalPosition = new Vector2Int(gridX, gridZ);
                tileInfo.gridIndexPosition = new Vector2Int(x, z);
                
                if(enableAnimation && Application.isPlaying) tile.transform.localScale = Vector3.zero;
                
                Renderer tileRenderer = tile.GetComponent<Renderer>();
                if(tileRenderer != null){
                    int colorIndex = (x + z) % 2; 
                    Material tileMaterial = new Material(tileRenderer.material);
                    tileMaterial.color = gridColors[colorIndex];
                    tileRenderer.material = tileMaterial;
                    
                    
                    tileInfo.originalMaterial = tileMaterial;
                    tileInfo.originalColor = gridColors[colorIndex];
                    tileInfo.isDarkTile = colorIndex == 1;
                }
                
                
                if(tile.GetComponent<Collider>() == null) tile.AddComponent<BoxCollider>();
                gridTiles[new Vector2Int(gridX, gridZ)] = tile;
            }
        }
    }
    
    IEnumerator AnimateGridWave(){
        if(!enableAnimation || gridTiles.Count == 0) yield break;
        isAnimating = true;

        float delayBetweenTiles = animationDuration / (gridLength + gridWidth);
        
        for(int sum = 0; sum <= gridLength + gridWidth - 2; sum++){
            for(int x = 0; x < gridWidth; x++){
                int z = sum - x;
                if(z >= 0 && z < gridLength){
                    int gridX = x - (gridWidth / 2);
                    int gridZ = z - (gridLength / 2);
                    StartCoroutine(AnimateSingleTile(new Vector2Int(gridX, gridZ)));
                    yield return new WaitForSeconds(delayBetweenTiles);
                }
            }
        }

        yield return new WaitForSeconds(1f);
        isAnimating = false;
    }
    
    IEnumerator AnimateSingleTile(Vector2Int gridPos){
        if(!gridTiles.ContainsKey(gridPos)) yield break;
        
        GameObject tile = gridTiles[gridPos];
        Vector3 startPos = tile.transform.position;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration){
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            float scaleValue = scaleCurve.Evaluate(t);
            tile.transform.localScale = Vector3.one * scaleValue;
            
            float heightValue = heightCurve.Evaluate(t) * maxHeightOffset;
            tile.transform.position = new Vector3(
                startPos.x,
                startPos.y + heightValue,
                startPos.z
            );
            
            yield return null;
        }
        
        
        tile.transform.localScale = Vector3.one;
        tile.transform.position = startPos;
    }
    
    void Update(){
        if(Application.isPlaying) DetectCursor();
    }
    
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
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask)){
            GameObject hitTile = hit.collider.gameObject;
            
            if(lastHoveredTile != null && lastHoveredTile != hitTile){
                ResetTileColor(lastHoveredTile);
            }
            
            if(hitTile != lastHoveredTile){
                lastHoveredTile = hitTile;
                SetTileInteractionColor(hitTile, hoverColor);
            }
            
            if(Mouse.current.leftButton.wasPressedThisFrame){
                StartCoroutine(FlashTile(hitTile, clickColor, 0.2f));
                
                GridTileInfo tileInfo = hitTile.GetComponent<GridTileInfo>();
                //if(tileInfo != null) Debug.Log("Sawit");
            }
        }else{
            if(lastHoveredTile != null){
                ResetTileColor(lastHoveredTile);
                lastHoveredTile = null;
            }
        }
    }
    
    void SetTileInteractionColor(GameObject tile, Color interactionColor){
        Renderer renderer = tile.GetComponent<Renderer>();
        if(renderer != null) renderer.material.color = interactionColor;
    }
    
    void ResetTileColor(GameObject tile){
        GridTileInfo tileInfo = tile.GetComponent<GridTileInfo>();
        if(tileInfo != null){
            Renderer renderer = tile.GetComponent<Renderer>();
            if(renderer != null) renderer.material.color = tileInfo.originalColor;
        }
    }
    
    IEnumerator FlashTile(GameObject tile, Color flashColor, float duration){
        Renderer renderer = tile.GetComponent<Renderer>();
        if(renderer == null) yield break;
        
        Color originalColor = renderer.material.color;
        renderer.material.color = flashColor;
        
        yield return new WaitForSeconds(duration);
        
        if(tile != null && renderer != null){
            if(tile == lastHoveredTile) renderer.material.color = hoverColor;
            else ResetTileColor(tile);
        }
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
    
    public GameObject GetTileAtLocalPosition(Vector2Int localPos) => gridTiles.ContainsKey(localPos) ? gridTiles[localPos] : null;
    public GameObject GetTileAtWorldPosition(Vector3 worldPos){
        Vector3 center = GridCenter;
        float offsetX = (gridWidth % 2 == 0) ? gridSpacing * 0.5f : 0f;
        float offsetZ = (gridLength % 2 == 0) ? gridSpacing * 0.5f : 0f;
        
        int localX = Mathf.RoundToInt((worldPos.x - center.x - offsetX) / gridSpacing);
        int localZ = Mathf.RoundToInt((worldPos.z - center.z - offsetZ) / gridSpacing);
        
        return GetTileAtLocalPosition(new Vector2Int(localX, localZ));
    }
}

public class GridTileInfo : MonoBehaviour
{
    public Vector2Int gridLocalPosition;  
    public Vector2Int gridIndexPosition;  
    [HideInInspector] public Material originalMaterial;
    [HideInInspector] public Color originalColor;
    [HideInInspector] public bool isDarkTile;
}