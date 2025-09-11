using System.Collections.Generic;
using Helper;
using UnityEngine;

namespace Managers
{
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            Tile.OnTilesMatched += SpawnParticlesForMatchedTiles;
            Tile.OnTileMatched += SpawnParticlesForMatchedTile;
        }

        private void OnDisable()
        {
            Tile.OnTilesMatched -= SpawnParticlesForMatchedTiles;
            Tile.OnTileMatched -= SpawnParticlesForMatchedTile;
        }

        private void SpawnParticlesForMatchedTile(Tile tile, Vector3 position)
        {
            if (tile == null) return;

            // Decide which particle effect to spawn based on tile type
            switch (tile.tileType)
            {
                case TileType.Cube:
                    SpawnCubeParticles(tile as CubeTile, position);
                    break;
                case TileType.Balloon:
                case TileType.Duck:
                    SpawnPassiveParticles(tile, position);
                    break;
            }
        }

        private void SpawnParticlesForMatchedTiles(List<Tile> tiles, List<Vector3> positions)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];
                Vector3 pos = positions[i];
                if (tile == null) continue;

                // Spawn appropriate particles for each matched tile
                switch (tile.tileType)
                {
                    case TileType.Cube:
                        SpawnCubeParticles(tile as CubeTile, pos);
                        break;
                    case TileType.Balloon:
                    case TileType.Duck:
                        SpawnPassiveParticles(tile, pos);
                        break;
                }
            }
        }

        private void SpawnCubeParticles(CubeTile cubeTile, Vector3 position)
        {
            if (cubeTile == null || cubeTile.particleEffect == null) return;

            Color color = SelectTileColor.GetColor(cubeTile.tileColor); // Get color based on tile
            SpawnParticles(cubeTile.particleEffect, position, color);
        }

        public void SpawnPassiveParticles(Tile tile, Vector3 position)
        {
            if (tile.particleEffect == null) return;
            SpawnParticles(tile.particleEffect, position, Color.white);
        }

        private void SpawnParticles(GameObject prefab, Vector3 position, Color color)
        {
            GameObject psObj = Instantiate(prefab, position, Quaternion.identity);

            var mainPS = psObj.GetComponent<ParticleSystem>();
            if (mainPS == null) return;

            var main = mainPS.main;
            main.playOnAwake = false;
            main.startDelay = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World; // Ensure particles are in world space

            // Update color for sub-emitters as well
            var subEmittersModule = mainPS.subEmitters;
            for (int i = 0; i < subEmittersModule.subEmittersCount; i++)
            {
                var sub = subEmittersModule.GetSubEmitterSystem(i);
                if (sub == null) continue;

                var subMain = sub.main;
                subMain.startColor = color;
                subMain.startDelay = 0f;
                subMain.simulationSpace = ParticleSystemSimulationSpace.World;
            }

            var renderer = psObj.GetComponentInChildren<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = "Default";
                renderer.sortingOrder = 100;
            }

            mainPS.Emit(1); // Emit a single burst
            Destroy(psObj, mainPS.main.startLifetime.constantMax + 0.1f); // Clean up after lifetime
        }
    }
}
