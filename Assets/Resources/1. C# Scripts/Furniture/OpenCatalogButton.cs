using UnityEngine;
using UnityEngine.InputSystem;
using Nova;

[RequireComponent(typeof(Interactable))]
public class OpenCatalogButton : MonoBehaviour
{
    [SerializeField] private UIBlock2D root;
    [SerializeField] private GameObject catalogPanel;
    [SerializeField] private InformationPanelUI infoPanel;

    public static OpenCatalogButton Instance {get; private set;}

    void Awake(){
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //Harusnya udah ga kepake lagi
    //Cuman gw ga hapus, karena ada objek yg referensiin script ini
    //Gw males cari.
    public bool IsPanelActive() => catalogPanel.activeSelf;
    public void ActivateCatalogPanel(bool type) => catalogPanel.SetActive(type);   
}