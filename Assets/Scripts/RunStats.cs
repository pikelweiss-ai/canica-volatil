using System.Collections.Generic;
using UnityEngine;

// Estadistiques de la partida de masmorra en curs (per al resum final) i records guardats.
// Els records van separats per a jugar sol i en cooperatiu. El Mode Creatiu no compta
// perque nomes es compta dins de la masmorra (DungeonGenerator).
// No hi ha cap record de temps: es podria inflar quedant-se parat.
public static class RunStats
{
    public static float Seconds;
    public static int Kills, BossKills, CoinsEarned, RoomsCleared, ChestsOpened;
    static int maxFloor = 1;
    public static int MaxFloor => Mathf.Max(maxFloor, RunState.Floor);
    static readonly Dictionary<string, int> weaponKills = new();
    static bool committed;

    // Arma del colp que s'aplica ara mateix: null = cap context, "" = un company (online).
    // L'assignen PlayerAttack, Projectile i NetCoop just abans de fer el dany.
    public static string HitBy;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init() => Reset();

    public static void Reset()
    {
        Seconds = 0f;
        Kills = BossKills = CoinsEarned = RoomsCleared = ChestsOpened = 0;
        maxFloor = 1;
        weaponKills.Clear();
        committed = false;
        HitBy = null;
    }

    public static bool Counting => RunState.Active && DungeonGenerator.Instance != null;

    public static void Tick(float dt) { if (Counting) Seconds += dt; }
    public static void Floor(int floor) { if (floor > maxFloor) maxFloor = floor; }
    public static void Coin() { if (Counting) CoinsEarned++; }
    public static void RoomCleared() { if (Counting) RoomsCleared++; }
    public static void ChestOpened() { if (Counting) ChestsOpened++; }

    // hitByPlayer: algun jugador l'ha colpejat. Un kamikaze que esclata sol no compta.
    public static void EnemyKilled(EnemyHealth enemy, string lastHitBy, bool hitByPlayer)
    {
        if (!Counting || enemy == null) return;
        if (!hitByPlayer && enemy.GetComponent<EnemyKamikaze>() != null) return;
        Kills++;
        if (enemy.GetComponent<EnemyBoss>() != null) BossKills++;
        if (!string.IsNullOrEmpty(lastHitBy))
            weaponKills[lastHitBy] = weaponKills.TryGetValue(lastHitBy, out int n) ? n + 1 : 1;
    }

    // Arma amb que aquest jugador ha rematat mes enemics (null si cap)
    public static string FavoriteWeapon(out int kills)
    {
        string best = null; kills = 0;
        foreach (var kv in weaponKills) if (kv.Value > kills) { best = kv.Key; kills = kv.Value; }
        return best;
    }

    public static string TimeText()
    {
        int s = Mathf.FloorToInt(Seconds);
        return s >= 3600 ? $"{s / 3600}:{s / 60 % 60:00}:{s % 60:00}" : $"{s / 60}:{s % 60:00}";
    }

    // ---------------- Records ----------------
    public struct Record
    {
        public string label;
        public int value;   // el d'aquesta partida
        public int best;    // el millor guardat (despres d'actualitzar)
        public bool isNew;  // aquesta partida l'ha superat
    }

    static readonly (string key, string label)[] RecordDefs =
    {
        ("Floor", "Pis màxim"), ("Kills", "Enemics"), ("Bosses", "Bosses"), ("Coins", "Monedes"),
    };

    static int Value(string key) => key switch
    {
        "Floor" => MaxFloor, "Kills" => Kills, "Bosses" => BossKills, _ => CoinsEarned,
    };

    static string Key(bool coop, string key) => "Record_" + (coop ? "Coop_" : "Solo_") + key;

    public static int Best(bool coop, string key) => PlayerPrefs.GetInt(Key(coop, key), 0);

    // Les proves automatiques (NetBot) comparen pero no guarden: no han de tocar els records de Ferran
    public static bool DontSave;

    // Compara la partida amb els records i guarda els que supera. Nomes una vegada per partida.
    public static List<Record> CommitRecords(bool coop)
    {
        var list = new List<Record>();
        foreach (var (key, label) in RecordDefs)
        {
            int value = Value(key), best = Best(coop, key);
            bool isNew = !committed && value > best;
            if (isNew) { if (!DontSave) PlayerPrefs.SetInt(Key(coop, key), value); best = value; }
            list.Add(new Record { label = label, value = value, best = best, isNew = isNew });
        }
        if (!committed && !DontSave) PlayerPrefs.Save();
        committed = true;
        return list;
    }
}
