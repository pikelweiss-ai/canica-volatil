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
        ClickButton("JUGAR");
        yield return WaitFor(() => FindFirstObjectByType<MapSelectUI>() != null, 5, "selector de mapes");
        var creator = FindObjectsByType<Button>(FindObjectsSortMode.None)
            .FirstOrDefault(b => b.gameObject.name == "Mapa Creador");
        if (creator == null) { Fail("falta la targeta Creador"); yield break; }
        creator.onClick.Invoke();
        yield return WaitFor(() => Scene == "Creador", 5, "escena Creador");
        if (failed) yield break;
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
        MapStore.Delete("Mapa");
        Ok("targeta, escena, graella 16×16, rotació, desfer/refer, guardar i obrir");
        Pass();
    }
}
