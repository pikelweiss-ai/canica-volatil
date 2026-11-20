using UnityEngine;

// El que es conserva entre pisos de la masmorra (les escenes es tornen a carregar).
public static class RunState
{
    public static bool Active;              // Estem en una partida de masmorra
    public static int Floor = 1;            // Pis actual (1, 2, 3...)
    public static int Health;               // Cors en acabar el pis anterior
    public static int Coins;                // Monedes de la partida
    public static float[] WeaponCooldowns;
    public static WeaponData[] Weapons;     // Hotbar en acabar el pis anterior
    public static HealItem[] HealItems;     // Bossa de cures
    public static int[] HealCounts;
    public static int Seed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init() => Reset();

    public static void Reset()
    {
        Active = false;
        Floor = 1;
        Health = 0;
        Coins = 0;
        Weapons = null;
        WeaponCooldowns = null;
        HealItems = null;
        HealCounts = null;
        Seed = 0;
    }
}
