using Nova;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Interactable))]
public class MenuButton : MonoBehaviour
{
    public UnityEvent OnButtonPressed;

    [SerializeField] private UIBlock2D root;
    [SerializeField] private TextBlock text;
    // Animation variables
    [SerializeField] private Color normalBlockColor;
    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color hoverBlockColor;
    [SerializeField] private Color hoverTextColor;
    [SerializeField] private Color pressedBlockColor;
    [SerializeField] private Color pressedTextColor;
    // Audio variables
    [SerializeField] private string normalAudioKey;
    [SerializeField] private string hoverAudioKey;
    [SerializeField] private string pressedAudioKey;

    void Start(){
        root.AddGestureHandler<Gesture.OnPress>(OnClick);
        root.AddGestureHandler<Gesture.OnHover>(OnHover);
        root.AddGestureHandler<Gesture.OnUnhover>(OnUnhover);
    }

    void OnEnable()
    {
        ResetButton();
    }

    public void ResetButton()
    {
        root.Color = normalBlockColor;
        text.Color = normalTextColor;
        root.Shadow.Enabled = false;
    }

    void OnClick(Gesture.OnPress evt)
    {
        root.Color = pressedBlockColor;
        text.Color = pressedTextColor;
        root.Shadow.Enabled = false;
        if(pressedAudioKey != "") AudioManager.Instance.PlaySFX(pressedAudioKey);
        OnButtonPressed.Invoke();
    }

    void OnHover(Gesture.OnHover evt)
    {
        root.Color = hoverBlockColor;
        text.Color = hoverTextColor;
        root.Shadow.Enabled = true;
        if(hoverAudioKey != "") AudioManager.Instance.PlaySFX(hoverAudioKey);
    }

    void OnUnhover(Gesture.OnUnhover evt)
    {
        root.Color = normalBlockColor;
        text.Color = normalTextColor;
        root.Shadow.Enabled = false;
        if(normalAudioKey != "") AudioManager.Instance.PlaySFX(normalAudioKey);
    }
}
