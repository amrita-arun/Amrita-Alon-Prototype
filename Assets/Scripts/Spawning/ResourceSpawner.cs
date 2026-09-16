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

    [Header("Comeback Spawning")]
    [Tooltip("No spawn-ratio adjustment until the size difference reaches this value.")]
    [SerializeField] private float sizeDifferenceBeforeBias = 0.75f;

    [Tooltip("The size difference at which the maximum spawn bias is reached.")]
    [SerializeField] private float sizeDifferenceForMaximumBias = 3f;

    [Tooltip("0.25 allows the spawn ratio to shift from 50/50 up to 75/25.")]
    [Range(0f, 0.45f)]
    [SerializeField] private float maximumComebackBias = 0.25f;

    private Transform circlePlayer;
    private Transform trianglePlayer;

    private void Start()
    {
        FindPlayers();

        for (int i = 0; i < startingResourceCount; i++)
        {
            TrySpawnResource();
        }

        StartCoroutine(SpawnContinuously());
    }

    private void FindPlayers()
    {
        GameObject circleObject =
            GameObject.FindGameObjectWithTag("Circle");

        GameObject triangleObject =
            GameObject.FindGameObjectWithTag("Triangle");

        if (circleObject != null)
        {
            circlePlayer = circleObject.transform;
        }

        if (triangleObject != null)
        {
            trianglePlayer = triangleObject.transform;
        }
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
            Vector3 spawnPosition = new Vector3(
                Random.Range(minimumX, maximumX),
                Random.Range(minimumY, maximumY),
                0f
            );

            bool positionIsOccupied = Physics.CheckSphere(
                spawnPosition,
                spawnClearance,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

            if (!positionIsOccupied)
            {
                float circleSpawnChance = CalculateCircleSpawnChance();

                GameObject prefabToSpawn =
                    Random.value < circleSpawnChance
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

    private float CalculateCircleSpawnChance()
    {
        // Use an even ratio if either player no longer exists.
        if (circlePlayer == null || trianglePlayer == null)
        {
            return 0.5f;
        }

        float circleSize = circlePlayer.localScale.x;
        float triangleSize = trianglePlayer.localScale.x;
        float sizeDifference = circleSize - triangleSize;
        float absoluteDifference = Mathf.Abs(sizeDifference);

        // Keep normal 50/50 spawning while their sizes are reasonably close.
        if (absoluteDifference <= sizeDifferenceBeforeBias)
        {
            return 0.5f;
        }

        float biasProgress = Mathf.InverseLerp(
            sizeDifferenceBeforeBias,
            sizeDifferenceForMaximumBias,
            absoluteDifference
        );

        float currentBias = maximumComebackBias * biasProgress;

        if (circleSize > triangleSize)
        {
            // Circle is winning: spawn fewer circles and more triangles.
            return 0.5f - currentBias;
        }

        // Triangle is winning: spawn more circles and fewer triangles.
        return 0.5f + currentBias;
    }
}