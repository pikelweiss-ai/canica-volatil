using System;

public static class MapImport160
{
    public static bool SaveCodeForPlay(string code, out string storedName, out string error)
    {
        storedName = null;
        if (!MapShare160.Decode(code, out var map, out error)) return false;
        storedName = "__play_" + Guid.NewGuid().ToString("N");
        if (MapStore.Save(map, storedName, out error)) return true;
        storedName = null;
        return false;
    }
}
