using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SMW
{
    // Shared PlayMode test helper. Builds a minimal Player + ground scene in memory
    // (no disk artifact) and provides deterministic stepping + input-injection hooks.
    //
    // Physics runs at a fixed 60Hz during tests so frame-counting assertions are
    // stable regardless of the editor's project settings.
    public static class SceneBuildHelpers
    {
        public const int SolidLayer = 13;
        public const int PlayerLayer = 8;
        public const float FixedStep = 1f / 60f;

        private static float _savedFixedDelta;
        private static float _savedMaxDelta;

        public static void SetupPhysicsTiming()
        {
            _savedFixedDelta = Time.fixedDeltaTime;
            _savedMaxDelta = Time.maximumDeltaTime;
            Time.fixedDeltaTime = FixedStep;
            Time.maximumDeltaTime = FixedStep * 4f;
        }

        public static void RestorePhysicsTiming()
        {
            if (_savedFixedDelta > 0f) Time.fixedDeltaTime = _savedFixedDelta;
            if (_savedMaxDelta > 0f) Time.maximumDeltaTime = _savedMaxDelta;
        }

        public static GameObject CreatePlayer(Vector2 position, string name = "Player")
        {
            var go = new GameObject(name);
            go.layer = PlayerLayer;
            go.transform.position = position;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.None;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 1.9f);
            box.offset = new Vector2(0f, 0.95f);

            go.AddComponent<GroundProbe>();
            go.AddComponent<PlayerCarry>();
            go.AddComponent<PlayerInputBinding>();
            go.AddComponent<PlayerController>();
            return go;
        }

        public static GameObject CreateGroundBox(Vector2 bottomLeft, Vector2 size, string name = "Ground")
        {
            var go = new GameObject(name);
            go.layer = SolidLayer;
            go.transform.position = bottomLeft;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.offset = size * 0.5f;
            return go;
        }

        public static GameObject CreateCeilingBox(Vector2 topLeft, Vector2 size, string name = "Ceiling")
        {
            var go = new GameObject(name);
            go.layer = SolidLayer;
            go.transform.position = topLeft;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.offset = new Vector2(size.x * 0.5f, -size.y * 0.5f);
            return go;
        }

        public static GameObject CreateSlope(Vector2 bottomLeft, SlopeKind kind, int length)
        {
            var go = new GameObject($"Slope_{kind}_{length}");
            go.layer = SolidLayer;
            go.transform.position = bottomLeft;
            go.AddComponent<PolygonCollider2D>();
            var slope = go.AddComponent<Slope>();
            slope.Configure(kind, length);
            return go;
        }

        // Advances the PlayMode runtime by `ticks` fixed-updates. Between each tick,
        // invokes `beforeTick(i)` so the test can set / reset intent flags on the
        // binding just before the physics step reads them.
        public static IEnumerator Step(int ticks, System.Action<int> beforeTick = null)
        {
            for (int i = 0; i < ticks; i++)
            {
                beforeTick?.Invoke(i);
                yield return new WaitForFixedUpdate();
            }
        }

        // Cleanup: destroys every root GameObject in the active scene. Call from [TearDown].
        public static void DestroyAllRoots()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                Object.Destroy(root);
            }
        }
    }
}
