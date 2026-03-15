using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyNavMesh enemyPrefab;
    [SerializeField] private CarHealth targetCar;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn")]
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxAliveEnemies = 20;
    [SerializeField] private float navMeshSnapDistance = 4f;

    private readonly List<EnemyNavMesh> aliveEnemies = new List<EnemyNavMesh>();

    private void Start()
    {
        if (targetCar == null)
        {
            targetCar = FindObjectOfType<CarHealth>();
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: enemyPrefab is not assigned.");
            enabled = false;
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("EnemySpawner: spawnPoints are not assigned.");
            enabled = false;
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        while (targetCar != null && !targetCar.IsDead)
        {
            CleanupAliveList();

            if (aliveEnemies.Count < maxAliveEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPosition = point.position;

        if (NavMesh.SamplePosition(point.position, out NavMeshHit hit, navMeshSnapDistance, NavMesh.AllAreas))
        {
            spawnPosition = hit.position;
        }

        EnemyNavMesh enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.Initialize(targetCar);

        aliveEnemies.Add(enemy);
    }

    private void CleanupAliveList()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }
}
