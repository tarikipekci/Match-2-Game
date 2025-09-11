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
    [Header("Position on board")] [HideInInspector]
    public int row;

    [HideInInspector] public int column;

    [Header("Tile Settings")] public TileType tileType;
    public ITileBehavior behavior;
    public bool isItObstacle;
    private bool isMatchable;
    public bool StopFurtherSearch { get; set; }
    public GameObject particleEffect;
    [SerializeField] public GameObject ballonParticleEffect;
    [SerializeField] public GameObject duckParticleEffect;
    [HideInInspector] public GridManager ownerGrid;

    [Header("References")] public SpriteRenderer sr;

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
        SetIsMatchable();
        SetParticleEffect();
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
            ownerGrid.moveManager.UseMove();
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

    private void SetIsMatchable()
    {
        isMatchable = tileType switch
        {
            TileType.Balloon => true,
            TileType.Duck => false,
            TileType.Rocket => false,
            TileType.Cube => true,
            TileType.None => false,
            _ => isMatchable
        };
    }
    
    private void SetParticleEffect()
    {
        particleEffect = tileType switch
        {
            TileType.Balloon => ballonParticleEffect,
            TileType.Duck => duckParticleEffect,
            TileType.None => null,
            _ => particleEffect
        };
    }

    public bool GetIsMatchable()
    {
        return isMatchable;
    }
    
    public void SetIsMatchable(bool newValue)
    {
        isMatchable = newValue;
    }

    public void OnSpawn()
    {
        gameObject.SetActive(true);
        InitializeBehavior();
        ownerGrid = GameManager.Instance.currentGridManager;
        SetIsMatchable();
        SetParticleEffect();
    }

    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }
}