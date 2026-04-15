using UnityEditor;
using UnityEngine;

namespace SMW.Editor.Setup
{
    public static class PhysicsLayerMatrixSetup
    {
        // SMW layer names used for lookup. Matches TagManager.asset positions (8..19).
        public const string Player             = "Player";
        public const string PlayerInvulnerable = "PlayerInvulnerable";
        public const string PlayerProjectile   = "PlayerProjectile";
        public const string Enemy              = "Enemy";
        public const string EnemyProjectile    = "EnemyProjectile";
        public const string Solid              = "Solid";
        public const string OneWay             = "OneWay";
        public const string Hazard             = "Hazard";
        public const string Pickup             = "Pickup";
        public const string Interactive        = "Interactive";
        public const string LevelBounds        = "LevelBounds";
        public const string MapNode            = "MapNode";

        [MenuItem("Tools/SMW/Setup/Apply Physics2D Layer Matrix")]
        public static void Apply()
        {
            // Start from "everything collides," then disable per SPEC §4.19.
            ResetAllPairs(true);

            // Disabled pairs.
            Ignore(Player, PlayerProjectile);
            Ignore(Enemy, Enemy);
            Ignore(Pickup, Pickup);
            Ignore(Pickup, Solid);
            Ignore(PlayerInvulnerable, Enemy);

            // LevelBounds never collides with anything.
            IgnoreAllWith(LevelBounds);
            // MapNode only interacts with itself/UI clicks; disable physics collisions with world layers.
            IgnoreAllWith(MapNode);

            // Self-ignore housekeeping.
            Ignore(LevelBounds, LevelBounds);
            Ignore(MapNode, MapNode);

            AssetDatabase.SaveAssets();
            Debug.Log("[SMW Setup] Physics2D layer matrix applied per SPEC §4.19.");
        }

        private static void Ignore(string a, string b)
        {
            var la = LayerMask.NameToLayer(a);
            var lb = LayerMask.NameToLayer(b);
            if (la < 0 || lb < 0)
            {
                Debug.LogWarning($"[SMW Setup] Missing layer for '{a}' or '{b}'. Skipping.");
                return;
            }
            Physics2D.IgnoreLayerCollision(la, lb, true);
        }

        private static void IgnoreAllWith(string layer)
        {
            var l = LayerMask.NameToLayer(layer);
            if (l < 0) return;
            for (int i = 0; i < 32; i++)
                Physics2D.IgnoreLayerCollision(l, i, true);
        }

        private static void ResetAllPairs(bool collide)
        {
            for (int a = 0; a < 32; a++)
                for (int b = a; b < 32; b++)
                    Physics2D.IgnoreLayerCollision(a, b, !collide);
        }
    }
}
