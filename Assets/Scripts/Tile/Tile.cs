using System;
using System.Collections.Generic;
using Interfaces;
using Managers;
using TileBehaviors;
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
public class Tile : MonoBehaviour, IPoolable
{
    [Header("Position on board")]
    [HideInInspector] public int row;
    [HideInInspector] public int column;

    [Header("Tile Settings")]
    public TileType tileType;
    public ITileBehavior behavior;
    public bool isItObstacle;
    public bool StopFurtherSearch { get; set; }
    public GameObject particleEffect;
    [HideInInspector] public GridManager ownerGrid;
    
    [Header("References")]
    public SpriteRenderer sr;

    [Header("Other Sprites")] public Sprite balloonSprite;
    public Sprite duckSprite;

    public static Action<List<Tile>, List<Vector3>> OnTilesMatched;
    public static Action<Tile, Vector3> OnTileMatched;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
        UpdateSprite();
        InitializeBehavior();
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
        if (this is IActivatable activatable)
        {
            activatable.Activate(ownerGrid);
        }
        else
        {
            ownerGrid.TryMatch(this);
        }
    }

    public void InitializeBehavior()
    {
        behavior = tileType switch
        {
            TileType.Cube => new CubeBehavior(),
            TileType.Balloon => new BalloonBehavior(),
            TileType.Duck => new DuckBehavior(),
            TileType.Rocket => new RocketBehavior(),
            _ => null
        };
    }
    
    public void OnSpawn()
    {
        gameObject.SetActive(true);
        InitializeBehavior();
        ownerGrid = GameManager.Instance.currentGridManager;
    }

    public void OnDespawn()
    {
         gameObject.SetActive(false);
    }
}
