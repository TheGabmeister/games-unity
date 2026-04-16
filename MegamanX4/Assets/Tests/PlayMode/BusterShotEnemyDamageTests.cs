using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BusterShotEnemyDamageTests
{
    const float ShotSpawnX = -0.6f;

    [UnityTest]
    public IEnumerator BusterShot_AppliesDamage_And_DestroysEnemyAtZero()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Assert.That(enemyLayer, Is.Not.EqualTo(-1), "Expected Enemy layer to exist.");

        GameObject enemy = null;
        BusterShot firstShot = null;
        BusterShot secondShot = null;

        try
        {
            enemy = CreateEnemy(enemyLayer, maxHealth: 2);
            var enemyHealth = enemy.GetComponent<Health>();

            firstShot = CreateShot(damage: 1);
            yield return WaitForShotToResolve(firstShot);

            Assert.That(enemyHealth.CurrentHealth, Is.EqualTo(1));
            Assert.That(enemy, Is.Not.Null);

            secondShot = CreateShot(damage: 1);
            yield return WaitForShotToResolve(secondShot);
            yield return null;

            Assert.That(enemy == null, Is.True, "Expected enemy to be destroyed at zero health.");
        }
        finally
        {
            if (firstShot)
                Object.DestroyImmediate(firstShot.gameObject);

            if (secondShot)
                Object.DestroyImmediate(secondShot.gameObject);

            if (enemy)
                Object.DestroyImmediate(enemy);
        }
    }

    static GameObject CreateEnemy(int enemyLayer, int maxHealth)
    {
        var enemy = new GameObject("TestEnemy");
        enemy.SetActive(false);
        enemy.layer = enemyLayer;
        enemy.transform.position = Vector3.zero;

        var collider = enemy.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.8f, 1f);

        var health = enemy.AddComponent<Health>();
        SetPrivateField(health, "_maxHealth", maxHealth);
        enemy.AddComponent<Enemy>();

        enemy.SetActive(true);
        return enemy;
    }

    static BusterShot CreateShot(int damage)
    {
        var shotObject = new GameObject("TestBusterShot");
        shotObject.SetActive(false);
        shotObject.transform.position = new Vector3(ShotSpawnX, 0f, 0f);

        var collider = shotObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.08f;

        var body = shotObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.useFullKinematicContacts = true;

        var shot = shotObject.AddComponent<BusterShot>();
        SetPrivateField(shot, "_speed", 8f);
        SetPrivateField(shot, "_lifetime", 1f);
        SetPrivateField(shot, "_damage", damage);
        SetPrivateField(shot, "_hitLayers", LayerMask.GetMask("Enemy"));

        shotObject.SetActive(true);
        shot.Fire();
        return shot;
    }

    static IEnumerator WaitForShotToResolve(BusterShot shot)
    {
        float timeout = Time.time + 1f;
        while (shot && Time.time < timeout)
            yield return null;

        Assert.That(shot == null, Is.True, "Expected shot to collide and destroy itself.");
    }

    static void SetPrivateField<T>(object target, string fieldName, T value)
    {
        var field = FindField(target.GetType(), fieldName);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}' on {target.GetType().Name}.");
        field.SetValue(target, value);
    }

    static FieldInfo FindField(System.Type type, string fieldName)
    {
        while (type != null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
                return field;

            type = type.BaseType;
        }

        return null;
    }
}
