using UnityEngine;

// Opcions de joc (Ajustos > OPCIONS). Es guarden en PlayerPrefs i funcionen sense xarxa.
public static class GameOptions
{
    static bool GetBool(string key, bool fallback) { try { return PlayerPrefs.GetInt(key, fallback ? 1 : 0) != 0; } catch { return fallback; } }
    static void SetBool(string key, bool value) { try { PlayerPrefs.SetInt(key, value ? 1 : 0); PlayerPrefs.Save(); } catch { } }

    // Clic dret = cura intel·ligent (v1.3). Activada per defecte.
    public static bool SmartHeal { get => HealInventory.SmartHeal; set => HealInventory.SmartHeal = value; }
    // Números de dany que surten dels enemics
    public static bool DamageNumbers { get => GetBool("Opt_DamageNumbers", true); set => SetBool("Opt_DamageNumbers", value); }
    // L'aim del Disc de cometa ensenya els rebots
    public static bool BounceAim { get => GetBool("Opt_BounceAim", true); set => SetBool("Opt_BounceAim", value); }
    // Tremolor de càmera, de 0 (cap) a 1 (normal)
    public static float Shake
    {
        get { try { return Mathf.Clamp01(PlayerPrefs.GetFloat("Opt_Shake", 1f)); } catch { return 1f; } }
        set { try { PlayerPrefs.SetFloat("Opt_Shake", Mathf.Clamp01(value)); PlayerPrefs.Save(); } catch { } }
    }
}
