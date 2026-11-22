using System.Collections;
using System.IO;
using UnityEngine;

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
        Ok("codi CVM1, exportació/importació .canicamap i rebuig de fitxer corrupta"); Pass();
    }
}
