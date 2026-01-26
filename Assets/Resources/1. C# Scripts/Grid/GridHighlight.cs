using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridHighlight : MonoBehaviour
{
    [Header("HIGHLIGHT SETTINGS")]
    [SerializeField] private Color unoccupiedTileColor = Color.cyan;
    [SerializeField] private float highlightAnimationSpeed = 2f;
    [SerializeField] private float highlightIntensity = 0.3f;
    [SerializeField] private float highlightScaleFactor = 1.1f;
    [SerializeField] private float animationSmoothness = 10f;
    [Space(10)]
    [SerializeField] private float waveDuration = 0.3f;
    [SerializeField] private float delayBetweenTiles = 0.05f;
    [SerializeField] private float waveHighlightIntensity = 0.3f;
    [SerializeField] private float maxScaleFactor = 1.1f;
    [SerializeField] private float waveStartDelay = 0.2f;
    [SerializeField] private float waveHoldDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("REFERENCES")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private BuildSystem buildSystem;
    
    private Dictionary<Vector2Int, GameObject> highlightedTiles = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, Vector3> originalScales = new Dictionary<Vector2Int, Vector3>();
    private Dictionary<Vector2Int, Color> originalColors = new Dictionary<Vector2Int, Color>();
    private Dictionary<Vector2Int, Material> originalMaterials = new Dictionary<Vector2Int, Material>();
    
    private bool isAnimating = false;
    private Coroutine animationCoroutine;
    private Coroutine waveAnimationCoroutine;
    
    void OnDisable() => StopAllAnimations();
    
    public void HighlightUnoccupiedTiles(){
        StopAllAnimations();
        
        List<Vector2Int> unoccupiedPositions = buildSystem.GetAllUnoccupiedTilePositions();
        
        highlightedTiles.Clear();
        originalScales.Clear();
        originalColors.Clear();
        originalMaterials.Clear();
        
        foreach(Vector2Int pos in unoccupiedPositions){
            GameObject tile = gridManager.GetTileAtLocalPosition(pos);
            if(tile != null){
                highlightedTiles[pos] = tile;
                originalScales[pos] = tile.transform.localScale;
                
                Renderer renderer = tile.GetComponent<Renderer>();
                if(renderer != null){
                    originalColors[pos] = renderer.material.color;
                    originalMaterials[pos] = renderer.material;
                }
            }
        }
        
        if(animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateTiles());
    }
    
    IEnumerator AnimateTiles(){
        isAnimating = true;
        float time = 0f;
        
        while(isAnimating && highlightedTiles.Count > 0){
            float pulse = Mathf.Sin(time * highlightAnimationSpeed) * 0.5f + 0.5f;
            float scaleMultiplier = Mathf.Lerp(1f, highlightScaleFactor, pulse);
            float colorIntensity = Mathf.Lerp(0.2f, highlightIntensity, pulse);
            
            foreach(var kvp in highlightedTiles){
                if(kvp.Value == null) continue;
                
                Vector2Int pos = kvp.Key;
                GameObject tile = kvp.Value;
                
                if(originalScales.ContainsKey(pos)){
                    Vector3 targetScale = originalScales[pos] * scaleMultiplier;
                    tile.transform.localScale = Vector3.Lerp(
                        tile.transform.localScale, 
                        targetScale, 
                        Time.deltaTime * animationSmoothness
                    );
                }
                
                Renderer renderer = tile.GetComponent<Renderer>();
                if(renderer != null && originalColors.ContainsKey(pos)){
                    Color targetColor = Color.Lerp(
                        originalColors[pos], 
                        unoccupiedTileColor, 
                        colorIntensity
                    );
                    
                    renderer.material.color = Color.Lerp(
                        renderer.material.color, 
                        targetColor, 
                        Time.deltaTime * animationSmoothness
                    );
                }
            }
            
            time += Time.deltaTime;
            yield return null;
        }
        
        RestoreAllTiles();
    }
    
    public void RestoreAllTiles(){
        foreach(var kvp in highlightedTiles){
            if(kvp.Value != null) RestoreTile(kvp.Key, kvp.Value);
        }
        
        highlightedTiles.Clear();
        originalScales.Clear();
        originalColors.Clear();
        originalMaterials.Clear();
        
        isAnimating = false;
    }
    
    void RestoreTile(Vector2Int pos, GameObject tile){
        if(tile == null) return;
        
        if(originalScales.ContainsKey(pos)) tile.transform.localScale = originalScales[pos];
        
        Renderer renderer = tile.GetComponent<Renderer>();
        if(renderer != null && originalColors.ContainsKey(pos)) renderer.material.color = originalColors[pos];
    }
    
    public void StopAllAnimations(){
        isAnimating = false;
        
        if(animationCoroutine != null){
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        if(waveAnimationCoroutine != null){
            StopCoroutine(waveAnimationCoroutine);
            waveAnimationCoroutine = null;
        }
        
        RestoreAllTiles();
    }

    public void AnimateWaveToBlue(){
        if(waveAnimationCoroutine != null) StopCoroutine(waveAnimationCoroutine);
        
        List<Vector2Int> unoccupiedPositions = buildSystem.GetAllUnoccupiedTilePositions();
        if(unoccupiedPositions.Count == 0) return;
        
        Dictionary<Vector2Int, Vector3> waveScales = new Dictionary<Vector2Int, Vector3>();
        Dictionary<Vector2Int, Color> waveColors = new Dictionary<Vector2Int, Color>();
        
        Vector2Int centerPos = GetCenterPosition(unoccupiedPositions);
        
        unoccupiedPositions.Sort((a, b) => {
            float distA = Vector2.Distance(a, centerPos);
            float distB = Vector2.Distance(b, centerPos);
            return distA.CompareTo(distB);
        });
        
        foreach(Vector2Int pos in unoccupiedPositions){
            GameObject tile = gridManager.GetTileAtLocalPosition(pos);
            if(tile != null){
                waveScales[pos] = tile.transform.localScale;
                
                Renderer renderer = tile.GetComponent<Renderer>();
                if(renderer != null) waveColors[pos] = renderer.material.color;
            }
        }
        
        isAnimating = true;
        waveAnimationCoroutine = StartCoroutine(WaveAnimationRoutine(unoccupiedPositions, waveScales, waveColors));
    }

    Vector2Int GetCenterPosition(List<Vector2Int> positions){
        if(positions.Count == 0) return Vector2Int.zero;
        
        Vector2 sum = Vector2.zero;
        foreach(Vector2Int pos in positions) sum += (Vector2)pos;
        return Vector2Int.RoundToInt(sum / positions.Count);
    }

    IEnumerator WaveAnimationRoutine(List<Vector2Int> positions, Dictionary<Vector2Int, Vector3> waveScales, Dictionary<Vector2Int, Color> waveColors){
        yield return new WaitForSeconds(waveStartDelay);
        
        List<Coroutine> tileAnimations = new List<Coroutine>();
        
        foreach(Vector2Int pos in positions){
            GameObject tile = gridManager.GetTileAtLocalPosition(pos);
            if(tile == null) continue;
            
            Coroutine tileAnim = StartCoroutine(AnimateSingleTileRoutine(pos, tile, waveScales, waveColors));
            tileAnimations.Add(tileAnim);
            yield return new WaitForSeconds(delayBetweenTiles);
        }
        
        foreach(Coroutine anim in tileAnimations){
            if(anim != null) yield return anim;
        }
        
        yield return new WaitForSeconds(waveHoldDuration);
        yield return StartCoroutine(FadeOutAllTiles(positions, waveScales, waveColors));
        
        waveAnimationCoroutine = null;
        isAnimating = false;
    }

    IEnumerator AnimateSingleTileRoutine(Vector2Int pos, GameObject tile, Dictionary<Vector2Int, Vector3> waveScales, Dictionary<Vector2Int, Color> waveColors){
        float elapsed = 0f;
        Vector3 originalScale = waveScales.ContainsKey(pos) ? waveScales[pos] : tile.transform.localScale;
        Color originalColor = waveColors.ContainsKey(pos) ? waveColors[pos] : Color.white;
        
        Renderer renderer = tile.GetComponent<Renderer>();
        if(renderer == null) yield break;
        
        while(elapsed < waveDuration){
            float t = elapsed / waveDuration;
            
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);
            float scaleMultiplier = Mathf.Lerp(1f, maxScaleFactor, easedT);
            tile.transform.localScale = originalScale * scaleMultiplier;
            
            Color targetColor = Color.Lerp(originalColor, unoccupiedTileColor, waveHighlightIntensity);
            Color currentColor = Color.Lerp(originalColor, targetColor, t);
            renderer.material.color = currentColor;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Color finalColor = Color.Lerp(originalColor, unoccupiedTileColor, waveHighlightIntensity);
        tile.transform.localScale = originalScale;
        renderer.material.color = finalColor;
    }

    IEnumerator FadeOutAllTiles(List<Vector2Int> positions, Dictionary<Vector2Int, Vector3> waveScales, Dictionary<Vector2Int, Color> waveColors){
        float fadeElapsed = 0f;
        
        Dictionary<Vector2Int, Color> startColors = new Dictionary<Vector2Int, Color>();
        foreach(Vector2Int pos in positions){
            GameObject tile = gridManager.GetTileAtLocalPosition(pos);
            if(tile != null){
                Renderer renderer = tile.GetComponent<Renderer>();
                if(renderer != null) startColors[pos] = renderer.material.color;
            }
        }
        
        while(fadeElapsed < fadeOutDuration){
            float t = fadeElapsed / fadeOutDuration;
            
            foreach(Vector2Int pos in positions){
                GameObject tile = gridManager.GetTileAtLocalPosition(pos);
                if(tile == null) continue;
                
                Renderer renderer = tile.GetComponent<Renderer>();
                if(renderer == null || !startColors.ContainsKey(pos) || !waveColors.ContainsKey(pos)) continue;
                
                Color startColor = startColors[pos];
                Color targetColor = waveColors[pos];
                Color currentColor = Color.Lerp(startColor, targetColor, t);
                renderer.material.color = currentColor;
                
                if(waveScales.ContainsKey(pos)){
                    float scaleT = Mathf.Lerp(1f, 0.95f, Mathf.Sin(t * Mathf.PI));
                    tile.transform.localScale = waveScales[pos] * scaleT;
                }
            }
            
            fadeElapsed += Time.deltaTime;
            yield return null;
        }
        
        foreach(Vector2Int pos in positions){
            GameObject tile = gridManager.GetTileAtLocalPosition(pos);
            if(tile == null) continue;
            
            if(waveScales.ContainsKey(pos)) tile.transform.localScale = waveScales[pos];
            
            Renderer renderer = tile.GetComponent<Renderer>();
            if(renderer != null && waveColors.ContainsKey(pos)) renderer.material.color = waveColors[pos];
        }
    }
    
    public bool IsAnimating => isAnimating;
    public int GetHighlightedTileCount() => highlightedTiles.Count;
}