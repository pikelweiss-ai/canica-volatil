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
        var cornerCells = new[] {
            new MapCell { x=1,y=1,blockId="Oficina/Panell_recte_H" },
            new MapCell { x=1,y=2,blockId="Oficina/Panell_T" },
            new MapCell { x=2,y=1,blockId="Oficina/Panell_creu" }
        };
        if (MapAutoTile160.Resolve(cornerCells[0], cornerCells).blockId != "Oficina/Panell_canto_NE")
        { Fail("autotile de cantonada de panell incorrecte"); yield break; }
        var pipeCells = new[] {
            new MapCell { x=1,y=1,blockId="Laboratori/Tuberia_H" },
            new MapCell { x=1,y=2,blockId="Laboratori/Tuberia_V" },
            new MapCell { x=2,y=1,blockId="Laboratori/Tuberia_H" },
            new MapCell { x=0,y=1,blockId="Laboratori/Tuberia_H" }
        };
        if (MapAutoTile160.Resolve(pipeCells[0], pipeCells).blockId != "Laboratori/Tuberia_T")
        { Fail("autotile de T de canonada incorrecte"); yield break; }
        var carpetCells = new[] {
            new MapCell { x=0,y=0,blockId="Bany/Catifa_bany_centre" },
            new MapCell { x=1,y=0,blockId="Bany/Catifa_bany_centre" },
            new MapCell { x=0,y=1,blockId="Bany/Catifa_bany_centre" },
            new MapCell { x=1,y=1,blockId="Bany/Catifa_bany_centre" }
        };
        string[] carpetCorners = { "Bany/Catifa_bany_SO", "Bany/Catifa_bany_SE", "Bany/Catifa_bany_NO", "Bany/Catifa_bany_NE" };
        for (int i = 0; i < carpetCells.Length; i++)
            if (MapAutoTile160.Resolve(carpetCells[i], carpetCells).blockId != carpetCorners[i])
            { Fail("autotile de cantonada de catifa incorrecte en " + i); yield break; }
        var overlappingLayers = new[] {
            new MapCell { x=0,y=0,blockId="Natura/Gespa_cami_recte" },
            new MapCell { x=1,y=0,blockId="Terres/Terra" },
            new MapCell { x=1,y=0,blockId="Natura/Gespa_cami_recte" }
        };
        var overlapIndex = MapAutoTile160.Index(overlappingLayers);
        if (MapAutoTile160.Resolve(overlappingLayers[0], overlapIndex).blockId != "Natura/Gespa_cami_final")
        { Fail("l'índex d'autotile ha perdut una capa superposada"); yield break; }
        var directions = new[] { new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(-1,0) };
        foreach (string name in new[] { "Roca_cova_1", "Roca_cova_marro", "Roca_cova_gran", "Roca_cova_molsa", "Roca_cova_magma", "Roca_cova_blava", "Aigua_profunda", "Aigua_nit", "Aigua_fosca" })
        {
            var block = BlockCatalog.Instance.blocks.First(b => b.id == "Natura/" + name);
            for (int mask = 0; mask < 16; mask++)
            {
                var fixture = new System.Collections.Generic.List<MapCell> { new MapCell { x=0,y=0,blockId=block.id } };
                for (int bit = 0; bit < directions.Length; bit++)
                    if ((mask & (1 << bit)) != 0) fixture.Add(new MapCell { x=directions[bit].x,y=directions[bit].y,blockId=block.id });
                var resolvedMask = MapAutoTile160.Resolve(fixture[0], MapAutoTile160.Index(fixture.ToArray()));
                if (resolvedMask.resolvedSprite != block.autoTileSprites[mask])
                { Fail("màscara d'autotile incorrecta per a " + name + ": " + mask); yield break; }
            }
        }
        var railingCorner = new[] {
            new MapCell { x=1,y=1,blockId="Port/Barana_H" },
            new MapCell { x=1,y=2,blockId="Port/Barana_V" },
            new MapCell { x=2,y=1,blockId="Port/Barana_H" }
        };
        if (MapAutoTile160.Resolve(railingCorner[0], railingCorner).blockId != "Port/Barana_canto_NE")
        { Fail("autotile de cantonada de barana incorrecte"); yield break; }
        var railingStraight = new[] {
            new MapCell { x=1,y=1,blockId="Port/Barana_H" },
            new MapCell { x=1,y=2,blockId="Port/Barana_V" },
            new MapCell { x=1,y=0,blockId="Port/Barana_V" }
        };
        if (MapAutoTile160.Resolve(railingStraight[0], railingStraight).blockId != "Port/Barana_V")
        { Fail("autotile recte de barana incorrecte"); yield break; }
        var rect = MapAutoTile160.RotateHitbox(new Rect(-.25f,-.5f,.5f,1f), 1);
        if (Mathf.Abs(rect.xMin + .5f) > .001f || Mathf.Abs(rect.yMin + .25f) > .001f ||
            Mathf.Abs(rect.width - 1f) > .001f || Mathf.Abs(rect.height - .5f) > .001f)
        { Fail("la hitbox no gira amb el bloc"); yield break; }
        var flipped = MapAutoTile160.TransformHitbox(new Rect(-.4f, -.2f, .6f, .4f), 0, true);
        if (Mathf.Abs(flipped.xMin + .2f) > .001f || Mathf.Abs(flipped.yMin + .2f) > .001f)
        { Fail("la hitbox no es reflecteix amb el bloc"); yield break; }
        var map = new MapData { width=2,height=2,layers=new MapLayers {
            ground=new[] { new MapCell {x=0,y=0,blockId="Terres/Terra"} },
            objects=new[] { new MapCell {x=0,y=0,blockId="Bany/Catifa_bany_centre"} } } };
        if (!MapStore.Validate(map, out var error)) { Fail("no permet superposar capes: " + error); yield break; }
        Ok("autotile de roques/aigua (144 màscares), camí, catifa, barana, hitbox i capes"); Pass();
    }
}
