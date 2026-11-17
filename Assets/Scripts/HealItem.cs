using UnityEngine;

// Item de cura (poma, bena, poció, farmaciola).
// Es crea amb: clic dret a Project -> Create -> Canica -> Item de cura
// Els que estan a Assets/Resources/Items els troben sols els cofres i els enemics.
[CreateAssetMenu(fileName = "NouItemCura", menuName = "Canica/Item de cura")]
public class HealItem : ScriptableObject
{
    public string itemName = "Poma";
    public Sprite icon;
    public int healAmount = 1;               // Cors que torna
    public ChestRarity rarity = ChestRarity.Comu;

    static HealItem[] all;

    public static HealItem[] All
    {
        get
        {
            if (all == null || all.Length == 0) all = Resources.LoadAll<HealItem>("Items");
            return all;
        }
    }

    // Item a l'atzar fins a una raresa (els comuns surten més)
    public static HealItem Random(ChestRarity maxRarity = ChestRarity.Llegendari)
    {
        HealItem[] items = All;
        if (items == null || items.Length == 0) return null;

        float total = 0f;
        foreach (HealItem i in items) if (i.rarity <= maxRarity) total += Weight(i.rarity);
        float r = UnityEngine.Random.value * total;
        foreach (HealItem i in items)
        {
            if (i.rarity > maxRarity) continue;
            r -= Weight(i.rarity);
            if (r <= 0f) return i;
        }
        return items[0];
    }

    public static HealItem RandomCommon() => Random(ChestRarity.Rar);

    static float Weight(ChestRarity r) => r == ChestRarity.Comu ? 6f : r == ChestRarity.Rar ? 3f : r == ChestRarity.Epic ? 1.5f : 0.6f;
}
