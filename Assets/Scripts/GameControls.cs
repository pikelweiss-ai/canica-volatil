using UnityEngine;

// Input antic, dues tecles per accio i persistencia local.
public static class GameControls
{
    public static readonly string[] Actions = { "Amunt", "Avall", "Esquerra", "Dreta", "Atacar", "Cofre", "Deixar arma", "Apuntat", "Pausa", "Mapa", "Usar cura", "Arma 1", "Arma 2", "Arma 3", "Arma 4", "Cura 1", "Cura 2", "Cura 3", "Cura 4" };
    static readonly KeyCode[] Primary = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Mouse0, KeyCode.E, KeyCode.Z, KeyCode.Tab, KeyCode.Escape, KeyCode.M, KeyCode.H, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8 };
    static readonly KeyCode[] Secondary = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.Space, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8 };
    public static bool Editing;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Init() { Editing=false; }
    static string Pref(int a, int slot) => "Controls_" + a + "_" + slot;
    public static KeyCode Key(int a, int slot = 0) => (KeyCode)PlayerPrefs.GetInt(Pref(a, slot), (int)(slot == 0 ? Primary[a] : Secondary[a]));
    public static bool Held(int a) => !Editing && ((Key(a) != KeyCode.None && Input.GetKey(Key(a))) || (Key(a, 1) != KeyCode.None && Input.GetKey(Key(a, 1))));
    public static bool Down(int a) => !Editing && ((Key(a) != KeyCode.None && Input.GetKeyDown(Key(a))) || (Key(a, 1) != KeyCode.None && Input.GetKeyDown(Key(a, 1))));
    public static Vector2 Move => new Vector2((Held(3)?1:0)-(Held(2)?1:0), (Held(0)?1:0)-(Held(1)?1:0)).normalized;
    public static int Wheel { get => Mathf.Clamp(PlayerPrefs.GetInt("Controls_Wheel", 2), 0, 3); set { PlayerPrefs.SetInt("Controls_Wheel", value); PlayerPrefs.Save(); } }
    public static bool WheelHeals => Wheel == 1 || (Wheel == 2 && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)));
    public static string Label(int a)
    {
        KeyCode key=Key(a)==KeyCode.None?Key(a,1):Key(a);
        return key.ToString().Replace("None","—").Replace("Alpha", "").Replace("Keypad", "KP").Replace("Mouse", "Rat").Replace("Left", "L").Replace("Right", "R").Replace("Control", "Ctrl").Replace("Arrow", "").Replace("Space", "Esp");
    }
    // Intercanviar les assignacions evita duplicats accidentals entre accions.
    public static void Bind(int a, int slot, KeyCode key)
    {
        KeyCode old = Key(a, slot);
        if (key != KeyCode.None)
            for (int i=0; i<Actions.Length; i++) for(int j=0;j<2;j++)
                if ((i != a || j != slot) && Key(i,j) == key) PlayerPrefs.SetInt(Pref(i,j), (int)old);
        PlayerPrefs.SetInt(Pref(a,slot), (int)key); PlayerPrefs.Save();
    }
    public static void Reset()
    {
        for(int i=0;i<Actions.Length;i++) for(int j=0;j<2;j++) PlayerPrefs.DeleteKey(Pref(i,j));
        PlayerPrefs.DeleteKey("Controls_Wheel"); PlayerPrefs.Save();
    }
}
public class ControlKeyLabel : MonoBehaviour
{
    public int action;
    void Update(){GetComponent<TMPro.TextMeshProUGUI>().text=GameControls.Label(action);}
}