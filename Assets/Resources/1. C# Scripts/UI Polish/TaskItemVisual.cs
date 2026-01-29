using UnityEngine;
using Nova;

public class TaskItemVisual : ItemVisuals
{
    [Header("DATA")]
    public UIBlock2D statusBlock;
    public TextBlock progressText;
    public TextBlock descriptionText;   
    
    [Header("COLORS")]
    public Color completedColor = new Color32(180, 230, 86, 255);
    public Color pendingColor = new Color32(226, 226, 226, 255);
    public Color failedColor = new Color32(245, 70, 76, 255);
}