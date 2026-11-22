using System.Collections;
using System.Linq;
using UnityEngine;

public partial class NetBot
{
    IEnumerator Tile160()
    {
        yield return WaitFor(() => Scene == "MainMenu", 20, "autotile i geometria v1.6");
        var cells = new[] {
            new MapCell { x=1,y=1,blockId="Natura/Gespa_cami_recte" },
            new MapCell { x=1,y=2,blockId="Natura/Gespa_cami_recte" },
            new MapCell { x=2,y=1,blockId="Natura/Gespa_cami_recte" },
            new MapCell { x=0,y=1,blockId="Natura/Gespa_cami_recte" }
        };
        var resolved = MapAutoTile160.Resolve(cells[0], cells);
        if (resolved.blockId != "Natura/Gespa_cami_T") { Fail("autotile no ha triat la peça T: " + resolved.blockId); yield break; }
        var rect = MapAutoTile160.RotateHitbox(new Rect(-.25f,-.5f,.5f,1f), 1);
        if (Mathf.Abs(rect.width - 1f) > .001f || Mathf.Abs(rect.height - .5f) > .001f)
        { Fail("la hitbox no gira amb el bloc"); yield break; }
        var map = new MapData { width=2,height=2,layers=new MapLayers {
            ground=new[] { new MapCell {x=0,y=0,blockId="Terres/Terra"} },
            objects=new[] { new MapCell {x=0,y=0,blockId="Bany/Catifa_bany_centre"} } } };
        if (!MapStore.Validate(map, out var error)) { Fail("no permet superposar capes: " + error); yield break; }
        Ok("autotile de camí, hitbox rotada i capes superposades"); Pass();
    }
}
