using UnityEngine;
using Nova;

public class TrustManager : MonoBehaviour
{
    public static TrustManager Instance {get; private set;}
    [SerializeField] private float currentTrust;
    [SerializeField] private TextBlock trustText;
    [SerializeField] private float maxTrust = 100f;

    public float Trust => currentTrust;

    void Awake(){
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }

        currentTrust = maxTrust/2;
    }

    void Update(){
        if(currentTrust <= 0 && !GameManager.Instance.isEnded) GameManager.Instance.StartCoroutine(GameManager.Instance.EndGame());
    }

    public void AddTrust(float value){
        currentTrust += value;
        currentTrust = Mathf.Clamp(currentTrust, 0f, maxTrust);
    }
}