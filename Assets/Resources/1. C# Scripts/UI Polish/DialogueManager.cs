using UnityEngine;
using Nova;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public enum CharacterType
    {
        Npc,
        Grandma,
        Player
    };

    [Header("DIALOGUE CONFIG")]
    [SerializeField] private CharacterConfig[] configs = new CharacterConfig[3];
    [SerializeField] private float typingSpeed = 30f;
    [SerializeField] private float punctuationDelay = 0.3f;
    [SerializeField] private float expressionBlinkInterval = 0.2f;
    [SerializeField] private float tildePauseDuration = 1f;
    [Space(10)]
    [SerializeField] private float idleDialogueCooldown = 10f;
    [SerializeField] private float lastIdleDialogueTime = 0f;

    [Header("EXTRAS")]
    [SerializeField] private UIBlock2D skipButton;
    [SerializeField] private Color32 skipHoverColor;
    [SerializeField] private Color32 skipUnhoverColor;

    [Header("AUDIO SETTINGS")]
    [SerializeField] private string skipPressedSFXKey = "buttonPress";

    public ItemView dialogueItemVisual;
    private DialogueItemVisual visual;

    private Dictionary<CharacterType, CharacterConfig> configDictionary;
    private Coroutine typingRoutine;
    private Coroutine expressionRoutine;
    private bool isTyping = false;
    private bool skipAllDialogues = false;

    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
    }

    void Start(){
        visual = dialogueItemVisual.Visuals as DialogueItemVisual;
        if(visual == null) Debug.LogError("dialogueItemVisual.Visuals is not a DialogueItemVisual!");
        InitializeConfigDictionary();

        skipButton.AddGestureHandler<Gesture.OnPress>(skipClick);
        skipButton.AddGestureHandler<Gesture.OnHover>(skipHover);
        skipButton.AddGestureHandler<Gesture.OnUnhover>(skipUnhover);
    }

    void Update(){
        if(!GameManager.Instance.isInitialized) return;
        if(skipButton != null) skipButton.transform.position = new Vector3(-1000, -1000, -1000);

        if(!IsTypingActive() && Time.time - lastIdleDialogueTime > idleDialogueCooldown){
            string randomIdleLine = DialogueLib.GetRandomDialogue("Idle");
            SetDialogue(
                DialogueManager.CharacterType.Grandma,
                randomIdleLine
            );
            lastIdleDialogueTime = Time.time;
        }
    }

    void InitializeConfigDictionary(){
        configDictionary = new Dictionary<CharacterType, CharacterConfig>();
        foreach(CharacterConfig config in configs){
            if(config != null){
                if(configDictionary.ContainsKey(config.characterType)) Debug.LogWarning($"Duplicate CharacterType found: {config.characterType}");
                else configDictionary[config.characterType] = config;
            }
        }
        foreach(CharacterType type in System.Enum.GetValues(typeof(CharacterType))){
            if(!configDictionary.ContainsKey(type)) Debug.LogWarning($"No CharacterConfig set up for type: {type}");
        }
    }

    public void SetDialogue(CharacterType type, string content, int expressionIndex = 0){
        if(visual == null) return;
        
        if(!configDictionary.TryGetValue(type, out CharacterConfig config)){
            Debug.LogError($"No CharacterConfig found for type: {type}");
            return;
        }
        if(typingRoutine != null) StopCoroutine(typingRoutine);
        if(expressionRoutine != null) StopCoroutine(expressionRoutine);

        isTyping = false;
        visual.dialogueName.Text = config.characterName;
        visual.dialogueContent.Text = "";
        if(config.characterSprite.Length > 0) visual.dialogueChar.SetImage(config.characterSprite[Mathf.Min(expressionIndex, config.characterSprite.Length - 1)]);

        if(skipAllDialogues) visual.dialogueContent.Text = StripTags(content);
        else typingRoutine = StartCoroutine(TypeText(content, config, expressionIndex));
    }

    private string StripTags(string text){
        string result = text.Replace("~", "");
        return result;
    }

    private IEnumerator TypeText(string text, CharacterConfig config, int expressionIndex){
        if(skipAllDialogues){
            visual.dialogueContent.Text = StripTags(text);
            yield break;
        }
        
        isTyping = true;
        if(config.characterSprite.Length > 1) expressionRoutine = StartCoroutine(BlinkExpression(config));
        
        int i = 0;
        string displayedText = "";
        while(i < text.Length && !skipAllDialogues){
            if(text[i] == '<'){
                int tagEnd = text.IndexOf('>', i);
                if(tagEnd != -1){
                    string tagContent = text.Substring(i, tagEnd - i + 1);
                    displayedText += tagContent;
                    visual.dialogueContent.Text = displayedText;
                    i = tagEnd + 1;
                    continue;
                }
            }
            else if(text[i] == '~'){
                yield return new WaitForSeconds(tildePauseDuration);
                i++;
                continue;
            }
            
            displayedText += text[i];
            visual.dialogueContent.Text = displayedText;
            if(text[i] == ',' || text[i] == '.') yield return new WaitForSeconds(punctuationDelay);
            else yield return new WaitForSeconds(1f / typingSpeed);
            i++;
        }
        
        if(skipAllDialogues){
            visual.dialogueContent.Text = StripTags(text);
        }
        
        if(!skipAllDialogues){
            yield return new WaitForSeconds(1f);
            if(config.characterSprite.Length > 0) visual.dialogueChar.SetImage(config.characterSprite[Mathf.Min(expressionIndex, config.characterSprite.Length - 1)]);
        }
        
        isTyping = false;
    }

    private IEnumerator BlinkExpression(CharacterConfig config){
        int index = 0;
        while(isTyping && !skipAllDialogues){
            index = index == 0 ? 1 : 0;
            visual.dialogueChar.SetImage(config.characterSprite[index]);
            yield return new WaitForSeconds(expressionBlinkInterval);
        }
    }

    public void SkipTyping(){
        skipAllDialogues = true;
        if(typingRoutine != null) StopCoroutine(typingRoutine);
        if(expressionRoutine != null) StopCoroutine(expressionRoutine);
        isTyping = false;
    }

    public void ResetSkip(){
        skipAllDialogues = false;
    }

    public bool IsTypingActive() => isTyping && !skipAllDialogues;

    #region SKIP BUTTON
    void skipClick(Gesture.OnPress evt){
        SkipTyping();
        GameManager.Instance.isInitialized = true;

        AudioManager.Instance.PlaySFX(skipPressedSFXKey);
    }
    void skipHover(Gesture.OnHover evt) => StartCoroutine(LerpSkipButton(skipHoverColor, 1.1f));
    void skipUnhover(Gesture.OnUnhover evt) => StartCoroutine(LerpSkipButton(skipUnhoverColor, 1f));
    
    IEnumerator LerpSkipButton(Color32 targetColor, float targetScale){
        if(skipButton == null) yield break;

        float duration = 0.2f;
        float elapsed = 0f;
        
        Color32 startColor = skipButton.Color;
        Vector3 startScale = skipButton.transform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, targetScale);
        
        while(elapsed < duration && skipButton != null){
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            skipButton.Color = Color32.Lerp(startColor, targetColor, t);
            skipButton.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            yield return null;
        }
        
        skipButton.Color = targetColor;
        skipButton.transform.localScale = endScale;
    }
    #endregion
}

[System.Serializable]
public class CharacterConfig
{
    public Sprite[] characterSprite = new Sprite[2];
    public string characterName;
    public DialogueManager.CharacterType characterType = DialogueManager.CharacterType.Npc;
}