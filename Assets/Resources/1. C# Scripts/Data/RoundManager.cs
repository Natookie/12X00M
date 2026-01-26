using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private int roundCount = 1;

    public static RoundManager Instance;

    public void CompleteRound() => roundCount++;
    public int CheckCurrentRound() => roundCount;

    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
    }

}