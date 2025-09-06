using System;
using UnityEngine;

public enum TileType
{
    None,
    Cube,
    Balloon,
    Duck
}

public enum TileColor
{
    None,
    Red,
    Blue,
    Green,
    Yellow,
    Purple
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    [Header("Position on board")]
    [HideInInspector] public int row;
    [HideInInspector] public int column;
    
    [Header("Tile Settings")]
    public TileType tileType;
    public TileColor tileColor; // for only cubes

    [Header("References")]
    public SpriteRenderer sr;

    [Header("Cube Sprites")]
    public Sprite redSprite;
    public Sprite blueSprite;
    public Sprite greenSprite;
    public Sprite yellowSprite;
    public Sprite purpleSprite;

    [Header("Other Sprites")]
    public Sprite balloonSprite;
    public Sprite duckSprite;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        UpdateSprite();
    }

    public void UpdateSprite()
    {
        sr.sprite = tileType switch
        {
            TileType.Cube => tileColor switch
            {
                TileColor.Red => redSprite,
                TileColor.Blue => blueSprite,
                TileColor.Green => greenSprite,
                TileColor.Yellow => yellowSprite,
                TileColor.Purple => purpleSprite,
                _ => sr.sprite
            },
            TileType.Balloon => balloonSprite,
            TileType.Duck => duckSprite,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Sprite GetSpriteForColor(TileColor color)
    {
        return color switch
        {
            TileColor.Red => redSprite,
            TileColor.Blue => blueSprite,
            TileColor.Green => greenSprite,
            TileColor.Yellow => yellowSprite,
            TileColor.Purple => purpleSprite,
            _ => null
        };
    }

    public void Collect()
    {
        FindObjectOfType<GoalManager>()?.CollectTile(this);
    }

    private void OnMouseDown()
    {
        FindObjectOfType<GridManager>().TryMatch(this);
    }
}
