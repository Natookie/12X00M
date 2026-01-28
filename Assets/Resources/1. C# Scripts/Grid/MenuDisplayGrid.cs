using System.Collections.Generic;
using UnityEngine;

public class MenuDisplayGrid : MonoBehaviour
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

    [Header("ROTATION SETTINGS")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float rotationDirection = 1f;
    [SerializeField] private float smoothTime = .2f;

    
    private Dictionary<Vector2Int, GridTileData> gridTiles = new Dictionary<Vector2Int, GridTileData>();    
    private Vector3 GridCenter{
        get{
            return centerOnTransform ? transform.position : customCenterPos;
        }
    }

    //Rotation state
    private float targetRotationY;
    private float currentRotationY;
    private float rotationVelocity;

    void Start(){
        ClearExistingGrid();
        InitGrid();
    }

    void FixedUpdate(){
        float rotationDelta = rotationDirection * rotationSpeed * Time.deltaTime;
        targetRotationY += rotationDelta;  
        UpdateRotation();
    }

    void InitGrid(){
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
                }
                
                if(tile.GetComponent<Collider>() == null) tile.AddComponent<BoxCollider>();
                
                GridTileData tileData = new GridTileData{
                    gameObject = tile,
                    renderer = tile.GetComponent<Renderer>(),
                    originalPosition = position,
                    originalScale = originalScale,
                    startDelay = 0f,
                    localYOffset = 0f,
                    targetScale = Vector3.zero,
                    currentScale = Vector3.zero,
                    tileInfo = tileInfo
                };
                
                gridTiles[new Vector2Int(gridX, gridZ)] = tileData;
            }
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
}
