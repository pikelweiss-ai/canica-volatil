using UnityEngine;
using TMPro;

public class AlphaVisualStyle : ScriptableObject
{
    public TMP_FontAsset font;
    public TMP_FontAsset coinFont;
    public Sprite grass,brick,wood,tree,player;
    static AlphaVisualStyle instance;
    public static AlphaVisualStyle Current => instance != null ? instance : instance = Resources.Load<AlphaVisualStyle>("AlphaVisualStyle");
}
