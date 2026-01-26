using UnityEngine;
using System.Collections.Generic;
using Nova;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance {get; private set;}
    [SerializeField] private int currentMoney;
    [SerializeField] private int starterBudget = 1000;
    [SerializeField] private int moneyPerUnoccupiedTile = 5;
    [Space(10)]
    [SerializeField] private TextBlock moneyText;

    public int Money => currentMoney;

    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }

        currentMoney = starterBudget;
    }

    void Update(){
        moneyText.Text = $"${currentMoney}";
    }

    public void AddMoney(int value){
        currentMoney += value;
        currentMoney = Mathf.Max(0, currentMoney);
    }

    public void AddMoneyForUnoccupiedTiles(){
        if(BuildSystem.Instance == null) return;

        List<Vector2Int> unoccupiedTiles = BuildSystem.Instance.GetAllUnoccupiedTilePositions();
        int moneyToAdd = unoccupiedTiles.Count * moneyPerUnoccupiedTile;
        
        AddMoney(moneyToAdd);
        Debug.Log($"Added ${moneyToAdd} for {unoccupiedTiles.Count} unoccupied tiles");
    }

    public bool CanAfford(int value) => currentMoney >= value;
}