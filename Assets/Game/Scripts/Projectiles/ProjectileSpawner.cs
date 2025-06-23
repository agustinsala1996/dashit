using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Spawns projectiles with a delay.
/// </summary>
public class ProjectileSpawner : Startable
{
    public GameObject projectileSpawnerPrefab;
    public Vector2 minMaxTimeBetweenSpawns = new Vector2(2f, 10f);
    public Vector2 minMaxTimeInitialDelay = Vector2.zero;

    [Range(0f, 1f)]
    public float chanceToSpawn = 0.5f;

    public override void OnStart()
    {
        StartCoroutine(SpawnShapes());
    }

    private IEnumerator SpawnShapes()
    {
        if (minMaxTimeInitialDelay != Vector2.zero)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(minMaxTimeBetweenSpawns.x, minMaxTimeBetweenSpawns.y));
        }

        while (true)
        {
            float nextSpawnTime = GetNextSpawnTime();
            yield return new WaitForSeconds(nextSpawnTime);

            if (UnityEngine.Random.value > chanceToSpawn)
                continue;

            try
            {
                SpawnPoint();
            }
            catch (UnityException)
            {
                break;
            }
        }
    }

    private float GetNextSpawnTime()
    {
        return UnityEngine.Random.Range(minMaxTimeBetweenSpawns.x, minMaxTimeBetweenSpawns.y);
    }

    private void SpawnPoint()
    {
        if (ObjectPool.instance == null)
            throw new UnityException("Object pool not found");

        var projObj = ObjectPool.instance.GetObjectForType(projectileSpawnerPrefab.name, false);
        var renderer = projObj.GetComponentInChildren<SpriteRenderer>();

        projObj.SetActive(true);

        Vector2 spawnLocation = PointWithinBounds.GetRandomWithinBounds(
            renderer.sprite.bounds.size.x * renderer.gameObject.transform.localScale.x,
            renderer.sprite.bounds.size.y * renderer.gameObject.transform.localScale.y
        );

        projObj.transform.position = spawnLocation;

        // ✅ Reproducir SFX al disparar proyectil
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayEnemyShoot();
    }
}