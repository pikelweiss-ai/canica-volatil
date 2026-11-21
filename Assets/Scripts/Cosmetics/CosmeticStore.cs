using System;
using UnityEngine;

public enum CosmeticKind { Skins, Hats, Trails, Colors, Expressions }

public static class CosmeticStore
{
    public const string KeyColor = "Cos_Color", KeyExpression = "Cos_Expression";
    const string Migrated = "Cosmetics_Store_v1";
    static readonly (string name, Color color)[] colors =
    {
        ("Natural", Color.white), ("Menta", new Color(.63f, 1f, .78f)), ("Mel", new Color(1f, .83f, .52f)),
        ("Ametista", new Color(.85f, .65f, 1f)), ("Coral", new Color(1f, .63f, .65f)), ("Aurora", new Color(.55f, .92f, 1f))
    };
    static readonly string[] expressions = { "Original", "Somriure", "Clucada", "Sorpresa", "Fúria", "Cors" };
    static readonly Sprite[] faces = new Sprite[expressions.Length];

    public static string Key(CosmeticKind kind) => kind switch
    {
        CosmeticKind.Skins => CosmeticsDatabase.KeySkin,
        CosmeticKind.Hats => CosmeticsDatabase.KeyHat,
        CosmeticKind.Trails => CosmeticsDatabase.KeyTrail,
        CosmeticKind.Colors => KeyColor,
        _ => KeyExpression
    };

    public static int Count(CosmeticKind kind)
    {
        var db = CosmeticsDatabase.Instance;
        return kind switch
        {
            CosmeticKind.Skins => db?.skins?.Length ?? 0,
            CosmeticKind.Hats => db?.hats?.Length ?? 0,
            CosmeticKind.Trails => db?.trails?.Length ?? 0,
            CosmeticKind.Colors => colors.Length,
            _ => expressions.Length
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
            CosmeticKind.Colors => colors[index].name,
            _ => expressions[index]
        };
    }

    public static Color Tint(int index) => colors[Mathf.Clamp(index, 0, colors.Length - 1)].color;

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
        return Price(kind, index) == 0 || PlayerPrefs.GetInt(OwnershipKey(kind, index)) != 0;
    }

    public static ChestRarity Quality(CosmeticKind kind, int index)
    {
        if (index < 2) return ChestRarity.Comu;
        string name = Name(kind, index);
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
        _ => "Comú"
    };

    public static int Price(CosmeticKind kind, int index) => Quality(kind, index) switch
    {
        ChestRarity.Llegendari => 260,
        ChestRarity.Epic => 120,
        ChestRarity.Rar => 55,
        _ => 0
    };

    public static bool Buy(CosmeticKind kind, int index)
    {
        if (!Valid(kind, index) || Owned(kind, index) || !CoinBank.TrySpend(Price(kind, index))) return false;
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

    public static Sprite Expression(int index)
    {
        if (index <= 0 || index >= faces.Length) return null;
        if (faces[index] != null) return faces[index];
        string[][] pixels =
        {
            null,
            new[] { ".#...#.", ".#...#.", ".......", "#.....#", ".#####." },
            new[] { ".##..#.", "#....#.", ".......", ".....#.", "..###.." },
            new[] { ".#...#.", ".#...#.", ".......", "...#...", "..#.#..", "...#..." },
            new[] { "#.....#", ".#...#.", ".......", "..###..", ".#...#." },
            new[] { "hh.hh.h", "hhh.hhh", ".h...h.", ".......", "..###.." }
        };
        var pattern = pixels[index];
        var tex = new Texture2D(16, 16) { filterMode = FilterMode.Point };
        tex.SetPixels(new Color[16 * 16]);
        for (int y = 0; y < pattern.Length; y++)
            for (int x = 0; x < pattern[y].Length; x++)
            {
                char pixel = pattern[y][x];
                if (pixel != '.') tex.SetPixel(x + 4, 10 - y, pixel == 'h' ? new Color(1f, .18f, .37f) : new Color(.09f, .07f, .13f));
            }
        tex.Apply();
        return faces[index] = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.one * .5f, 16);
    }
}

