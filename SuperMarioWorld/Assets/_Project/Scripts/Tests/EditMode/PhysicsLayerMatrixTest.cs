using NUnit.Framework;
using UnityEngine;

namespace SMW
{
    public sealed class PhysicsLayerMatrixTest
    {
        [OneTimeSetUp]
        public void Apply()
        {
            // Ensure test runs regardless of earlier state: re-apply the matrix.
            PhysicsLayerMatrixSetup.Apply();
        }

        private static bool Collides(string a, string b)
        {
            var la = LayerMask.NameToLayer(a);
            var lb = LayerMask.NameToLayer(b);
            if (la < 0 || lb < 0) Assert.Fail($"Missing layer {a} or {b}");
            return !Physics2D.GetIgnoreLayerCollision(la, lb);
        }

        [Test] public void Player_Collides_Enemy()  { Assert.IsTrue(Collides("Player", "Enemy")); }
        [Test] public void Player_Collides_Solid()  { Assert.IsTrue(Collides("Player", "Solid")); }
        [Test] public void Player_Ignores_PlayerProjectile() { Assert.IsFalse(Collides("Player", "PlayerProjectile")); }
        [Test] public void Enemy_Ignores_Enemy()    { Assert.IsFalse(Collides("Enemy", "Enemy")); }
        [Test] public void Pickup_Ignores_Pickup()  { Assert.IsFalse(Collides("Pickup", "Pickup")); }
        [Test] public void Pickup_Ignores_Solid()   { Assert.IsFalse(Collides("Pickup", "Solid")); }
        [Test] public void PlayerInvulnerable_Ignores_Enemy() { Assert.IsFalse(Collides("PlayerInvulnerable", "Enemy")); }
        [Test] public void LevelBounds_Ignores_Player() { Assert.IsFalse(Collides("LevelBounds", "Player")); }
        [Test] public void LevelBounds_Ignores_Enemy()  { Assert.IsFalse(Collides("LevelBounds", "Enemy")); }
    }
}
