using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Nova;

public class CatalogEditor : MonoBehaviour
{
    private List<Transform> originalItemOrder = new List<Transform>();
    
    void start(){
        StoreOriginalOrder();
    }

    #region ASSIGNING ITEM
    [ContextMenu("Assign Catalog Items")]
    public void AssignItems(){
        CatalogItem[] catalogItems = GetComponentsInChildren<CatalogItem>(true);
        
        if(originalItemOrder.Count == 0){
            foreach(CatalogItem item in catalogItems) originalItemOrder.Add(item.transform);
        }
        
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

        if(!string.IsNullOrEmpty(data.FurnitureName)) catalogItem.gameObject.name = data.FurnitureName;
        
        Transform itemTransform = catalogItem.transform;
        if(itemTransform.childCount > 0){
            Transform iconChild = itemTransform.GetChild(0);
            UIBlock2D uiBlock = iconChild.GetComponent<UIBlock2D>();
            if(uiBlock != null && data.furnitureIcon != null) uiBlock.SetImage(data.furnitureIcon as Sprite);
        }
        
        if(itemTransform.childCount > 1){
            Transform textChild = itemTransform.GetChild(1);
            TextBlock textBlock = textChild.GetComponent<TextBlock>();
            if(textBlock != null) textBlock.Text = data.FurnitureName;
        }
        
        if(itemTransform.childCount > 2){
            Transform costChild = itemTransform.GetChild(2);
            TextBlock costText = costChild.GetComponent<TextBlock>();
            if(costText != null) costText.Text = $"${data.furnitureCost}";
        }
    }
    #endregion
    
    #region CLEARING ITEM
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
    #endregion

    #region FILTERING ITEMS
    [ContextMenu("Reset to Original Order")]
    public void ResetToOriginalOrder(){
        if(originalItemOrder.Count == 0){
            Debug.LogWarning("No original order stored. Assign items first.");
            return;
        }
        for(int i = 0; i < originalItemOrder.Count; i++) originalItemOrder[i].SetSiblingIndex(i);
    }
    
    [ContextMenu("Sort by Price (ASC)")] public void SortByPriceAscending(){SortCatalogItems((a, b) => a.Data.furnitureCost.CompareTo(b.Data.furnitureCost));}
    [ContextMenu("Sort by Price (DSC)")] public void SortByPriceDescending(){SortCatalogItems((a, b) => b.Data.furnitureCost.CompareTo(a.Data.furnitureCost));}
    [ContextMenu("Sort by Name (A-Z)")] public void SortByNameAscending(){SortCatalogItems((a, b) => a.Data.furnitureName.CompareTo(b.Data.furnitureName));}
    [ContextMenu("Sort by Name (Z-A)")] public void SortByNameDescending(){SortCatalogItems((a, b) => b.Data.furnitureName.CompareTo(a.Data.furnitureName));}
    [ContextMenu("Sort by Charisma (DSC)")] public void SortByCharismaDescending(){SortCatalogItems((a, b) => b.Data.CharismaContribution.CompareTo(a.Data.CharismaContribution));}
    [ContextMenu("Sort by Comfort (DSC)")] public void SortByComfortDescending(){SortCatalogItems((a, b) => b.Data.ComfortContribution.CompareTo(a.Data.ComfortContribution));}
    [ContextMenu("Sort by Functionality (DSC)")] public void SortByFunctionalityDescending(){SortCatalogItems((a, b) => b.Data.FunctionalityContribution.CompareTo(a.Data.FunctionalityContribution));}
    
    public void FilterByFurnitureType(FurnitureType type){
        List<CatalogItem> allItems = new List<CatalogItem>(GetComponentsInChildren<CatalogItem>(true));
        
        allItems.Sort((a, b) => {
            bool aMatches = a.Data.furnitureType == type;
            bool bMatches = b.Data.furnitureType == type;
            
            if(aMatches && !bMatches) return -1;
            if(!aMatches && bMatches) return 1;
            return 0;
        });
        
        for(int i = 0; i < allItems.Count; i++) allItems[i].transform.SetSiblingIndex(i);
    }
    
    public void FilterByPriceRange(int minPrice, int maxPrice){
        List<CatalogItem> allItems = new List<CatalogItem>(GetComponentsInChildren<CatalogItem>(true));
        
        allItems.Sort((a, b) => {
            bool aInRange = a.Data.furnitureCost >= minPrice && a.Data.furnitureCost <= maxPrice;
            bool bInRange = b.Data.furnitureCost >= minPrice && b.Data.furnitureCost <= maxPrice;
            
            if(aInRange && !bInRange) return -1;
            if(!aInRange && bInRange) return 1;
            
            return a.Data.furnitureCost.CompareTo(b.Data.furnitureCost);
        });
        
        for(int i = 0; i < allItems.Count; i++) allItems[i].transform.SetSiblingIndex(i);
    }
    
    private void SortCatalogItems(System.Comparison<CatalogItem> comparison){
        List<CatalogItem> allItems = new List<CatalogItem>(GetComponentsInChildren<CatalogItem>(true));
        
        allItems.Sort(comparison);
        
        for(int i = 0; i < allItems.Count; i++) allItems[i].transform.SetSiblingIndex(i);
    }
    
    [ContextMenu("Sort by Best Overall Traits")]
    public void SortByBestOverallTraits(){
        SortCatalogItems((a, b) => {
            int aScore = a.Data.CharismaContribution + a.Data.ComfortContribution + a.Data.FunctionalityContribution;
            int bScore = b.Data.CharismaContribution + b.Data.ComfortContribution + b.Data.FunctionalityContribution;
            return bScore.CompareTo(aScore); //Descending
        });
    }
    
    [ContextMenu("Sort by Worst Overall Traits")]
    public void SortByWorstOverallTraits(){
        SortCatalogItems((a, b) => {
            int aScore = a.Data.CharismaContribution + a.Data.ComfortContribution + a.Data.FunctionalityContribution;
            int bScore = b.Data.CharismaContribution + b.Data.ComfortContribution + b.Data.FunctionalityContribution;
            return aScore.CompareTo(bScore); //Ascending
        });
    }
    #endregion
    
    #region UTILITY METHODS
    [ContextMenu("Store Original Order")]
    public void StoreOriginalOrder(){
        originalItemOrder.Clear();
        CatalogItem[] catalogItems = GetComponentsInChildren<CatalogItem>(true);
        
        foreach(CatalogItem item in catalogItems){
            originalItemOrder.Add(item.transform);
        }
        
        Debug.Log($"Stored original order of {originalItemOrder.Count} items");
    }
    
    public void FilterItems(string filterType, bool ascending = true){
        switch(filterType.ToLower()){
            case "price":
                if(ascending) SortByPriceAscending();
                else SortByPriceDescending();
                break;
            case "name":
                if(ascending) SortByNameAscending();
                else SortByNameDescending();
                break;
            case "charisma":
                SortByCharismaDescending();
                break;
            case "comfort":
                SortByComfortDescending();
                break;
            case "functionality":
                SortByFunctionalityDescending();
                break;
            case "overall":
                if(ascending) SortByBestOverallTraits();
                else SortByWorstOverallTraits();
                break;
        }
    }
    #endregion
}