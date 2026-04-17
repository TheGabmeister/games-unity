using NUnit.Framework;
using UnityEngine;

public class CheckpointAndServicesTests
{
    [Test]
    public void CheckpointService_RegistersWithLocator_AndResolvesCheckpointAfterPendingRespawn()
    {
        var servicesRoot = new GameObject("ServicesRoot");
        servicesRoot.SetActive(false);
        servicesRoot.AddComponent<Services>();
        servicesRoot.AddComponent<CheckpointService>();

        try
        {
            servicesRoot.SetActive(true);

            Assert.That(Services.TryGet<ICheckpointService>(out var checkpointService), Is.True);
            Assert.That(Services.Instance.Get<CheckpointService>(), Is.SameAs(servicesRoot.GetComponent<CheckpointService>()));

            checkpointService.EnterScene(new Vector3(1f, 2f, 0f));
            checkpointService.ActivateCheckpoint(new Vector3(8f, 2f, 0f));
            checkpointService.MarkPendingRespawn();
            checkpointService.EnterScene(new Vector3(1f, 2f, 0f));

            Assert.That(checkpointService.TryGetRespawnPosition(out var respawnPosition), Is.True);
            Assert.That(respawnPosition, Is.EqualTo(new Vector3(8f, 2f, 0f)));
        }
        finally
        {
            Object.DestroyImmediate(servicesRoot);
        }
    }

    [Test]
    public void CheckpointService_FallsBackToDefaultSpawn_WhenCheckpointIsStale()
    {
        var servicesRoot = new GameObject("ServicesRoot");
        servicesRoot.SetActive(false);
        servicesRoot.AddComponent<Services>();
        servicesRoot.AddComponent<CheckpointService>();

        try
        {
            servicesRoot.SetActive(true);
            Assert.That(Services.TryGet<ICheckpointService>(out var checkpointService), Is.True);

            Vector3 defaultSpawn = new(2f, 3f, 0f);
            checkpointService.EnterScene(defaultSpawn);
            checkpointService.ActivateCheckpoint(new Vector3(9f, 9f, 0f));
            checkpointService.MarkPendingRespawn();
            checkpointService.EnterScene(defaultSpawn);

            Assert.That(checkpointService.TryGetRespawnPosition(out var respawnPosition), Is.True);
            Assert.That(respawnPosition, Is.EqualTo(new Vector3(9f, 9f, 0f)));
        }
        finally
        {
            Object.DestroyImmediate(servicesRoot);
        }
    }

    [Test]
    public void CheckpointService_ClearsCheckpoint_OnNormalSceneEntry()
    {
        var servicesRoot = new GameObject("ServicesRoot");
        servicesRoot.SetActive(false);
        servicesRoot.AddComponent<Services>();
        servicesRoot.AddComponent<CheckpointService>();

        try
        {
            servicesRoot.SetActive(true);
            Assert.That(Services.TryGet<ICheckpointService>(out var checkpointService), Is.True);

            Vector3 defaultSpawn = new(2f, 3f, 0f);
            checkpointService.EnterScene(defaultSpawn);
            checkpointService.ActivateCheckpoint(new Vector3(9f, 9f, 0f));
            checkpointService.EnterScene(defaultSpawn);

            Assert.That(checkpointService.TryGetRespawnPosition(out var respawnPosition), Is.True);
            Assert.That(respawnPosition, Is.EqualTo(defaultSpawn));
        }
        finally
        {
            Object.DestroyImmediate(servicesRoot);
        }
    }

    [Test]
    public void HealthKill_IgnoresInvulnerability_AndDepletesImmediately()
    {
        var go = new GameObject("Health");
        var health = go.AddComponent<Health>();

        try
        {
            int depletedCount = 0;
            health.Depleted += () => depletedCount++;

            health.ApplyDamage(1, Vector2.zero);
            Assert.That(health.IsInvulnerable, Is.True);

            health.Kill(new Vector2(4f, 5f));

            Assert.That(health.CurrentHealth, Is.Zero);
            Assert.That(health.IsDepleted, Is.True);
            Assert.That(depletedCount, Is.EqualTo(1));
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}
