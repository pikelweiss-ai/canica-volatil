using System;
using UnityEngine;

public enum CosmeticKind { Skins, Hats, Trails }

public static class CosmeticStore
{
    const string Migrated = "Cosmetics_Store_v1";
    public static string Key(CosmeticKind kind) => kind switch
    {
        CosmeticKind.Skins => CosmeticsDatabase.KeySkin,
        CosmeticKind.Hats => CosmeticsDatabase.KeyHat,
        CosmeticKind.Trails => CosmeticsDatabase.KeyTrail,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static int Count(CosmeticKind kind)
    {
        var db = CosmeticsDatabase.Instance;
        return kind switch
        {
            CosmeticKind.Skins => db?.skins?.Length ?? 0,
            CosmeticKind.Hats => db?.hats?.Length ?? 0,
            CosmeticKind.Trails => db?.trails?.Length ?? 0,
            _ => 0
        };
    }

    public static int Equipped(CosmeticKind kind) => CosmeticsDatabase.Get(Key(kind), Count(kind));

    public static string Name(CosmeticKind kind, int index)
    {
        var db = CosmeticsDatabase.Instance;
        return kind switch
        {
            CosmeticKind.Skins => index == 0 ? "Original" : db.skins[index].name,
            CosmeticKind.Hats => string.IsNullOrEmpty(db.hats[index].name) ? "Cap" : db.hats[index].name,
            CosmeticKind.Trails => db.trails[index].name,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    static bool Valid(CosmeticKind kind, int index) => index >= 0 && index < Count(kind);

    static string OwnershipKey(CosmeticKind kind, int index)
    {
        var db = CosmeticsDatabase.Instance;
        string id = kind switch
        {
            CosmeticKind.Skins => db.skins[index].sprite != null ? db.skins[index].sprite.name : db.skins[index].name,
            CosmeticKind.Hats => db.hats[index].sprite != null ? db.hats[index].sprite.name : db.hats[index].name,
            _ => Name(kind, index)
        };
        return "Cos_Owned_" + kind + "_" + id;
    }

    static void ImportLegacyEquipment()
    {
        if (PlayerPrefs.GetInt(Migrated) != 0 || CosmeticsDatabase.Instance == null) return;
        foreach (var kind in new[] { CosmeticKind.Skins, CosmeticKind.Hats, CosmeticKind.Trails })
        {
            int index = Equipped(kind);
            if (Valid(kind, index)) PlayerPrefs.SetInt(OwnershipKey(kind, index), 1);
        }
        PlayerPrefs.SetInt(Migrated, 1);
        PlayerPrefs.Save();
    }

    public static bool Owned(CosmeticKind kind, int index)
    {
        if (!Valid(kind, index)) return false;
        ImportLegacyEquipment();
        return (Price(kind, index) == 0 && !IsExclusive(kind, index)) || PlayerPrefs.GetInt(OwnershipKey(kind, index)) != 0;
    }

    public static bool IsExclusive(CosmeticKind kind, int index) =>
        kind == CosmeticKind.Skins && Valid(kind, index) && Name(kind, index) == "Capvespre";

    public static ChestRarity Quality(CosmeticKind kind, int index)
    {
        if (index == 0) return ChestRarity.Comu;
        string name = Name(kind, index);
        if (kind == CosmeticKind.Skins)
        {
            switch (name)
            {
                case "Bombolla": case "Glaçó": case "Ninja":
                    return ChestRarity.PocComu;
                case "Coure": case "Caramel": case "Llima":
                    return ChestRarity.Rar;
                case "Robot": case "Aurora": case "Cosmos": case "Canica d'or": case "Canica de vidre":
                    return ChestRarity.Epic;
                case "Capvespre":
                    return ChestRarity.Llegendari;
            }
        }
        if (index < 2) return ChestRarity.Comu;
        if (name.Contains("Arc de Sant Martí", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Univers", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Diamant", StringComparison.OrdinalIgnoreCase)) return ChestRarity.Llegendari;
        float fraction = (float)index / Mathf.Max(2, Count(kind) - 1);
        return fraction >= .91f ? ChestRarity.Llegendari : fraction >= .62f ? ChestRarity.Epic : ChestRarity.Rar;
    }

    public static string QualityName(ChestRarity quality) => quality switch
    {
        ChestRarity.Llegendari => "Llegendari",
        ChestRarity.Epic => "Èpic",
        ChestRarity.Rar => "Rar",
        ChestRarity.PocComu => "Poc comú",
        _ => "Comú"
    };

    public static int Price(CosmeticKind kind, int index) => IsExclusive(kind, index) ? 0 : Quality(kind, index) switch
    {
        ChestRarity.Llegendari => 260,
        ChestRarity.Epic => 120,
        ChestRarity.Rar => 55,
        ChestRarity.PocComu => 25,
        _ => 0
    };

    public static bool Buy(CosmeticKind kind, int index)
    {
        if (!Valid(kind, index) || IsExclusive(kind, index) || Owned(kind, index) || !CoinBank.TrySpend(Price(kind, index))) return false;
        PlayerPrefs.SetInt(OwnershipKey(kind, index), 1);
        PlayerPrefs.Save();
        return true;
    }

    public static bool Use(CosmeticKind kind, int index)
    {
        if (!Owned(kind, index)) return false;
        PlayerPrefs.SetInt(Key(kind), index);
        PlayerPrefs.Save();
        return true;
    }

    public static int RandomOwned(CosmeticKind kind)
    {
        var options = new System.Collections.Generic.List<int>();
        for (int i = 0; i < Count(kind); i++) if (Owned(kind, i)) options.Add(i);
        return options.Count == 0 ? 0 : options[UnityEngine.Random.Range(0, options.Count)];
    }

}

