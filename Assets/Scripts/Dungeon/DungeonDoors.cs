using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// v1.4.0: dibuix de les portes de la masmorra, vistes des de dalt.
// Només és visual: la col·lisió continua sent el tilemap de portes del generador (no canvia ningú el camí).
// Estats: 0 = tancada, 1 = mitjana, 2 = oberta. Si la sala les té tancades (combat) sempre es veuen tancades;
// si no, s'obrin sols quan t'hi acostes i es tornen a tancar en allunyar-te.
public class DungeonDoors : MonoBehaviour
{
    class Door
    {
        public readonly SpriteRenderer[] sr = new SpriteRenderer[3];
        public readonly Vector3Int[] cells = new Vector3Int[3];
        public Vector2 center;
        public bool vertical;
        public int state = -1;
    }

    public static DungeonDoors Current { get; private set; }
    readonly List<Door> doors = new List<Door>();
    DungeonGenerator generator;
    DoorArt art;
    string style;
    float lastSound;

    public int Count => doors.Count;
    public int StateAt(int i) => doors[i].state;
    public Vector2 CenterAt(int i) => doors[i].center;
    public bool IsLockedAt(int i) => generator.doors.HasTile(doors[i].cells[1]);

    public static void Attach(DungeonGenerator generator)
    {
        if (Current != null) Destroy(Current.gameObject);
        Current = null;
        var theme = generator.Theme;
        if (theme == null || string.IsNullOrEmpty(theme.doorStyle)) return;
        var art = DoorArt.Load();
        if (art == null) return;
        var go = new GameObject("Portes visuals");
        var component = go.AddComponent<DungeonDoors>();
        component.Build(generator, art, theme.doorStyle);
        Current = component;
    }

    void Build(DungeonGenerator gen, DoorArt doorArt, string doorStyle)
    {
        generator = gen; art = doorArt; style = doorStyle;
        int layer = 0, order = 5;
        var renderer = gen.doors != null ? gen.doors.GetComponent<TilemapRenderer>() : null;
        if (renderer != null) { layer = renderer.sortingLayerID; order = renderer.sortingOrder + 1; renderer.enabled = false; } // el dibuix vell del tile queda amagat; la col·lisió es manté
        foreach (var room in gen.Rooms)
        {
            var cells = room.doorCells;
            for (int i = 0; i + 2 < cells.Count; i += 3)
            {
                var group = new List<Vector3Int> { cells[i], cells[i + 1], cells[i + 2] };
                var door = new Door { vertical = group[0].x == group[1].x }; // caselles apilades en y: passadís horitzontal, fulla vertical
                if (door.vertical) group.Sort((a, b) => b.y.CompareTo(a.y)); else group.Sort((a, b) => a.x.CompareTo(b.x));
                for (int part = 0; part < 3; part++)
                {
                    door.cells[part] = group[part];
                    var child = new GameObject("Porta " + part);
                    child.transform.SetParent(transform, false);
                    child.transform.position = gen.ground.GetCellCenterWorld(group[part]);
                    var sprite = child.AddComponent<SpriteRenderer>();
                    sprite.sortingLayerID = layer; sprite.sortingOrder = order;
                    door.sr[part] = sprite;
                }
                door.center = gen.ground.GetCellCenterWorld(group[1]);
                doors.Add(door);
                Apply(door, 0, false);
            }
        }
    }

    void Apply(Door door, int state, bool sound)
    {
        for (int part = 0; part < 3; part++) door.sr[part].sprite = art.Get(style, door.vertical, state, part);
        if (sound && Time.unscaledTime - lastSound > .15f)
        {
            lastSound = Time.unscaledTime;
            MobSounds.PlayLocal(style == "vidre" ? "porta_vidre_obrir" : "porta_metall_lliscant", door.center, .35f);
        }
        door.state = state;
    }

    static float NearestPerson(Vector2 point, Transform local)
    {
        float best = float.MaxValue;
        if (local != null) best = Vector2.Distance(local.position, point);
        foreach (var avatar in NetShared.Avatars())
            if (avatar != null && !avatar.Down) best = Mathf.Min(best, Vector2.Distance(avatar.transform.position, point));
        return best;
    }

    void Update()
    {
        if (generator == null) return;
        foreach (var door in doors)
        {
            int want = 0;
            if (!generator.doors.HasTile(door.cells[1]))
            {
                float d = NearestPerson(door.center, generator.player);
                // Histèresi: per canviar d'estat cal passar un poc del límit, així la porta no parpelleja
                float open = door.state == 2 ? 2.3f : 1.9f, half = door.state >= 1 ? 3.6f : 3.2f;
                want = d < open ? 2 : d < half ? 1 : 0;
            }
            if (want != door.state) Apply(door, want, want > door.state && door.state >= 0);
        }
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
        if (generator != null && generator.doors != null)
        {
            var renderer = generator.doors.GetComponent<TilemapRenderer>();
            if (renderer != null) renderer.enabled = true;
        }
    }
}
