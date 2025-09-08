using UnityEngine;

public enum TileType
{
    None,
    Cube,
    Balloon,
    Duck,
    Rocket
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    [Header("Position on board")]
    [HideInInspector] public int row;
    [HideInInspector] public int column;

    [Header("Tile Settings")]
    public TileType tileType;
    public bool isItObstacle;

    [Header("References")]
    public SpriteRenderer sr;

    [Header("Other Sprites")]
    public Sprite balloonSprite;
    public Sprite duckSprite;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    public virtual void UpdateSprite()
    {
        sr.sprite = tileType switch
        {
            TileType.Balloon => balloonSprite,
            TileType.Duck => duckSprite,
            TileType.None => null,
            _ => sr.sprite
        };
    }

    private void OnMouseDown()
    {
        GridManager grid = FindObjectOfType<GridManager>();

        if (this is IActivatable activatable)
        {
            activatable.Activate(grid);
        }
        else
        {
            grid.TryMatch(this);
        }
    }
}
