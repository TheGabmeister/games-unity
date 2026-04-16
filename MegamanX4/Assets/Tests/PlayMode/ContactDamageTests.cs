using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ContactDamageTests
{
    [UnityTest]
    public IEnumerator TriggerContact_DamagesPlayerOnceUntilReenter()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        Assert.That(playerLayer, Is.Not.EqualTo(-1), "Expected Player layer to exist.");

        GameObject player = null;
        GameObject damager = null;

        try
        {
            player = CreatePlayer(playerLayer, startPosition: new Vector2(-2f, 0f));
            damager = CreateDamager(trigger: true, position: Vector2.zero);

            var health = player.GetComponent<Health>();
            var body = player.GetComponent<Rigidbody2D>();

            yield return MovePlayer(body, new Vector2(2f, 0f), frames: 30);
            Assert.That(health.CurrentHealth, Is.EqualTo(2));

            yield return null;
            yield return null;
            Assert.That(health.CurrentHealth, Is.EqualTo(2), "Expected no repeated damage while overlap persists.");

            yield return MovePlayer(body, new Vector2(-2f, 0f), frames: 30);
            yield return MovePlayer(body, new Vector2(2f, 0f), frames: 30);
            Assert.That(health.CurrentHealth, Is.EqualTo(1), "Expected re-entering contact to apply damage again.");
        }
        finally
        {
            if (damager)
                Object.DestroyImmediate(damager);

            if (player)
                Object.DestroyImmediate(player);
        }
    }

    [UnityTest]
    public IEnumerator CollisionContact_DamagesPlayerOnEnter()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        Assert.That(playerLayer, Is.Not.EqualTo(-1), "Expected Player layer to exist.");

        GameObject player = null;
        GameObject damager = null;

        try
        {
            player = CreatePlayer(playerLayer, startPosition: new Vector2(-2f, 0f));
            damager = CreateDamager(trigger: false, position: Vector2.zero);

            var health = player.GetComponent<Health>();
            var body = player.GetComponent<Rigidbody2D>();

            yield return MovePlayer(body, new Vector2(2f, 0f), frames: 30);
            Assert.That(health.CurrentHealth, Is.EqualTo(2));
        }
        finally
        {
            if (damager)
                Object.DestroyImmediate(damager);

            if (player)
                Object.DestroyImmediate(player);
        }
    }

    static GameObject CreatePlayer(int playerLayer, Vector2 startPosition)
    {
        var player = new GameObject("TestPlayer");
        player.SetActive(false);
        player.layer = playerLayer;
        player.transform.position = startPosition;

        var body = player.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;

        var collider = player.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 1.4f);

        var health = player.AddComponent<Health>();
        SetPrivateField(health, "_maxHealth", 3);

        player.SetActive(true);
        return player;
    }

    static GameObject CreateDamager(bool trigger, Vector2 position)
    {
        var damager = new GameObject(trigger ? "TriggerDamager" : "CollisionDamager");
        damager.transform.position = position;

        var collider = damager.AddComponent<BoxCollider2D>();
        collider.isTrigger = trigger;
        collider.size = new Vector2(1f, 1f);

        var contactDamage = damager.AddComponent<ContactDamage>();
        SetPrivateField(contactDamage, "_damageAmount", 1);
        return damager;
    }

    static IEnumerator MovePlayer(Rigidbody2D body, Vector2 destination, int frames)
    {
        Vector2 start = body.position;
        for (int i = 1; i <= frames; i++)
        {
            float t = i / (float)frames;
            body.MovePosition(Vector2.Lerp(start, destination, t));
            yield return new WaitForFixedUpdate();
        }

        body.linearVelocity = Vector2.zero;
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
