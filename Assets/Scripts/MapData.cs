using System;

[Serializable]
public sealed class MapData
{
    public int format = 1;
    public string name = "Mapa nou";
    public string author = "";
    public int width = 16, height = 16;
    public MapLayers layers = new MapLayers();
    public MapSpawn[] spawns = Array.Empty<MapSpawn>();
    public MapChest[] chests = Array.Empty<MapChest>();
    public MapGoal goal;
}

[Serializable]
public sealed class MapLayers
{
    public MapCell[] ground = Array.Empty<MapCell>();
    public MapCell[] objects = Array.Empty<MapCell>();
    public MapCell[] overlay = Array.Empty<MapCell>();
}

[Serializable]
public struct MapCell
{
    public int x, y, rot;
    public string blockId;
    public bool flip;
}

[Serializable]
public struct MapSpawn { public int x, y; public string kind, id; }
[Serializable]
public struct MapChest { public int x, y; public string rarity; }
[Serializable]
public sealed class MapGoal { public int x, y; }
