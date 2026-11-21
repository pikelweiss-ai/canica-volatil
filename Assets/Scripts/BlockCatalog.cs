using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockCatalog : ScriptableObject
{
    public BlockData[] blocks = Array.Empty<BlockData>();
    public DungeonTheme[] themes = Array.Empty<DungeonTheme>();

    static BlockCatalog instance;
    public static BlockCatalog Instance => instance != null ? instance : instance = Resources.Load<BlockCatalog>("BlockCatalog");

    Dictionary<Sprite, BlockData> bySprite;
    Dictionary<string, BlockData> byName;

    public BlockData Find(Sprite sprite)
    {
        if (sprite == null) return null;
        if (bySprite == null)
        {
            bySprite = new Dictionary<Sprite, BlockData>();
            byName = new Dictionary<string, BlockData>(StringComparer.OrdinalIgnoreCase);
            foreach (var block in blocks)
            {
                if (block == null || block.sprite == null) continue;
                bySprite[block.sprite] = block;
                if (block.scenerySprite != null && !bySprite.ContainsKey(block.scenerySprite)) bySprite.Add(block.scenerySprite, block);
                if (!byName.ContainsKey(block.sprite.name)) byName.Add(block.sprite.name, block);
            }
        }
        if (bySprite.TryGetValue(sprite, out var exact)) return exact;
        return byName.TryGetValue(sprite.name, out var sameName) ? sameName : null;
    }
}
