using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class NetBot
{
    IEnumerator Share160()
    {
        yield return WaitFor(() => Scene == "MainMenu", 20, "compartir mapes v1.6");
        var map = new MapData { name="Compartit", width=3,height=2, layers=new MapLayers { ground=new[] { new MapCell{x=2,y=1,blockId="Terres/Terra",rot=2} } } };
        if (!MapShare160.Encode(map,out var code,out var error) || !code.StartsWith("CVM1:") || !MapShare160.Decode(code,out var decoded,out error) || decoded.layers.ground[0].blockId!="Terres/Terra")
        { Fail("codi CVM1 no fa anada i tornada: "+error); yield break; }
        string path=Path.Combine(Application.temporaryCachePath,"netbot-map.can icamap").Replace(".can icamap",".canicamap");
        if (!MapShare160.ExportFile(map,path,out error) || !MapShare160.ImportFile(path,out var imported,out error) || imported.name!=map.name)
        { if(File.Exists(path))File.Delete(path); Fail("fitxer de mapa no fa anada i tornada: "+error); yield break; }
        File.WriteAllText(path,"{ invalid json");
        bool rejects=!MapShare160.ImportFile(path,out _,out _); File.Delete(path);
        if(!rejects){Fail("s'ha acceptat un fitxer corrupta");yield break;}
        if (!MapImport160.SaveCodeForPlay(code, out var temporaryName, out error) ||
            !MapStore.Load(temporaryName, out var temporaryMap, out error) || temporaryMap.layers.ground[0].blockId != "Terres/Terra")
        { Fail("no es pot preparar el mapa importat per a jugar: " + error); yield break; }
        MapPlaytest.MapName = temporaryName; MapPlaytest.StartInDungeon = true; MapPlaytest.DeleteMapAfterConsume = true;
        if (!MapPlaytest.TryConsumeDungeonMap(out var consumed) || consumed == null || MapStore.List().Contains(temporaryName))
        { Fail("el mapa de codi no s'ha consumit o no s'ha netejat després de carregar-lo"); yield break; }
        var playButton = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "Boto JUGAR");
        if (playButton == null) { Fail("no s'ha trobat JUGAR per obrir el selector"); yield break; }
        playButton.onClick.Invoke();
        yield return WaitFor(() => FindFirstObjectByType<MapSelectUI>() != null, 4, "selector amb importació de mapes per codi");
        // Jugar un mapa GUARDAT des del selector: ha de carregar la masmorra real i NO esborrar el mapa.
        const string savedName = "NETBOT-Guardat";
        var savedMap = new MapData { name = savedName, width = 3, height = 2, layers = new MapLayers { ground = new[] { new MapCell { x = 0, y = 0, blockId = "Terres/Terra" } } } };
        if (!MapStore.Save(savedMap, savedName, out error)) { Fail("no s'ha pogut preparar el mapa guardat: " + error); yield break; }
        var savedButton = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "Boto JUGAR MAPA GUARDAT");
        if (savedButton == null) { MapStore.Delete(savedName); Fail("falta el botó JUGAR MAPA GUARDAT al selector"); yield break; }
        const string trashName = "NETBOT-Paperera";
        if (!MapStore.Save(new MapData { name = trashName, width = 2, height = 2 }, trashName, out error)) { MapStore.Delete(savedName); Fail("no s'ha pogut preparar el mapa per a la paperera: " + error); yield break; }
        savedButton.onClick.Invoke();
        // Paperera: el primer clic només arma l'esborrat (el mapa continua), el segon l'esborra; l'altre mapa no es toca.
        var trashButton = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "Paperera " + trashName);
        if (trashButton == null) { MapStore.Delete(savedName); MapStore.Delete(trashName); Fail("falta la paperera a la llista de mapes guardats"); yield break; }
        trashButton.onClick.Invoke();
        if (!MapStore.List().Contains(trashName)) { MapStore.Delete(savedName); Fail("la paperera ha esborrat el mapa al primer clic: ha de demanar confirmació"); yield break; }
        yield return null; // la llista es reconstrueix
        trashButton = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "Paperera " + trashName);
        if (trashButton == null) { MapStore.Delete(savedName); MapStore.Delete(trashName); Fail("la paperera ha desaparegut després del primer clic"); yield break; }
        trashButton.onClick.Invoke();
        bool trashStillThere = MapStore.List().Contains(trashName);
        if (trashStillThere) MapStore.Delete(trashName);
        if (trashStillThere) { MapStore.Delete(savedName); Fail("la paperera no ha esborrat el mapa al segon clic"); yield break; }
        if (!MapStore.List().Contains(savedName)) { Fail("la paperera ha esborrat un altre mapa"); yield break; }
        yield return null;
        var savedRow = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "Mapa per jugar " + savedName);
        if (savedRow == null) { MapStore.Delete(savedName); Fail("la llista de mapes guardats no mostra el mapa"); yield break; }
        savedRow.onClick.Invoke();
        yield return WaitFor(() => Scene == "Dungeon" && FindFirstObjectByType<MapPlaytestRuntime>() != null, 10, "mapa guardat carregat en el joc real");
        bool savedStillThere = MapStore.List().Contains(savedName);
        MapStore.Delete(savedName);
        if (failed) yield break;
        if (!savedStillThere) { Fail("jugar un mapa guardat l'ha esborrat del disc"); yield break; }
        Ok("jugar un mapa guardat des del selector: carrega la masmorra i el mapa continua guardat");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        yield return WaitFor(() => Scene == "MainMenu", 20, "tornar al menú després del mapa guardat");
        var playAgain = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "Boto JUGAR");
        if (playAgain == null) { Fail("no s'ha trobat JUGAR en tornar al menú"); yield break; }
        playAgain.onClick.Invoke();
        yield return WaitFor(() => FindFirstObjectByType<MapSelectUI>() != null, 4, "selector, segona vegada");
        var importButton = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "Boto IMPORTAR CODI I JUGAR");
        if (importButton == null)
        { Fail("falta l'opció per jugar codis de mapa des del selector"); yield break; }
        importButton.onClick.Invoke();
        var codeField = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None).FirstOrDefault(f => f.gameObject.name == "Camp Codi CVM1");
        var playMapButton = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.gameObject.name == "Boto JUGAR MAPA");
        if (codeField == null || playMapButton == null) { Fail("el panell d'importació no mostra el codi i el botó de jugar"); yield break; }
        codeField.text = code; playMapButton.onClick.Invoke();
        yield return WaitFor(() => Scene == "Dungeon" && FindFirstObjectByType<MapPlaytestRuntime>() != null, 10,
            "mapa CVM1 carregat en el joc real");
        if (failed) yield break;
        Ok("CVM1, fitxer .canicamap, selector mòbil i joc real del mapa importat"); Pass();
    }
}
