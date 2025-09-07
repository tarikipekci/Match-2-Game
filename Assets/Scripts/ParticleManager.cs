using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    [Header("Particle Prefab")] public GameObject cubeParticlePrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SpawnCubeParticles(Vector3 position, Color cubeColor)
    {
        if (cubeParticlePrefab == null) return;

        GameObject psObj = Instantiate(cubeParticlePrefab, position, Quaternion.identity);

        var mainPS = psObj.GetComponent<ParticleSystem>();
        if (mainPS == null) return;

        var main = mainPS.main;
        main.playOnAwake = false;
        main.startDelay = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var subEmittersModule = mainPS.subEmitters;
        for (int i = 0; i < subEmittersModule.subEmittersCount; i++)
        {
            var sub = subEmittersModule.GetSubEmitterSystem(i);
            if (sub == null) continue;

            var subMain = sub.main;
            subMain.startColor = cubeColor;
            subMain.startDelay = 0f;
            subMain.simulationSpace = ParticleSystemSimulationSpace.World;
        }

        var renderer = psObj.GetComponentInChildren<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = "Default"; 
            renderer.sortingOrder = 100;
        }

        mainPS.Emit(1);
        Destroy(psObj, mainPS.main.startLifetime.constantMax + 0.1f);
    }
}