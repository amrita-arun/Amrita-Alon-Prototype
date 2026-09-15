using System.Collections;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Resource Prefabs")]
    [SerializeField] private GameObject circleResourcePrefab;
    [SerializeField] private GameObject triangleResourcePrefab;
    [SerializeField] private Transform resourcesParent;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 0.75f;
    [SerializeField] private int startingResourceCount = 10;
    [SerializeField] private int maximumResourceCount = 20;

    [Header("Spawn Area")]
    [SerializeField] private float minimumX = -7.8f;
    [SerializeField] private float maximumX = 7.8f;
    [SerializeField] private float minimumY = -3.9f;
    [SerializeField] private float maximumY = 3.9f;

    [Header("Collision Avoidance")]
    [SerializeField] private float spawnClearance = 0.5f;
    [SerializeField] private int maximumSpawnAttempts = 20;

    private void Start()
    {
        for (int i = 0; i < startingResourceCount; i++)
        {
            TrySpawnResource();
        }

        StartCoroutine(SpawnContinuously());
    }

    private IEnumerator SpawnContinuously()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (resourcesParent.childCount < maximumResourceCount)
            {
                TrySpawnResource();
            }
        }
    }

    private void TrySpawnResource()
    {
        for (int attempt = 0; attempt < maximumSpawnAttempts; attempt++)
        {
            Vector2 spawnPosition = new Vector2(
                Random.Range(minimumX, maximumX),
                Random.Range(minimumY, maximumY)
            );

            bool positionIsOccupied =
                Physics2D.OverlapCircle(spawnPosition, spawnClearance) != null;

            if (!positionIsOccupied)
            {
                GameObject prefabToSpawn =
                    Random.value < 0.5f
                        ? circleResourcePrefab
                        : triangleResourcePrefab;

                Instantiate(
                    prefabToSpawn,
                    spawnPosition,
                    Quaternion.identity,
                    resourcesParent
                );

                return;
            }
        }
    }
}