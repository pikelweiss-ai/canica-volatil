using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class BuildCompatibility
{
    [Serializable] class Manifest
    {
        public string platform, version, sourceSha256;
        public int protocol;
        public string[] weapons, heals;
    }
    public static void Write(string folder, string platform)
    {
        var catalog = Resources.Load<NetContentCatalog>("NetContent");
        var source = new StringBuilder();
        foreach (var path in Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
            source.Append(path.Replace('\\', '/')).Append('\n').Append(File.ReadAllText(path).Replace("\r\n", "\n")).Append('\n');
        string hash;
        using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(source.ToString()))).Replace("-", "").ToLowerInvariant();
        var manifest = new Manifest {
            platform = platform, version = GameVersion.Package, protocol = NetProtocol.Version, sourceSha256 = hash,
            weapons = catalog.weapons.Select(w => w.name).OrderBy(n => n, StringComparer.Ordinal).ToArray(),
            heals = HealItem.All.Select(h => h.name).OrderBy(n => n, StringComparer.Ordinal).ToArray()
        };
        File.WriteAllText(Path.Combine(folder, "compatibility.json"), JsonUtility.ToJson(manifest, true));
    }
}
