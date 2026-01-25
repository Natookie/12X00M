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

    void Update(){
        //Open Inventory
        if(Keyboard.current.tabKey.isPressed) openCatalogBtn.ActivateCatalogPanel(true);

        //Get Inventory selection
    }
}