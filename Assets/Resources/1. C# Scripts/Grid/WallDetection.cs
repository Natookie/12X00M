using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WallDetection : MonoBehaviour
{
    [Header("RAYCAST")]
    public Transform leftRaycast;
    public Transform rightRaycast;
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private LayerMask wallLayer;

    [Header("WALLS")]
    public List<GameObject> walls = new List<GameObject>();
    
    [Header("REFERENCES")]
    [SerializeField] private GridRotator gridRotator;
    
    [Header("DEBUG")]
    [SerializeField] private bool enableDetection = true;
    [SerializeField] private GameObject leftWallHit;
    [SerializeField] private GameObject rightWallHit;
    [SerializeField] private bool showDebugRays = true;
    
    private struct WallInfo{
        public GameObject gameObject;
        public Renderer renderer;
    }
    
    private WallInfo[] currentHiddenWalls = new WallInfo[2];//[0]=left, [1]=right
    private bool isRotating = false;
    private Color leftRayColor = Color.green;
    private Color rightRayColor = Color.green;
    
    void Start(){
        if(wallLayer == 0) wallLayer = LayerMask.GetMask("Default");
        if(gridRotator == null) gridRotator = FindFirstObjectByType<GridRotator>();
        
        if(gridRotator != null){
            gridRotator.OnRotationStarted.AddListener(OnRotationStarted);
            gridRotator.OnRotationStopped.AddListener(OnRotationStopped);
        }
        
        UpdateHiddenWalls();
        if(!enableDetection) DisableAllWalls();
    }
    
    void OnRotationStarted(){
        isRotating = true;
    }
    void OnRotationStopped(){
        isRotating = false;
        UpdateHiddenWalls();
    }
    
    void Update(){
        if(!enableDetection) return;

        if(isRotating) UpdateHiddenWalls();
        if(showDebugRays && Application.isPlaying) DrawDebugRays();
    }
    
    void UpdateHiddenWalls(){
        GameObject newLeftWall = GetWallHitByRay(leftRaycast, 0);
        GameObject newRightWall = GetWallHitByRay(rightRaycast, 1);
        
        UpdateSingleWall(0, newLeftWall);
        UpdateSingleWall(1, newRightWall);
    }
    
    void UpdateSingleWall(int index, GameObject newWall){
        WallInfo oldWallInfo = currentHiddenWalls[index];
        GameObject oldWall = oldWallInfo.gameObject;
        
        if(index == 0) leftWallHit = newWall;
        else rightWallHit = newWall;
        
        if(oldWall != newWall){
            if(oldWall != null && oldWall != currentHiddenWalls[1 - index].gameObject){
                ShowWall(oldWallInfo);
            }
            
            if(newWall != null) HideWall(index, newWall);
            currentHiddenWalls[index] = new WallInfo {
                gameObject = newWall,
                renderer = (newWall != null) ? newWall.GetComponent<Renderer>() : null
            };
        }else if(newWall != null && IsWallVisible(newWall)){
            HideWall(index, newWall);
        }
    }
    
    void HideWall(int index, GameObject wall){
        if(wall == null) return;
        
        if(currentHiddenWalls[index].gameObject != wall || currentHiddenWalls[index].renderer == null){
            currentHiddenWalls[index] = new WallInfo {
                gameObject = wall,
                renderer = wall.GetComponent<Renderer>()
            };
        }
        
        if(currentHiddenWalls[index].renderer != null) currentHiddenWalls[index].renderer.enabled = false;
    }
    
    void ShowWall(WallInfo wallInfo){
        if(wallInfo.gameObject == null) return;
        
        if(wallInfo.renderer != null)wallInfo.renderer.enabled = true;
        else wallInfo.gameObject.SetActive(true);
    }

    void DisableAllWalls(){
        foreach(GameObject wall in walls){
            if(wall != null) wall.SetActive(false);
        }
    }
    
    bool IsWallVisible(GameObject wall){
        if(wall == null) return false;
        
        Renderer renderer = wall.GetComponent<Renderer>();
        if(renderer != null) return renderer.enabled;
        return wall.activeSelf;
    }
    
    GameObject GetWallHitByRay(Transform rayOrigin, int sideIndex){
        if(rayOrigin == null) return null;
        
        RaycastHit hit;
        if(Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, rayDistance, wallLayer)){
            GameObject wall = FindInWallList(hit.collider.gameObject);
            
            if(sideIndex == 0) leftRayColor = (wall != null) ? Color.red : Color.yellow;
            else rightRayColor = (wall != null) ? Color.red : Color.yellow;
            
            return wall;
        }
        
        if(sideIndex == 0) leftRayColor = Color.green;
        else rightRayColor = Color.green;
        
        return null;
    }
    
    GameObject FindInWallList(GameObject hitObject){
        foreach(GameObject wall in walls) if(wall == hitObject) return wall;
        return null;
    }
    
    void DrawDebugRays(){
        if(leftRaycast != null) Debug.DrawRay(leftRaycast.position, leftRaycast.forward * rayDistance, leftRayColor);
        if(rightRaycast != null) Debug.DrawRay(rightRaycast.position, rightRaycast.forward * rayDistance, rightRayColor);
    }
}