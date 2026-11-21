using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class NetBot
{
    IEnumerator Store150()
    {
        yield return WaitFor(() => Scene == "MainMenu", 12, "menú de personalització");
        if (failed) yield break;
        var seen = new Dictionary<string, int>();
        const int missing = int.MinValue;
        void Save(string key) { if (!seen.ContainsKey(key)) seen[key] = PlayerPrefs.GetInt(key, missing); }
        var ownerKey = typeof(CosmeticStore).GetMethod("OwnershipKey", BindingFlags.NonPublic | BindingFlags.Static);
        string OwnerKey(CosmeticKind kind, int index) => (string)ownerKey.Invoke(null, new object[] { kind, index });
        Save("MonedesTotals");
        Save("Cosmetics_Store_v1");
        try
        {
            PlayerPrefs.SetInt("Cosmetics_Store_v1", 0);
            foreach (var kind in new[] { CosmeticKind.Skins, CosmeticKind.Hats, CosmeticKind.Trails, CosmeticKind.Colors, CosmeticKind.Expressions })
            {
                Save(CosmeticStore.Key(kind));
                int index = Mathf.Clamp(2, 0, CosmeticStore.Count(kind) - 1);
                Save(OwnerKey(kind, index));
                PlayerPrefs.DeleteKey(OwnerKey(kind, index));
                PlayerPrefs.SetInt(CosmeticStore.Key(kind), index);
            }
            foreach (var kind in new[] { CosmeticKind.Skins, CosmeticKind.Hats, CosmeticKind.Trails })
                if (!CosmeticStore.Owned(kind, 2) || CosmeticStore.Equipped(kind) != 2)
                { Fail("la tenda ha perdut la tria antiga de " + kind); yield break; }
            Ok("conserva les claus i tries antigues de canica, barret i estela");

            foreach (var kind in new[] { CosmeticKind.Skins, CosmeticKind.Hats, CosmeticKind.Trails, CosmeticKind.Colors, CosmeticKind.Expressions })
            {
                int index = CosmeticStore.Count(kind) - 1;
                var key = OwnerKey(kind, index);
                Save(key);
                PlayerPrefs.DeleteKey(key);
                int price = CosmeticStore.Price(kind, index);
                PlayerPrefs.SetInt("MonedesTotals", price - 1);
                if (CosmeticStore.Buy(kind, index) || CosmeticStore.Use(kind, index))
                { Fail("es pot comprar o equipar sense prou monedes: " + kind); yield break; }
                PlayerPrefs.SetInt("MonedesTotals", price + 12);
                if (!CosmeticStore.Buy(kind, index) || CoinBank.Total != 12 || !CosmeticStore.Owned(kind, index) ||
                    PlayerPrefs.GetInt(key) != 1 || CosmeticStore.Buy(kind, index) || !CosmeticStore.Use(kind, index) ||
                    CosmeticStore.Equipped(kind) != index)
                { Fail("compra, guardat o equipament incorrecte: " + kind); yield break; }
                Ok("compra i equipament: " + kind + " (" + price + ")");
            }

            var ui = CustomizeMenuUI.Create(null);
            ui.Show();
            var field = typeof(CustomizeMenuUI).GetField("store", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(ui, true);
            typeof(CustomizeMenuUI).GetMethod("Refresh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null);
            yield return Shot("fase4-tenda");
            Destroy(ui.gameObject);

            var avatar = new NetPlayer { id = 2, name = "Bot", color = 3, expression = 4 };
            using (var stream = new MemoryStream())
            {
                using var output = new BinaryWriter(stream);
                avatar.Write(output);
                stream.Position = 0;
                using var input = new BinaryReader(stream);
                var copy = NetPlayer.Read(input);
                if (copy.color != 3 || copy.expression != 4)
                { Fail("el color i l’expressió s’han perdut al protocol"); yield break; }
            }

            RunState.Reset(); RunState.Active = true; RunState.Seed = 154;
            SceneManager.LoadScene("Dungeon");
            yield return WaitFor(() => DungeonGenerator.Instance != null, 12, "sala Botiga del pis");
            if (failed) yield break;
            var dungeon = DungeonGenerator.Instance;
            var room = dungeon.Rooms.SingleOrDefault(r => r.type == DungeonGenerator.RoomType.Shop);
            var merchant = Shop.Find(Shop.MerchantKind);
            var vending = Shop.Find(Shop.VendingKind);
            if (room == null || merchant == null || vending == null ||
                !room.rect.Contains((Vector2Int)dungeon.ground.WorldToCell(merchant.transform.position)) ||
                !room.rect.Contains((Vector2Int)dungeon.ground.WorldToCell(vending.transform.position)))
            { Fail("la sala Botiga no conté mercader i màquina"); yield break; }
            var slot = merchant.slots.FirstOrDefault(article => article.amulet != Amulet.None);
            if (slot == null || vending.slots.Any(article => article.heal == null))
            { Fail("estoc incorrecte a la Botiga"); yield break; }
            MovePhase0Player(dungeon.ground.GetCellCenterWorld((Vector3Int)room.Center));
            CoinCounter.instance.currentCoins = RunState.Coins = 500;
            merchant.Buy(System.Array.IndexOf(merchant.slots, slot));
            if (!RunAmulets.Local.Has(slot.amulet) || RunState.Coins != 500 - slot.price)
            { Fail("comprar l'amulet no el posa a la ranura o no cobra"); yield break; }
            Ok("la sala Botiga té estoc complet i ven amulets");
            yield return Shot("fase4-sala-botiga");
        }
        finally
        {
            foreach (var entry in seen)
                if (entry.Value == missing) PlayerPrefs.DeleteKey(entry.Key);
                else PlayerPrefs.SetInt(entry.Key, entry.Value);
            PlayerPrefs.Save();
        }
        Pass();
    }
}

