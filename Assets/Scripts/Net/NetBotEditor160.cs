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
        var dragTrace = MapEditor.TraceLine(new Vector2Int(1, 1), new Vector2Int(4, 4));
        if (dragTrace.Length != 4 || dragTrace[0] != new Vector2Int(1, 1) || dragTrace[3] != new Vector2Int(4, 4))
        { Fail("la interpolació del traç deixa buits entre cel·les"); yield break; }
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
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.goal == null || !saved.spawns.Any(s => s.kind == "player") || !saved.spawns.Any(s => s.kind == "enemy" && s.id == "Llim"))
        { Fail("no s'han guardat el jugador, l'enemic i el portal"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto PROVAR").onClick.Invoke();
        yield return WaitFor(() => Scene == "MapaProva", 5, "prova de mapa");
        if (failed) yield break;
        yield return null;
        if (!FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t => t.text == "MAPA COMPLETAT!"))
        { Fail("el jugador no ha completat el mapa en arribar al portal"); yield break; }
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto TORNAR A L'EDITOR").onClick.Invoke();
        yield return WaitFor(() => Scene == "Creador", 5, "retorn a l'editor");
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto KIT CASA").onClick.Invoke();
        buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        buttons.First(b => b.gameObject.name == "Boto GUARDAR").onClick.Invoke();
        if (!MapStore.Load("Mapa", out saved, out error) || saved.layers.ground.Length == 0 || saved.layers.objects.Length == 0)
        { Fail("el kit de casa no ha col·locat terres i parets"); yield break; }
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
        MapStore.Delete("Mapa");
        Ok("editor, objectes, prova, kits i copiar/enganxar regions");
        Pass();
    }
}
