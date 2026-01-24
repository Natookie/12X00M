using UnityEngine;
using System.Collections.Generic;
using Nova;

public class CatalogEditor : MonoBehaviour
{
    [ContextMenu("Assign Catalog Items")]
    public void AssignItems(){
        CatalogItem[] catalogItems = GetComponentsInChildren<CatalogItem>(true);
        
        foreach(CatalogItem item in catalogItems){
            if(item.Data == null){
                Debug.LogWarning($"CatalogItem: {item.name} has no FurnitureData assigned");
                continue;
            }
            
            UpdateCatalogItemUI(item);
        }
        Debug.Log($"Assigned {catalogItems.Length} catalog items");
    }
    
    void UpdateCatalogItemUI(CatalogItem catalogItem){
        FurnitureData data = catalogItem.Data;
        
        Transform itemTransform = catalogItem.transform;
        if(itemTransform.childCount > 0){
            Transform iconChild = itemTransform.GetChild(0);
            UIBlock2D uiBlock = iconChild.GetComponent<UIBlock2D>();
            if(uiBlock != null && data.furnitureIcon != null) uiBlock.SetImage(data.furnitureIcon as Sprite);
        }
        
        if(itemTransform.childCount > 1){
            Transform textChild = itemTransform.GetChild(1);
            TextBlock textBlock = textChild.GetComponent<TextBlock>();
            if(textBlock != null) textBlock.Text = data.ItemName;
        }
        
        if(itemTransform.childCount > 2){
            Transform costChild = itemTransform.GetChild(2);
            TextBlock costText = costChild.GetComponent<TextBlock>();
            if(costText != null) costText.Text = $"${data.Cost}";
        }
    }
    
    [ContextMenu("Clear Catalog Items")]
    public void ClearItems(){
        CatalogItem[] catalogItems = GetComponentsInChildren<CatalogItem>(true);
        
        foreach(CatalogItem item in catalogItems) ClearItemUI(item);
        
        Debug.Log($"Cleared {catalogItems.Length} catalog items");
    }
    
    void ClearItemUI(CatalogItem catalogItem){
        Transform itemTransform = catalogItem.transform;
        
        if(itemTransform.childCount > 0){
            Transform iconChild = itemTransform.GetChild(0);
            UIBlock2D uiBlock = iconChild.GetComponent<UIBlock2D>();
            if(uiBlock != null) uiBlock.SetImage(null as Texture2D);
        }
        
        if(itemTransform.childCount > 1){
            Transform textChild = itemTransform.GetChild(1);
            TextBlock textBlock = textChild.GetComponent<TextBlock>();
            if(textBlock != null) textBlock.Text = "";
        }
    }
}