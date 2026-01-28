using UnityEngine;
using Nova;
using System.Collections;

public class CatalogUI : MonoBehaviour
{
    [SerializeField] private CatalogEditor catalogEditor;

    [Header("TRAITS INFO")]
    [SerializeField] private TextBlock charismaText;
    [SerializeField] private TextBlock comfortText;
    [SerializeField] private TextBlock funcText;

    [Header("COLOR CONFIG")]
    [SerializeField] private Color32 borderActiveColor;
    [SerializeField] private Color32 borderInactiveColor;
    [SerializeField] private Color32 arrowAscColor;
    [SerializeField] private Color32 arrowDescColor;
    [SerializeField] private Color32 resetNormalColor = new Color32(70, 86, 165, 255);
    [SerializeField] private Color32 resetHoverColor = new Color32(73, 149, 243, 255);

    [Header("UPPER FILTER")]
    [SerializeField] private FilterButton charismaFilter;
    [SerializeField] private FilterButton comfortFilter;
    [SerializeField] private FilterButton functionalityFilter;

    [Header("BOTTOM FILTER")]
    [SerializeField] private SortButton priceFilter;
    [SerializeField] private SortButton nameFilter;
    [SerializeField] private SortButton overallFilter;

    [Header("RESET FILTER")]
    [SerializeField] private UIBlock2D resetFilterBlock;
    private Vector3 resetOriginalRotation;
    private Color32 resetCurrentColor;
    private Coroutine resetAnimationRoutine;

    private FilterButton selectedUpperFilter;
    private SortButton selectedBottomFilter;
    private Coroutine animationRoutine;

    [System.Serializable]
    public class FilterButton
    {
        public UIBlock2D block;
        public UIBlock2D blockContainer;
        public Vector3 originalPosition;
        public bool isSelected;
        
        public void StoreOriginalPosition(){
            if(block != null) originalPosition = block.transform.localPosition;
        }
    }

    [System.Serializable]
    public class SortButton
    {
        public UIBlock2D block;
        public UIBlock2D blockContainer;
        public UIBlock2D arrow;
        public Vector3 originalPosition;
        public Vector3 arrowOriginalRotation;
        public bool isSelected;
        public bool isAscending = true;
        
        public void StoreOriginalPositions(){
            if(block != null) originalPosition = block.transform.localPosition;
            if(arrow != null) arrowOriginalRotation = arrow.transform.localEulerAngles;
        }
    }

    void Start(){
        StoreOriginalStates();
        ResetAllFilters();
        
        charismaFilter.blockContainer.AddGestureHandler<Gesture.OnPress>(CharismaClick);
        charismaFilter.blockContainer.AddGestureHandler<Gesture.OnHover>(CharismaHover);
        charismaFilter.blockContainer.AddGestureHandler<Gesture.OnUnhover>(CharismaUnhover);
        
        comfortFilter.blockContainer.AddGestureHandler<Gesture.OnPress>(ComfortClick);
        comfortFilter.blockContainer.AddGestureHandler<Gesture.OnHover>(ComfortHover);
        comfortFilter.blockContainer.AddGestureHandler<Gesture.OnUnhover>(ComfortUnhover);
        
        functionalityFilter.blockContainer.AddGestureHandler<Gesture.OnPress>(FunctionalityClick);
        functionalityFilter.blockContainer.AddGestureHandler<Gesture.OnHover>(FunctionalityHover);
        functionalityFilter.blockContainer.AddGestureHandler<Gesture.OnUnhover>(FunctionalityUnhover);
        
        priceFilter.blockContainer.AddGestureHandler<Gesture.OnPress>(PriceClick);
        priceFilter.blockContainer.AddGestureHandler<Gesture.OnHover>(PriceHover);
        priceFilter.blockContainer.AddGestureHandler<Gesture.OnUnhover>(PriceUnhover);
        
        nameFilter.blockContainer.AddGestureHandler<Gesture.OnPress>(NameClick);
        nameFilter.blockContainer.AddGestureHandler<Gesture.OnHover>(NameHover);
        nameFilter.blockContainer.AddGestureHandler<Gesture.OnUnhover>(NameUnhover);
        
        overallFilter.blockContainer.AddGestureHandler<Gesture.OnPress>(OverallClick);
        overallFilter.blockContainer.AddGestureHandler<Gesture.OnHover>(OverallHover);
        overallFilter.blockContainer.AddGestureHandler<Gesture.OnUnhover>(OverallUnhover);
        
        resetFilterBlock.AddGestureHandler<Gesture.OnPress>(ResetClick);
        resetFilterBlock.AddGestureHandler<Gesture.OnHover>(ResetHover);
        resetFilterBlock.AddGestureHandler<Gesture.OnUnhover>(ResetUnhover);
    }

    public void UpdateTraitDisplay(int charisma, int comfort, int func){
        charismaText.Text = charisma.ToString();
        comfortText.Text = comfort.ToString();
        funcText.Text = func.ToString();
    }

    void StoreOriginalStates(){
        charismaFilter.StoreOriginalPosition();
        comfortFilter.StoreOriginalPosition();
        functionalityFilter.StoreOriginalPosition();
        
        priceFilter.StoreOriginalPositions();
        nameFilter.StoreOriginalPositions();
        overallFilter.StoreOriginalPositions();
        
        if(resetFilterBlock != null){
            resetOriginalRotation = resetFilterBlock.transform.localEulerAngles;
            resetCurrentColor = resetNormalColor;
            resetFilterBlock.Color = resetCurrentColor;
        }
    }

    void DeselectUpperFilter(){
        if(selectedUpperFilter != null){
            selectedUpperFilter.isSelected = false;
            selectedUpperFilter.block.Border.Color = borderInactiveColor;
            selectedUpperFilter.block.transform.localPosition = selectedUpperFilter.originalPosition;

            selectedUpperFilter = null;
        }
    }

    void DeselectBottomFilter(){
        if(selectedBottomFilter != null){
            selectedBottomFilter.isSelected = false;
            selectedBottomFilter.block.Border.Color = borderInactiveColor;
            selectedBottomFilter.block.transform.localPosition = selectedBottomFilter.originalPosition;
            
            if(selectedBottomFilter.arrow != null){
                selectedBottomFilter.arrow.Color = arrowAscColor;
                selectedBottomFilter.arrow.transform.localEulerAngles = selectedBottomFilter.arrowOriginalRotation;
            }
            
            selectedBottomFilter = null;
        }
    }

    #region UPPER FILTER HANDLERS
    void CharismaClick(Gesture.OnPress evt){
        if(selectedUpperFilter == charismaFilter) return;
        DeselectUpperFilter();
        DeselectBottomFilter();
        
        selectedUpperFilter = charismaFilter;
        charismaFilter.isSelected = true;
        charismaFilter.block.Border.Color = borderActiveColor;
        charismaFilter.block.transform.localPosition = charismaFilter.originalPosition;
        
        catalogEditor.SortByCharismaDescending();
    }
    
    void CharismaHover(Gesture.OnHover evt){
        if(charismaFilter.isSelected) return;
        charismaFilter.block.Border.Color = borderActiveColor;
        charismaFilter.block.transform.localPosition = charismaFilter.originalPosition + new Vector3(0, 10, 0);
    }
    
    void CharismaUnhover(Gesture.OnUnhover evt){
        if(charismaFilter.isSelected) return;
        charismaFilter.block.Border.Color = borderInactiveColor;
        charismaFilter.block.transform.localPosition = charismaFilter.originalPosition;
    }
    
    void ComfortClick(Gesture.OnPress evt){
        if(selectedUpperFilter == comfortFilter) return;
        DeselectUpperFilter();
        DeselectBottomFilter();
        
        selectedUpperFilter = comfortFilter;
        comfortFilter.isSelected = true;
        comfortFilter.block.Border.Color = borderActiveColor;
        comfortFilter.block.transform.localPosition = comfortFilter.originalPosition;
        
        catalogEditor.SortByComfortDescending();
    }
    
    void ComfortHover(Gesture.OnHover evt){
        if(comfortFilter.isSelected) return;
        comfortFilter.block.Border.Color = borderActiveColor;
        comfortFilter.block.transform.localPosition = comfortFilter.originalPosition + new Vector3(0, 10, 0);
    }
    
    void ComfortUnhover(Gesture.OnUnhover evt){
        if(comfortFilter.isSelected) return;
        comfortFilter.block.Border.Color = borderInactiveColor;
        comfortFilter.block.transform.localPosition = comfortFilter.originalPosition;
    }
    
    void FunctionalityClick(Gesture.OnPress evt){
        if(selectedUpperFilter == functionalityFilter) return;
        DeselectUpperFilter();
        DeselectBottomFilter();
        
        selectedUpperFilter = functionalityFilter;
        functionalityFilter.isSelected = true;
        functionalityFilter.block.Border.Color = borderActiveColor;
        functionalityFilter.block.transform.localPosition = functionalityFilter.originalPosition;
        
        catalogEditor.SortByFunctionalityDescending();
    }
    
    void FunctionalityHover(Gesture.OnHover evt){
        if(functionalityFilter.isSelected) return;
        functionalityFilter.block.Border.Color = borderActiveColor;
        functionalityFilter.block.transform.localPosition = functionalityFilter.originalPosition + new Vector3(0, 10, 0);
    }
    
    void FunctionalityUnhover(Gesture.OnUnhover evt){
        if(functionalityFilter.isSelected) return;
        functionalityFilter.block.Border.Color = borderInactiveColor;
        functionalityFilter.block.transform.localPosition = functionalityFilter.originalPosition;
    }
    #endregion

    #region BOTTOM FILTER HANDLERS
    void PriceClick(Gesture.OnPress evt){
        if(selectedBottomFilter != priceFilter){
            DeselectUpperFilter();
            DeselectBottomFilter();
            
            selectedBottomFilter = priceFilter;
            priceFilter.isSelected = true;
            priceFilter.block.Border.Color = borderActiveColor;
        }
        
        priceFilter.isAscending = !priceFilter.isAscending;
        UpdateArrow(priceFilter);
        
        catalogEditor.FilterItems("price", priceFilter.isAscending);
    }
    
    void PriceHover(Gesture.OnHover evt){
        priceFilter.block.Border.Color = borderActiveColor;
        priceFilter.block.transform.localPosition = priceFilter.originalPosition + new Vector3(0, 5, 0);
    }
    
    void PriceUnhover(Gesture.OnUnhover evt){
        if(!priceFilter.isSelected){
            priceFilter.block.Border.Color = borderInactiveColor;
            priceFilter.block.transform.localPosition = priceFilter.originalPosition;
        }else priceFilter.block.transform.localPosition = priceFilter.originalPosition;
    }
    
    void NameClick(Gesture.OnPress evt){
        if(selectedBottomFilter != nameFilter){
            DeselectUpperFilter();
            DeselectBottomFilter();
            
            selectedBottomFilter = nameFilter;
            nameFilter.isSelected = true;
            nameFilter.block.Border.Color = borderActiveColor;
        }
        
        nameFilter.isAscending = !nameFilter.isAscending;
        UpdateArrow(nameFilter);
        
        catalogEditor.FilterItems("name", nameFilter.isAscending);
    }
    
    void NameHover(Gesture.OnHover evt){
        nameFilter.block.Border.Color = borderActiveColor;
        nameFilter.block.transform.localPosition = nameFilter.originalPosition + new Vector3(0, 5, 0);
    }
    
    void NameUnhover(Gesture.OnUnhover evt){
        if(!nameFilter.isSelected){
            nameFilter.block.Border.Color = borderInactiveColor;
            nameFilter.block.transform.localPosition = nameFilter.originalPosition;
        }else nameFilter.block.transform.localPosition = nameFilter.originalPosition;
    }
    
    void OverallClick(Gesture.OnPress evt){
        if(selectedBottomFilter != overallFilter){
            DeselectUpperFilter();
            DeselectBottomFilter();
            
            selectedBottomFilter = overallFilter;
            overallFilter.isSelected = true;
            overallFilter.block.Border.Color = borderActiveColor;
        }
        
        overallFilter.isAscending = !overallFilter.isAscending;
        UpdateArrow(overallFilter);
        
        catalogEditor.FilterItems("overall", overallFilter.isAscending);
    }
    
    void OverallHover(Gesture.OnHover evt){
        overallFilter.block.Border.Color = borderActiveColor;
        overallFilter.block.transform.localPosition = overallFilter.originalPosition + new Vector3(0, 5, 0);
    }
    
    void OverallUnhover(Gesture.OnUnhover evt){
        if(!overallFilter.isSelected){
            overallFilter.block.Border.Color = borderInactiveColor;
            overallFilter.block.transform.localPosition = overallFilter.originalPosition;
        }else overallFilter.block.transform.localPosition = overallFilter.originalPosition;
    }
    #endregion

    void UpdateArrow(SortButton sortButton){
        if(sortButton.arrow == null) return;
        
        if(sortButton.isAscending){
            sortButton.arrow.Color = arrowAscColor;
            sortButton.arrow.transform.localEulerAngles = sortButton.arrowOriginalRotation;
        }else{
            sortButton.arrow.Color = arrowDescColor;
            sortButton.arrow.transform.localEulerAngles = sortButton.arrowOriginalRotation + new Vector3(0, 0, 180);
        }
    }

    #region RESET FILTER HANDLERS
    void ResetClick(Gesture.OnPress evt){
        ResetAllFilters();
    }
    
    void ResetHover(Gesture.OnHover evt){
        if(resetAnimationRoutine != null) StopCoroutine(resetAnimationRoutine);
        resetAnimationRoutine = StartCoroutine(AnimateResetHover(true));
    }
    
    void ResetUnhover(Gesture.OnUnhover evt){
        if(resetAnimationRoutine != null) StopCoroutine(resetAnimationRoutine);
        resetAnimationRoutine = StartCoroutine(AnimateResetHover(false));
    }
    
    void ResetAllFilters(){
        DeselectUpperFilter();
        DeselectBottomFilter();
        
        priceFilter.isAscending = true;
        nameFilter.isAscending = true;
        overallFilter.isAscending = true;
        
        UpdateArrow(priceFilter);
        UpdateArrow(nameFilter);
        UpdateArrow(overallFilter);
        
        catalogEditor.ResetToOriginalOrder();
    }
    
    IEnumerator AnimateResetHover(bool isHovering){
        float duration = 0.2f;
        float elapsed = 0f;
        
        Color32 startColor = resetCurrentColor;
        Color32 targetColor = isHovering ? resetHoverColor : resetNormalColor;
        
        Vector3 startRotation = resetFilterBlock.transform.localEulerAngles;
        Vector3 targetRotation = isHovering ? resetOriginalRotation + new Vector3(0, 0, -180) : resetOriginalRotation;
        
        while(elapsed < duration){
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            resetFilterBlock.Color = Color32.Lerp(startColor, targetColor, t);
            resetFilterBlock.transform.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, t);
            
            yield return null;
        }
        
        resetFilterBlock.Color = targetColor;
        resetFilterBlock.transform.localEulerAngles = targetRotation;
        resetCurrentColor = targetColor;
    }
    #endregion
}