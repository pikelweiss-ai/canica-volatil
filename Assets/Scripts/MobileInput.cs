using UnityEngine;

// Pont entre els dos dits independents i les accions comunes del joc.
public static class MobileInput
{
    public static bool Active => Application.isMobilePlatform || System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "--mobile-preview") >= 0;
    public static Vector2 Movement, Aim = Vector2.right;
    public static bool Firing;
    static readonly bool[] held = new bool[GameControls.Actions.Length];
    static readonly bool[] pressed = new bool[GameControls.Actions.Length];
    public static bool Held(int action) => Active && (action == 4 ? Firing : held[action]);
    // La cua evita perdre el toc si EventSystem s'executa despres del jugador.
    public static bool Down(int action) { if(!Active || !pressed[action]) return false; pressed[action] = false; return true; }
    public static void Press(int action) { held[action] = true; pressed[action] = true; if (action == 4) Firing = true; }
    public static void Release(int action) { held[action] = false; if (action == 4) Firing = false; }
    public static void Clear() { Movement = Vector2.zero; Firing = false; System.Array.Clear(held, 0, held.Length); System.Array.Clear(pressed, 0, pressed.Length); }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init() { Clear(); Aim = Vector2.right; if (Active) { Input.simulateMouseWithTouches = false; Input.multiTouchEnabled = true; Application.targetFrameRate = 60; } }
}
