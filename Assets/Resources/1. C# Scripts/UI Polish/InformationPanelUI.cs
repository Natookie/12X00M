using UnityEngine;
using Nova;
using System.Collections;
using System.Collections.Generic;

public class InformationPanelUI : MonoBehaviour
{
    [Header("VISUALS")]
    [SerializeField] private SelectionButton[] selectionButtons;
    [SerializeField] private Color32 hoverColor;
    [SerializeField] private Color32 unhoverColor;
    [SerializeField] private Color32 selectedColor;
    [SerializeField] private float colorLerpSpeed = 10f;

    private int currentSelectedIndex = -1;
    private Coroutine colorLerpCoroutine;
    private Dictionary<UIBlock, SelectionButton> buttonToSelectionMap = new Dictionary<UIBlock, SelectionButton>();
    private Dictionary<SelectionButton, Color32> targetColors = new Dictionary<SelectionButton, Color32>();

    public static InformationPanelUI Instance {get; private set;}
    
    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
    }

    void Start(){
        InitializeButtons();
        StartColorLerpCoroutine();
    }
    
    void InitializeButtons(){
        buttonToSelectionMap.Clear();
        targetColors.Clear();
        
        for(int i = 0; i < selectionButtons.Length; i++){
            var selectionButton = selectionButtons[i];
            
            if(selectionButton.Button != null){
                buttonToSelectionMap[selectionButton.Button] = selectionButton;
                selectionButton.Button.Color = unhoverColor;
                targetColors[selectionButton] = unhoverColor;
                
                AddGestureHandlers(selectionButton.Button, i);
                
                if(selectionButton.Panel != null){
                    selectionButton.Panel.gameObject.SetActive(false);
                }
            }
        }
        
        if(selectionButtons.Length > 0) OnButtonClicked(0);
    }
    
    void AddGestureHandlers(UIBlock2D button, int index){
        button.AddGestureHandler<Gesture.OnHover>(evt => OnButtonHover(button));
        button.AddGestureHandler<Gesture.OnUnhover>(evt => OnButtonUnhover(button));
        button.AddGestureHandler<Gesture.OnClick>(evt => OnButtonClicked(index));
    }
    
    void OnButtonHover(UIBlock button){
        if(!buttonToSelectionMap.TryGetValue(button, out var selectionButton)) return;
        if(IsButtonSelected(selectionButton)) return;
        
        targetColors[selectionButton] = hoverColor;
    }
    
    void OnButtonUnhover(UIBlock button){
        if(!buttonToSelectionMap.TryGetValue(button, out var selectionButton)) return;
        if(IsButtonSelected(selectionButton)) return;
        
        targetColors[selectionButton] = unhoverColor;
    }
    
    public void OnButtonClicked(int buttonIndex){
        if(buttonIndex < 0 || buttonIndex >= selectionButtons.Length) return;
        
        var clickedButton = selectionButtons[buttonIndex];
        if(IsButtonSelected(clickedButton)) return;
        
        currentSelectedIndex = buttonIndex;
        UpdatePanels();
        UpdateTargetColors();
    }
    
    bool IsButtonSelected(SelectionButton selectionButton){
        if(currentSelectedIndex < 0 || currentSelectedIndex >= selectionButtons.Length) 
            return false;
        
        return selectionButtons[currentSelectedIndex] == selectionButton;
    }
    
    void UpdatePanels(){
        for(int i = 0; i < selectionButtons.Length; i++){
            var panel = selectionButtons[i].Panel;
            if(panel != null) panel.gameObject.SetActive(i == currentSelectedIndex);
        }
    }
    
    void UpdateTargetColors(){
        for(int i = 0; i < selectionButtons.Length; i++){
            var selectionButton = selectionButtons[i];
            if(selectionButton.Button == null) continue;
            
            targetColors[selectionButton] = (i == currentSelectedIndex) ? selectedColor : unhoverColor;
        }
    }
    
    void StartColorLerpCoroutine(){
        if(colorLerpCoroutine != null) StopCoroutine(colorLerpCoroutine);
        colorLerpCoroutine = StartCoroutine(ColorLerpRoutine());
    }
    
    IEnumerator ColorLerpRoutine(){
        while(true){
            bool needsUpdate = false;
            
            foreach (var kvp in targetColors){
                var selectionButton = kvp.Key;
                var targetColor = kvp.Value;
                
                if(selectionButton.Button == null) continue;
                
                Color32 currentColor = selectionButton.Button.Color;
                
                if(!ColorsEqual(currentColor, targetColor)){
                    selectionButton.Button.Color = Color32.Lerp(
                        currentColor, 
                        targetColor, 
                        Time.deltaTime * colorLerpSpeed
                    );
                    needsUpdate = true;
                }
            }
            
            if(!needsUpdate) yield return new WaitForSeconds(0.1f);
            else yield return null;
        }
    }
    
    bool ColorsEqual(Color32 a, Color32 b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    void OnDestroy(){
        if(colorLerpCoroutine != null) StopCoroutine(colorLerpCoroutine);
    }
}

[System.Serializable]
public class SelectionButton
{
    [SerializeField] private UIBlock2D button;
    [SerializeField] private UIBlock2D panel;

    public UIBlock2D Button => button;
    public UIBlock2D Panel => panel;
}