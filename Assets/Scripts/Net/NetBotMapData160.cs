using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

public partial class NetBot
{
    IEnumerator MapData160()
    {
        yield return WaitFor(() => Scene == "MainMenu", 20, "magatzem de mapes v1.6");
        if (failed) yield break;
        var map = new MapData { name = "Prova fase 2", width = 4, height = 3,
            layers = new MapLayers { ground = new[] { new MapCell { x = 1, y = 2, blockId = "Terres/Terra", rot = 3, flip = true } } } };
        if (!MapStore.Save(map, "NETBOT-Prova-fase2", out string error))
        { Fail("no es pot guardar el mapa: " + error); yield break; }
        if (!MapStore.List().Contains("NETBOT-Prova-fase2") ||
            !MapStore.Load("NETBOT-Prova-fase2", out var loaded, out error) ||
            loaded.width != map.width || loaded.height != map.height ||
            loaded.layers.ground.Length != 1 || loaded.layers.ground[0].blockId != "Terres/Terra" ||
            loaded.layers.ground[0].rot != 3 || !loaded.layers.ground[0].flip)
        { MapStore.Delete("NETBOT-Prova-fase2"); Fail("guardar i carregar no conserva el mapa: " + error); yield break; }
        string path = Path.Combine(Application.persistentDataPath, "Mapes", "NETBOT-Corrupte.can icamap");
        path = path.Replace(".can icamap", ".canicamap");
        File.WriteAllText(path, "{ broken json");
        bool rejected = !MapStore.Load("NETBOT-Corrupte", out _, out _);
        File.Delete(path);
        bool deleted = MapStore.Delete("NETBOT-Prova-fase2");
        MapStore.Save(map,"__autosave",out _);
        bool hiddenAutosave = !MapStore.List().Contains("__autosave") && MapStore.Load("__autosave",out _,out _);
        MapStore.Delete("__autosave");
        if (!rejected || !deleted || !hiddenAutosave)
        { Fail("el fitxer corrupte no es rebutja o no es pot esborrar"); yield break; }
        Ok("guardar, carregar, llistar, esborrar i rebutjar JSON corrupte");
        Pass();
    }
}
