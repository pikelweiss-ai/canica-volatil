using System.Collections;
using UnityEngine;

// v1.4.0 fase 4: les portes de la masmorra s'obrin segons la distància del jugador i no bloquegen el camí.
public partial class NetBot
{
    IEnumerator Doors140(Transform player, Rigidbody2D body)
    {
        var doors = DungeonDoors.Current;
        if (doors == null || doors.Count == 0) { Fail("no hi ha portes visuals a la masmorra"); yield break; }
        var generator = DungeonGenerator.Instance;
        int pick = -1;
        for (int i = 0; i < doors.Count; i++) if (!doors.IsLockedAt(i)) { pick = i; break; }
        if (pick < 0) { Fail("totes les portes estan tancades amb clau"); yield break; }
        Vector2 center = doors.CenterAt(pick);
        void Place(Vector2 at) { player.position = at; if (body != null) body.position = at; }

        Place(center + new Vector2(9f, 0f));
        yield return new WaitForSeconds(.25f);
        if (doors.StateAt(pick) != 0) { Fail("la porta no està tancada quan el jugador és lluny"); yield break; }
        Place(center + new Vector2(2.7f, 0f));
        yield return new WaitForSeconds(.25f);
        if (doors.StateAt(pick) != 1) { Fail("la porta no està a mitges a distància mitjana (estat " + doors.StateAt(pick) + ")"); yield break; }
        Place(center + new Vector2(.8f, 0f));
        yield return new WaitForSeconds(.25f);
        if (doors.StateAt(pick) != 2) { Fail("la porta no s'obri quan el jugador s'hi acosta (estat " + doors.StateAt(pick) + ")"); yield break; }
        Place(center + new Vector2(9f, 0f));
        yield return new WaitForSeconds(.25f);
        if (doors.StateAt(pick) != 0) { Fail("la porta no es tanca en allunyar-se"); yield break; }
        var cell = generator.ground.WorldToCell(center);
        if (generator.doors.HasTile(cell) || TileNav.IsBlocked(cell)) { Fail("una porta sense clau bloqueja el camí"); yield break; }
        Ok("portes: tancada lluny, mitjana de mitjana distància, oberta a prop; no bloquegen el camí (" + doors.Count + " portes)");
    }
}
