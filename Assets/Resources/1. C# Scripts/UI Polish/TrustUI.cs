using UnityEngine;
using Nova;

public class TrustUI : MonoBehaviour
{
    [Header("VISUALS")]
    [SerializeField] private UIBlock2D fillContent;
    [SerializeField] private TextBlock fillNumber;
    [SerializeField] private float currentTrust;
    [Space(5)]
    [SerializeField] private UIBlock2D trustIcon;
    [SerializeField] private Sprite[] trustSprite = new Sprite[3];
    [Space(10)]
    [SerializeField] private Color32 maxTrustColor;
    [SerializeField] private Color32 minTrustColor;
    [SerializeField] private float colorChangeSpeed = 5f;

    private Color32 targetColor;

    void Update(){
        if(fillContent == null || fillNumber == null) return;
        
        currentTrust = TrustManager.Instance.Trust;
        float trustPercentage = Mathf.Clamp01(currentTrust / 100f);
        
        fillContent.Size.X.Percent = trustPercentage;
        fillNumber.Text = $"Trust Level: {Mathf.RoundToInt(currentTrust).ToString()} / 100";
        
        Sprite currentMood = trustSprite[1];
        if(currentTrust >= 75) currentMood = trustSprite[2];
        else if(currentTrust <= 25) currentMood = trustSprite[0]; 
        trustIcon.SetImage(currentMood);

        targetColor = Color32.Lerp(minTrustColor, maxTrustColor, trustPercentage);
        fillContent.Color = Color32.Lerp(fillContent.Color, targetColor, Time.deltaTime * colorChangeSpeed);
    }
}