using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class DungeonVarietyChecks
{
    // Executable separat: permet provar el bot sense substituir el que usa la suite online.
    public static void BuildTestPlayer()
    {
        var scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes) if (scene.enabled) scenes.Add(scene.path);
        var report = BuildPipeline.BuildPlayer(scenes.ToArray(), "Builds/V140QA/Windows/CanicaAlphaReview.exe",
            BuildTarget.StandaloneWindows64, BuildOptions.StrictMode);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Build de proves fallit");
    }

    [MenuItem("Canica/Proves/50 llavors v1.4")]
    public static void Validate()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        int oldFloor = RunState.Floor, cycles = 0;
        var shapes = new HashSet<DungeonGenerator.RoomShape>();
        try
        {
            for (int seed = 1; seed <= 50; seed++)
            {
                var go = new GameObject("Prova de generacio"); go.SetActive(false);
                try
                {
                    var g = go.AddComponent<DungeonGenerator>(); RunState.Floor = 1 + seed % 8;
                    typeof(DungeonGenerator).GetField("rng", flags).SetValue(g, new System.Random(seed));
                    typeof(DungeonGenerator).GetMethod("BuildLayout", flags).Invoke(g, null);
                    typeof(DungeonGenerator).GetMethod("Carve", flags).Invoke(g, null);
                    var floor = (HashSet<Vector2Int>)typeof(DungeonGenerator).GetField("floor", flags).GetValue(g);
                    var solid = (HashSet<Vector2Int>)typeof(DungeonGenerator).GetField("solid", flags).GetValue(g);
                    var seen = new HashSet<Vector2Int>(); var queue = new Queue<Vector2Int>();
                    queue.Enqueue(g.Rooms[0].Center); seen.Add(g.Rooms[0].Center);
                    var dirs = new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
                    while (queue.Count > 0)
                    {
                        var p = queue.Dequeue();
                        foreach (var d in dirs) { var n = p + d; if (floor.Contains(n) && !solid.Contains(n) && seen.Add(n)) queue.Enqueue(n); }
                    }
                    int edges = 0;
                    foreach (var room in g.Rooms)
                    {
                        shapes.Add(room.shape); edges += room.links.Count;
                        if (!seen.Contains(room.Center)) throw new Exception($"Llavor {seed}: sala aillada");
                        foreach (var door in room.doorCells)
                            if (!seen.Contains(new Vector2Int(door.x, door.y))) throw new Exception($"Llavor {seed}: porta bloquejada");
                        foreach (var p in room.rect.allPositionsWithin)
                            if ((Mathf.Abs(p.x - room.Center.x) <= 1 || Mathf.Abs(p.y - room.Center.y) <= 1) && (!floor.Contains(p) || solid.Contains(p)))
                                throw new Exception($"Llavor {seed}: creu central bloquejada");
                    }
                    if (edges / 2 >= g.Rooms.Count) { if (RunState.Floor < 3) throw new Exception("Anell abans del pis 3"); cycles++; }
                }
                finally { UnityEngine.Object.DestroyImmediate(go); }
            }
            if (shapes.Count != 7 || cycles == 0) throw new Exception("Cobertura incompleta de formes/anells");
            Debug.Log($"DUNGEON_V140_PASS: 50 llavors, {shapes.Count} formes, {cycles} plantes amb anells; centres, portes i creus accessibles");
        }
        finally { RunState.Floor = oldFloor; }
    }
}
