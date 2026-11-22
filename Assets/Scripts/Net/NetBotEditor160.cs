using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class NetBot
{
    IEnumerator Editor160()
    {
        yield return WaitFor(() => Scene == "MainMenu", 20, "menú v1.6");
        if (failed) yield break;
        MapStore.Delete("Mapa");
        ClickButton("JUGAR");
        yield return WaitFor(() => FindFirstObjectByType<MapSelectUI>() != null, 5, "selector de mapes");
        var creator = FindObjectsByType<Button>(FindObjectsSortMode.None)
            .FirstOrDefault(b => b.gameObject.name == "Mapa Creador");
        if (creator == null) { Fail("falta la targeta Creador"); yield break; }
        creator.onClick.Invoke();
        yield return WaitFor(() => Scene == "Creador", 5, "escena Creador");
        if (failed) yield break;
        var mapEditor = FindFirstObjectByType<MapEditor>();
        var viewButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        viewButtons.First(b => b.gameObject.name == "Boto GRAELLA: SÍ").onClick.Invoke();
        if (mapEditor == null || mapEditor.GridVisibleForTest) { Fail("no s'ha pogut ocultar la quadrícula"); yield break; }
        viewButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        viewButtons.First(b => b.gameObject.name == "Boto GRAELLA: SÍ").onClick.Invoke();
        viewButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        viewButtons.First(b => b.gameObject.name == "Boto HITBOXES: NO").onClick.Invoke();
        if (!mapEditor.HitboxesVisibleForTest) { Fail("no s'ha pogut activar la visualització de hitboxes"); yield break; }
        viewButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        viewButtons.First(b => b.gameObject.name == "Boto HITBOXES: NO").onClick.Invoke();
        yield return null;
        // Remove only test-run leftovers from this new, isolated bot preference namespace.
        PlayerPrefs.DeleteKey("Canica.MapEditor.Favorites"); PlayerPrefs.DeleteKey("Canica.MapEditor.Recent"); PlayerPrefs.Save();
        bool hadFavorites = PlayerPrefs.HasKey("Canica.MapEditor.Favorites");
        bool hadRecents = PlayerPrefs.HasKey("Canica.MapEditor.Recent");
        string savedFavorites = PlayerPrefs.GetString("Canica.MapEditor.Favorites", "");
        string savedRecents = PlayerPrefs.GetString("Canica.MapEditor.Recent", "");
        System.Action restorePalettePreferences = () => {
            if (hadFavorites) PlayerPrefs.SetString("Canica.MapEditor.Favorites", savedFavorites); else PlayerPrefs.DeleteKey("Canica.MapEditor.Favorites");
            if (hadRecents) PlayerPrefs.SetString("Canica.MapEditor.Recent", savedRecents); else PlayerPrefs.DeleteKey("Canica.MapEditor.Recent");
            PlayerPrefs.Save();
        };
        PlayerPrefs.SetString("Canica.MapEditor.Favorites", ""); PlayerPrefs.SetString("Canica.MapEditor.Recent", ""); PlayerPrefs.Save();
        viewButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        viewButtons.First(b => b.gameObject.name == "Favorit Terres/Terra").onClick.Invoke();
        viewButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        viewButtons.First(b => b.gameObject.name == "Boto VISTA: TOTS").onClick.Invoke();
        yield return null;
        var filteredRows = FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b => b.gameObject.name.StartsWith("Bloc ")).ToArray();
        if (filteredRows.Length != 1 || filteredRows[0].gameObject.name != "Bloc Terres/Terra")
        { restorePalettePreferences(); Fail("la paleta no filtra pels favorits: " + string.Join(",", filteredRows.Select(b => b.gameObject.name))); yield break; }
        filteredRows[0].onClick.Invoke();
        viewButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        viewButtons.First(b => b.gameObject.name == "Boto VISTA: TOTS").onClick.Invoke();
        yield return null;
        if (!FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b => b.gameObject.name == "Bloc Terres/Terra"))
        { restorePalettePreferences(); Fail("la paleta no mostra els blocs recents"); yield break; }
        restorePalettePreferences();
        viewButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        viewButtons.First(b => b.gameObject.name == "Boto VISTA: TOTS").onClick.Invoke();
        yield return null;
        var enemyCatalog = Resources.Load<NetContentCatalog>("NetContent");
        if (enemyCatalog == null || enemyCatalog.enemies == null || !enemyCatalog.enemies.Any(e => e != null && e.GetComponent<EnemyBoss>() != null))
        { Fail("el catàleg d'enemics no inclou bosses seleccionables"); yield break; }
        var choiceButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        choiceButtons.First(b => b.gameObject.name == "Boto ENEMIC +").onClick.Invoke();
        if (!FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t => t.text.StartsWith("Enemic: ") && t.text != "Enemic: Llim"))
        { Fail("no es pot canviar l'enemic seleccionat"); yield break; }
        choiceButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        choiceButtons.First(b => b.gameObject.name == "Boto ENEMIC −").onClick.Invoke();
        choiceButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        choiceButtons.First(b => b.gameObject.name == "Boto COFRE +").onClick.Invoke();
        if (!FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t => t.text == "Cofre: Rar"))
        { Fail("no es pot canviar la raresa del cofre"); yield break; }
        choiceButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        choiceButtons.First(b => b.gameObject.name == "Boto COFRE −").onClick.Invoke();
        var dragTrace = MapEditor.TraceLine(new Vector2Int(1, 1), new Vector2Int(4, 4));
        if (dragTrace.Length != 4 || dragTrace[0] != new Vector2Int(1, 1) || dragTrace[3] != new Vector2Int(4, 4))
        { Fail("la interpolació del traç deixa buits entre cel·les"); yield break; }
        var solidBlock = BlockCatalog.Instance.blocks.FirstOrDefault(b => b != null && !b.passable);
        if (solidBlock == null || MapPlaytest.CellIsPassable(new MapData { width = 1, height = 1,
                layers = new MapLayers { ground = new[] { new MapCell { x = 0, y = 0, blockId = solidBlock.id } } } }, 0, 0))
        { Fail("el jugador pot travessar un bloc sòlid durant PROVAR"); yield break; }
        var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        if (buttons.Count(b => b.gameObject.name.StartsWith("Cel·la ")) != 256 ||
            !buttons.Any(b => b.gameObject.name.StartsWith("Bloc Terres/")) ||
            !buttons.Any(b => b.gameObject.name == "Boto GUARDAR") ||
            !FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None).Any())
        { Fail("falta la graella, la paleta o el guardat. Botons=" + buttons.Length +
            " guardar=" + buttons.Any(b => b.gameObject.name == "Boto GUARDAR") +
            " camps=" + FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None).Length); yield break; }
        buttons.First(b => b.gameObject.name == "Bloc Terres/Terra").onClick.Invoke();
        buttons.First(b => b.gameObject.name == "Boto ROTAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 0,0").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out var saved, out var error) || saved.layers.ground.Length != 1 ||
            saved.layers.ground[0].blockId != "Terres/Terra" || saved.layers.ground[0].rot != 1)
        { Fail("la cel·la pintada no s'ha guardat: " + error); yield break; }
        buttons.First(b => b.gameObject.name == "Boto DESFER").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Length != 0)
        { Fail("desfer no ha retirat la cel·la"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto REFER").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Length != 1)
        { Fail("refer no ha recuperat la cel·la"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto OBRIR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        var savedMapButton = buttons.FirstOrDefault(b => b.gameObject.name == "Mapa guardat Mapa");
        if (savedMapButton == null) { Fail("Obrir no mostra la llista amb nom del mapa"); yield break; }
        savedMapButton.onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto JUGADOR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 0,0").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto FINAL").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 0,0").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto ENEMIC").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 1,0").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto MERCADER").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 2,0").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto COFRE").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 3,0").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        var partialBlock = BlockCatalog.Instance.blocks.FirstOrDefault(b => b != null && !b.passable && !b.fullBlock && b.hitbox.width > 0 && b.hitbox.height > 0);
        if (partialBlock == null) { Fail("no hi ha cap semibloc sòlid per provar la hitbox"); yield break; }
        buttons.First(b => b.gameObject.name == "Bloc " + partialBlock.id).onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto PINTAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto HITBOXES: NO").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 4,0").onClick.Invoke();
        if (!FindObjectsByType<RectTransform>(FindObjectsSortMode.None).Any(r => r.gameObject.name == "Hitbox depuració"))
        { Fail("el semibloc no dibuixa la hitbox de depuració"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto HITBOXES: NO").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.goal == null || !saved.spawns.Any(s => s.kind == "player") || !saved.spawns.Any(s => s.kind == "enemy" && s.id == "Llim") || !saved.spawns.Any(s => s.kind == "merchant" && s.id == "Mercader") || saved.chests.Length != 1)
        { Fail("no s'han guardat el jugador, l'enemic, el mercader i el portal"); yield break; }
        bool partialRouted = partialBlock.layer == BlockData.DrawLayer.OverPlayer && !partialBlock.fullBlock
            ? saved.layers.overlay.Any(c => c.x == 4 && c.y == 0 && c.blockId == partialBlock.id)
            : !partialBlock.passable ? saved.layers.objects.Any(c => c.x == 4 && c.y == 0 && c.blockId == partialBlock.id)
            : saved.layers.ground.Any(c => c.x == 4 && c.y == 0 && c.blockId == partialBlock.id);
        if (!partialRouted) { Fail("el bloc no s'ha enviat a la capa indicada per BlockData"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto PROVAR").onClick.Invoke();
        yield return WaitFor(() => Scene == "Dungeon", 8, "escena real de masmorra per a provar el mapa");
        if (failed) yield break;
        yield return WaitFor(() => FindFirstObjectByType<MapPlaytestRuntime>() != null, 5, "inicialització del mapa en el món real");
        if (failed) yield break;
        yield return null;
        if (FindFirstObjectByType<PlayerHealth>() == null || FindFirstObjectByType<PlayerAttack>() == null ||
            !FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Any() ||
            !FindObjectsByType<Chest>(FindObjectsSortMode.None).Any() ||
            !FindObjectsByType<Shop>(FindObjectsSortMode.None).Any(s => s.IsMerchant) ||
            FindFirstObjectByType<DungeonPortal>() == null ||
            GameObject.Find("Hitboxes parcials del mapa") == null ||
            GameObject.Find("Hitboxes parcials del mapa").GetComponents<BoxCollider2D>().Length == 0)
        { Fail("PROVAR no ha creat jugador, enemic, cofre, mercader i portal reals"); yield break; }
        if (!FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t => t.text == "MAPA COMPLETAT!"))
        { Fail("el jugador no ha completat el mapa en arribar al portal"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto TORNAR A L'EDITOR").onClick.Invoke();
        yield return WaitFor(() => Scene == "Creador", 5, "retorn a l'editor");
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto KIT CASA").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Length == 0 || saved.layers.objects.Length == 0 || !saved.layers.objects.Any(c => c.blockId == "Objectes/Planta_test"))
        { Fail("el kit de casa no ha col·locat terres, parets i planta temàtica"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto COPIAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 3,3").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 4,4").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto ENGANXAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 10,10").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || !saved.layers.ground.Any(c => c.x == 11 && c.y == 11) || !saved.layers.objects.Any(c => c.x == 10 && c.y == 10))
        { Fail("copiar i enganxar no ha conservat les capes"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto NOU 16×16").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        if (buttons.Any(b => b.gameObject.name == "Boto CONFIRMAR")) buttons.First(b => b.gameObject.name == "Boto CONFIRMAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Bloc Terres/Terra").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto OMPLIR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 2,2").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        if (buttons.Any(b => b.gameObject.name == "Boto CONFIRMAR")) buttons.First(b => b.gameObject.name == "Boto CONFIRMAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Length != 256)
        { Fail("el cubell no ha omplit la zona buida connectada"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto NOU 16×16").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto R. PLE/BUIT").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto RECTANGLE").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 2,2").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 4,4").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        if (buttons.Any(b => b.gameObject.name == "Boto CONFIRMAR")) buttons.First(b => b.gameObject.name == "Boto CONFIRMAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Length != 9)
        { Fail("el rectangle ple ha guardat " + (saved == null || saved.layers == null ? -1 : saved.layers.ground.Length) + " cel·les: " + error); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto NOU 16×16").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto R. PLE/BUIT").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto RECTANGLE").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 2,2").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 4,4").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        if (buttons.Any(b => b.gameObject.name == "Boto CONFIRMAR")) buttons.First(b => b.gameObject.name == "Boto CONFIRMAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Length != 8 || saved.layers.ground.Any(c => c.x == 3 && c.y == 3))
        { Fail("el rectangle buit no ha deixat només el contorn: " + (saved == null || saved.layers == null ? -1 : saved.layers.ground.Length)); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto KIT CASTELL").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || !saved.layers.objects.Any(c => c.blockId == "Vegetacio/Seto_verd" && c.x >= 0))
        { Fail("el kit de castell no ha posat vegetació temàtica"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto KIT SUPERMERCAT").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || !saved.layers.objects.Any(c => c.blockId == "Objectes/Carro_compra") || !saved.layers.objects.Any(c => c.blockId == "Natura/Gespa_flors_mixtes"))
        { Fail("el kit de supermercat no ha posat els elements temàtics"); yield break; }
        var multipartMid = BlockCatalog.Instance.blocks.FirstOrDefault(b => b != null && b.multiPart && b.id.EndsWith("_mig"));
        if (multipartMid == null) { Fail("no hi ha cap grup multipart de tres peces per provar"); yield break; }
        string multipartStem = multipartMid.id.Substring(0, multipartMid.id.LastIndexOf('_'));
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto NOU 16×16").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Bloc " + multipartMid.id).onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto PINTAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 8,8").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        if (buttons.Any(b => b.gameObject.name == "Boto CONFIRMAR")) buttons.First(b => b.gameObject.name == "Boto CONFIRMAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error)) { Fail("no s'ha guardat l'objecte multipart: " + error); yield break; }
        var mapCells = saved.layers.ground.Concat(saved.layers.objects).Concat(saved.layers.overlay);
        var multipartPlaced = mapCells.Where(c => c.blockId.StartsWith(multipartStem + "_")).ToArray();
        if (multipartPlaced.Length != 3) { Fail("un objecte de tres peces s'ha col·locat com " + multipartPlaced.Length + " cel·les"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto COPIAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 8,8").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 8,8").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto ENGANXAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 12,8").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error)) { Fail("no s'ha guardat la còpia multipart"); yield break; }
        mapCells = saved.layers.ground.Concat(saved.layers.objects).Concat(saved.layers.overlay);
        multipartPlaced = mapCells.Where(c => c.blockId.StartsWith(multipartStem + "_")).ToArray();
        if (multipartPlaced.Length != 6) { Fail("copiar i enganxar ha separat les peces multipart: " + multipartPlaced.Length); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto ESBORRAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 8,8").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error)) { Fail("no s'ha guardat l'esborrat de l'objecte original"); yield break; }
        mapCells = saved.layers.ground.Concat(saved.layers.objects).Concat(saved.layers.overlay);
        multipartPlaced = mapCells.Where(c => c.blockId.StartsWith(multipartStem + "_")).ToArray();
        if (multipartPlaced.Length != 3) { Fail("esborrar l'original ha eliminat també la còpia multipart"); yield break; }
        bool horizontalBefore = multipartPlaced.Select(c => c.x).Distinct().Count() > 1;
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto ROTAR POSAT").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 12,8").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error)) { Fail("no s'ha guardat la rotació de l'objecte multipart"); yield break; }
        mapCells = saved.layers.ground.Concat(saved.layers.objects).Concat(saved.layers.overlay);
        multipartPlaced = mapCells.Where(c => c.blockId.StartsWith(multipartStem + "_")).ToArray();
        if (multipartPlaced.Length != 3 || (multipartPlaced.Select(c => c.x).Distinct().Count() > 1) == horizontalBefore)
        { Fail("la rotació no ha girat juntes les tres peces multipart"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto VOLTEJAR POSAT").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 12,8").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error)) { Fail("no s'ha guardat el volteig de l'objecte multipart"); yield break; }
        mapCells = saved.layers.ground.Concat(saved.layers.objects).Concat(saved.layers.overlay);
        multipartPlaced = mapCells.Where(c => c.blockId.StartsWith(multipartStem + "_")).ToArray();
        if (multipartPlaced.Length != 3 || multipartPlaced.Any(c => !c.flip))
        { Fail("el volteig no s'ha aplicat a totes les peces multipart"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto ESBORRAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 12,8").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Concat(saved.layers.objects).Concat(saved.layers.overlay).Any(c => c.blockId.StartsWith(multipartStem + "_")))
        { Fail("esborrar una peça no ha eliminat l'objecte multipart sencer"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto NOU 16×16").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Bloc Terres/Terra").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto PINTAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 0,0").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        if (buttons.Any(b => b.gameObject.name == "Boto CONFIRMAR")) buttons.First(b => b.gameObject.name == "Boto CONFIRMAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Length != 1)
        { Fail("el bloc base per a provar rotació no s'ha guardat: " + error); yield break; }
        int rotationBeforePlacedTool = saved.layers.ground[0].rot;
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto ROTAR POSAT").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 0,0").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Length != 1 || saved.layers.ground[0].rot != (rotationBeforePlacedTool + 1) % 4)
        { Fail("rotar un bloc col·locat no ha guardat la nova rotació; abans=" + rotationBeforePlacedTool + ", després=" + (saved == null || saved.layers == null || saved.layers.ground.Length == 0 ? -1 : saved.layers.ground[0].rot)); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto VOLTEJAR POSAT").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Cel·la 0,0").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Length != 1 || !saved.layers.ground[0].flip || saved.layers.ground[0].rot != (rotationBeforePlacedTool + 1) % 4)
        { Fail("voltejar un bloc col·locat no ha guardat el flip"); yield break; }
        MapStore.Delete("Mapa");
        Ok("editor, objectes, prova, kits i copiar/enganxar regions");
        Pass();
    }
}
