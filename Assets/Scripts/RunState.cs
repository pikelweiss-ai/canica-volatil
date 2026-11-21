using UnityEngine;

// El que es conserva entre pisos de la masmorra (les escenes es tornen a carregar).
public static class RunState
{
    public static bool Active;              // Estem en una partida de masmorra
    public static int Floor = 1;            // Pis actual (1, 2, 3...)
    public static int Health;               // Cors en acabar el pis anterior
    public static int Coins;                // Monedes de la partida
    public static float[] WeaponCooldowns;
    public static WeaponCondition[] WeaponConditions;
    public static WeaponData[] Weapons;     // Hotbar en acabar el pis anterior
    public static HealItem[] HealItems;     // Bossa de cures
    public static int[] HealCounts;
    public static int Seed;
    public static int FloorVisit;
    public static int ArrivalDirection;
    internal static readonly System.Collections.Generic.Dictionary<int, DungeonGenerator.FloorState> Floors = new();

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
        WeaponConditions = null;
        HealItems = null;
        HealCounts = null;
        Seed = 0;
        FloorVisit = 0;
        ArrivalDirection = 0;
        Floors.Clear();
        RunAmulets.Reset();
        RunStats.Reset();
    }
}
