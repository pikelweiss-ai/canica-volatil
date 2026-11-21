using UnityEngine;

public class BlockData : ScriptableObject
{
    public enum DrawLayer { UnderPlayer, OverPlayer }

    public string id, displayName, family, theme;
    public Sprite sprite, scenerySprite;
    public ChestRarity rarity;
    public bool passable, blocksBullets, breakable, fullBlock, hideInside, multiPart, emitsLight;
    public float damage;
    public DrawLayer layer;
    public Rect hitbox;
}
