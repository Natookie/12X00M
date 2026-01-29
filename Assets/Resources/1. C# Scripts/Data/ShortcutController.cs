using UnityEngine;
using UnityEngine.InputSystem;

public class ShortcutController : MonoBehaviour
{
    [Header("KEYCODES")]
    public KeyCode openInventory = KeyCode.Tab;
    public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.A;
    public KeyCode left = KeyCode.S;
    public KeyCode right = KeyCode.D;

    [Header("REFERENCES")]
    [SerializeField] private OpenCatalogButton openCatalogBtn;
    [SerializeField] private InformationPanelUI informationPanel;

    void Update(){
        //Skip dialogue
        if(Keyboard.current.nKey.wasPressedThisFrame){
            DialogueManager.Instance.SkipTyping();
            GameManager.Instance.isInitialized = true;
        }
        if(!GameManager.Instance.isInitialized) return;

        //Open Inventory
        if(Keyboard.current.digit1Key.wasPressedThisFrame) informationPanel.OnButtonClicked(0, true);
        else if(Keyboard.current.digit2Key.wasPressedThisFrame) informationPanel.OnButtonClicked(1, true);
        else if(Keyboard.current.digit3Key.wasPressedThisFrame) informationPanel.OnButtonClicked(2, true);

        //Debug: Add money
        if(Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.mKey.wasPressedThisFrame) MoneyManager.Instance.AddMoney(200);


        //Get Inventory selection
    }
}