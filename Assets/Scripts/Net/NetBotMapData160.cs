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
        var denseCells = new MapCell[64 * 64];
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++) denseCells[y * 64 + x] = new MapCell { x = x, y = y, blockId = "Terres/Terra" };
        var denseMap = new MapData { name = "Mapa dens", width = 64, height = 64,
            layers = new MapLayers { ground = denseCells, objects = System.Array.Empty<MapCell>(), overlay = System.Array.Empty<MapCell>() } };
        if (!MapStore.Save(denseMap, "NETBOT-Dens64", out error) ||
            !MapStore.Load("NETBOT-Dens64", out var denseLoaded, out error) || denseLoaded.layers.ground.Length != 4096)
        { MapStore.Delete("NETBOT-Dens64"); Fail("no es pot guardar i carregar un 64×64 ple: " + error); yield break; }
        var denseFile = new FileInfo(Path.Combine(Application.persistentDataPath, "Mapes", "NETBOT-Dens64.canicamap"));
        if (!denseFile.Exists || denseFile.Length > MapStore.MaxFileBytes)
        { MapStore.Delete("NETBOT-Dens64"); Fail("el 64×64 comprimit supera el límit de fitxer"); yield break; }
        byte[] legacyJson = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(map));
        if (!MapStore.TryDecodeFile(legacyJson, out var legacyLoaded, out error) ||
            legacyLoaded.layers.ground[0].blockId != "Terres/Terra" || legacyLoaded.layers.ground[0].rot != 3 || !legacyLoaded.layers.ground[0].flip)
        { MapStore.Delete("NETBOT-Dens64"); Fail("no es poden llegir mapes JSON antics: " + error); yield break; }
        Ok("mapa ple 64×64 en " + denseFile.Length + " bytes comprimits; JSON antic compatible");
        MapStore.Delete("NETBOT-Dens64");
        var futureBlock = new MapData { width = 1, height = 1,
            layers = new MapLayers { ground = new[] { new MapCell { x = 0, y = 0, blockId = "Antic/BlocRetirat" } } } };
        if (!MapStore.Save(futureBlock, "NETBOT-BlocDesconegut", out error) ||
            !MapStore.Load("NETBOT-BlocDesconegut", out var unknownLoaded, out error) ||
            unknownLoaded.layers.ground.Length != 1 || unknownLoaded.layers.ground[0].blockId != "Antic/BlocRetirat" ||
            BlockCatalog.Instance.blocks.Any(b => b.id == unknownLoaded.layers.ground[0].blockId))
        { MapStore.Delete("NETBOT-BlocDesconegut"); Fail("el bloc desconegut no es conserva com a dada"); yield break; }
        MapStore.Delete("NETBOT-BlocDesconegut");
        Ok("guardar, carregar, llistar, esborrar i rebutjar JSON corrupte");
        Pass();
    }
}
