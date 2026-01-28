using UnityEngine;
using System.Collections.Generic;
using Nova;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance {get; private set;}
    [SerializeField] private int currentMoney;
    [SerializeField] private int moneyPerUnoccupiedTile = 5;

    [SerializeField] private MoneyUI moneyUI;

    public int Money => currentMoney;

    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
    }

    public void AddMoney(int value){
        currentMoney += value;
        currentMoney = Mathf.Max(0, currentMoney);

        moneyUI.UpdateMoneyDisplay(currentMoney);
    }

    public void AddMoneyForUnoccupiedTiles(){
        if(BuildSystem.Instance == null) return;

        List<Vector2Int> unoccupiedTiles = BuildSystem.Instance.GetAllUnoccupiedTilePositions();
        int moneyToAdd = unoccupiedTiles.Count * moneyPerUnoccupiedTile;
        
        AddMoney(moneyToAdd);
    }

    public bool CanAfford(int value) => currentMoney >= value;
}