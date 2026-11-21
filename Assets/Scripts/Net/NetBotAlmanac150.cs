using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class NetBot
{
    IEnumerator Almanac150()
    {
        yield return WaitFor(() => Scene == "MainMenu", 15, "menú principal de v1.5");
        if (failed) yield break;
        ClickButton("JUGAR");
        yield return WaitFor(() => FindFirstObjectByType<MapSelectUI>() != null, 5, "selector amb quatre expedicions");
        if (failed) yield break;
        var selector = FindFirstObjectByType<MapSelectUI>();
        var cards = selector.GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("Mapa ")).ToArray();
        var creator = cards.FirstOrDefault(b => b.name == "Mapa Creador");
        if (cards.Length != 4 || creator == null || !creator.GetComponentsInChildren<TMP_Text>().Any(t => t.text == "PRÒXIMAMENT"))
        { Fail("falta la targeta apagada de Creador"); yield break; }
        yield return Shot("fase5-selector");
        creator.onClick.Invoke();
        yield return null;
        if (Scene != "MainMenu" || GameObject.Find("Avís Creador") == null)
        { Fail("la targeta Creador ha de mostrar un avís i quedar-se al menú"); yield break; }
        Ok("Creador diu Pròximament sense obrir cap escena");
        selector.Hide();

        var blocks = BlockCatalog.Instance?.blocks;
        if (blocks == null || blocks.Length != 626 || blocks.Select(b => b.id).Distinct().Count() != 626 ||
            blocks.Select(b => b.family).Distinct().Count() != 15 ||
            blocks.Any(b => b.sprite == null || !b.passable && b.hitbox.width <= 0f))
        { Fail("falten blocs, famílies, identificadors o hitboxes"); yield break; }
        if (!blocks.Where(b => b.family == "Objectes" && !b.passable).Select(b => b.hitbox).Distinct().Skip(1).Any())
        { Fail("els objectes comparteixen una sola hitbox"); yield break; }
        Ok("626 fitxes, 15 famílies i hitboxes individuals");

        int previous = blocks.Count(b => Almanac.HasBlock(b.id));
        BlockData sample = blocks.First(b => !Almanac.HasBlock(b.id));
        AlmanacUI.Open();
        var ui = FindFirstObjectByType<AlmanacUI>();
        string[] expectedTabs = { "ARMES", "ENEMICS", "BOSSES", "AMULETS", "BLOCS" };
        var tabs = ui.GetComponentsInChildren<Button>()
            .Select(b => b.GetComponentInChildren<TMP_Text>()?.text)
            .Where(expectedTabs.Contains);
        if (!tabs.SequenceEqual(expectedTabs))
        { Fail("ordre incorrecte de les pestanyes de l'Almanac"); yield break; }
        ClickAlmanacButton(ui, "BLOCS");
        yield return null;
        if (!ui.GetComponentsInChildren<TMP_Text>().Any(t => t.text == "???"))
        { Fail("els blocs que no has vist han de mostrar una silueta"); yield break; }
        if (ui.GetComponentsInChildren<TMP_Text>().Any(t => t.text == "Qualitat"))
        { Fail("l'Almanac de blocs encara mostra un filtre de qualitat"); yield break; }
        Ok("Amulets va davant de Blocs i els blocs no es filtren per qualitat");
        yield return Shot("fase5-almanac-blocs");
        ClickAlmanacButton(ui, "Família");
        yield return null;
        ClickAlmanacButton(ui, sample.family);
        yield return null;
        var search = ui.GetComponentInChildren<TMP_InputField>();
        search.text = sample.displayName;
        yield return null;
        if (!ui.GetComponentsInChildren<Button>().Any(b => b.name == "Carta " + sample.id))
        { Fail("el filtre de família i cerca pel nom no troba el bloc"); yield break; }
        Ok("família i cerca per nom combinades");
        ClickAlmanacButton(ui, "AMULETS");
        yield return null;
        if (!ui.GetComponentsInChildren<TMP_Text>().Any(t => t.text.EndsWith(" / 16")))
        { Fail("falta el catàleg d'amulets"); yield break; }
        if (!ui.GetComponentsInChildren<TMP_Text>().Any(t => t.text == "Qualitat"))
        { Fail("falta el filtre de qualitat dels amulets"); yield break; }
        yield return Shot("fase5-almanac-amulets");
        ClickAlmanacButton(ui, "TANCAR");

        RunState.Reset(); RunState.Active = true; RunState.Seed = 155; RunState.Floor = 2;
        SceneManager.LoadScene("Dungeon");
        yield return WaitFor(() => DungeonGenerator.Instance != null, 12, "masmorra per descobrir blocs");
        if (failed) yield break;
        var dungeon = DungeonGenerator.Instance;
        foreach (var room in dungeon.Rooms)
        {
            MovePhase0Player(dungeon.ground.GetCellCenterWorld((Vector3Int)room.Center));
            yield return null;
            yield return null;
            if (blocks.Count(b => Almanac.HasBlock(b.id)) > previous) break;
        }
        if (blocks.Count(b => Almanac.HasBlock(b.id)) <= previous)
        { Fail("veure blocs a la masmorra no els desbloqueja"); yield break; }
        Ok("la proximitat a blocs reals de la masmorra els desbloqueja");
        var discovered = blocks.First(b => Almanac.HasBlock(b.id));
        RunState.Reset();
        SceneManager.LoadScene("MainMenu");
        yield return WaitFor(() => Scene == "MainMenu", 10, "tornada a l'Almanac");
        if (failed) yield break;
        AlmanacUI.Open();
        ui = FindFirstObjectByType<AlmanacUI>();
        ClickAlmanacButton(ui, "BLOCS");
        ui.GetComponentInChildren<TMP_InputField>().text = discovered.displayName;
        yield return null;
        var tile = ui.GetComponentsInChildren<Button>().First(b => b.name == "Carta " + discovered.id);
        tile.onClick.Invoke();
        yield return Shot("fase5-bloc-descobert");
        Pass();
    }

    static void ClickAlmanacButton(AlmanacUI ui, string label)
    {
        var button = ui.GetComponentsInChildren<Button>().First(b => b.GetComponentsInChildren<TMP_Text>().Any(t => t.text == label));
        button.onClick.Invoke();
    }
}
