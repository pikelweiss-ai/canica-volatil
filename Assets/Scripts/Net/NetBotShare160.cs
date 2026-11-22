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
