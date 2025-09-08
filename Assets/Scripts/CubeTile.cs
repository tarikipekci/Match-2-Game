using UnityEngine;

public enum TileColor
{
    None,
    Red,
    Blue,
    Green,
    Yellow,
    Purple
}

public class CubeTile : Tile
{
    [Header("Cube Settings")]
    public TileColor tileColor;

    [Header("Cube Sprites")]
    public Sprite redSprite;
    public Sprite blueSprite;
    public Sprite greenSprite;
    public Sprite yellowSprite;
    public Sprite purpleSprite;

    public override void UpdateSprite()
    {
        tileType = TileType.Cube;
        sr.sprite = tileColor switch
        {
            TileColor.Red => redSprite,
            TileColor.Blue => blueSprite,
            TileColor.Green => greenSprite,
            TileColor.Yellow => yellowSprite,
            TileColor.Purple => purpleSprite,
            _ => sr.sprite
        };
    }

    public Sprite GetSpriteForColor(TileColor color)
    {
        return color switch
        {
            TileColor.None => null,
            TileColor.Red => redSprite,
            TileColor.Blue => blueSprite,
            TileColor.Green => greenSprite,
            TileColor.Yellow => yellowSprite,
            TileColor.Purple => purpleSprite,
            _ => null
        };
    }
}
