using UnityEngine;
using Nova;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance {get; private set;}
    [SerializeField] private int currentMoney;
    [SerializeField] private int starterBudget = 1000;
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

    public bool CanAfford(int value) => currentMoney >= value;
}