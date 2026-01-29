using UnityEngine;
using System.Collections.Generic;
using Nova;

public class InteractableManager : MonoBehaviour
{
    [SerializeField] private GameObject skipButton;
    [SerializeField] private GameObject vignetteBlock;
    
    void Start(){
        Interactable[] allInteractables = FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach(Interactable interactable in allInteractables){
            if(interactable.gameObject == skipButton) continue;
            interactable.enabled = false;
        }
    }
    
    public void EnableAllInteractables(){
        Interactable[] allInteractables = FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(Interactable interactable in allInteractables){
            if(interactable.gameObject == vignetteBlock) continue;
            interactable.enabled = true;
        }
    }
}