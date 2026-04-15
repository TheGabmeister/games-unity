using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class SlopeGroundedContinuityTest
    {
        [UnityTest]
        public IEnumerator Walking_Up_A_45_Slope_Keeps_Grounded_True()
        {
            SceneBuildHelpers.SetupPhysicsTiming();
            try
            {
                // Flat entry ramp, then a 45° slope rising to the right, then a flat landing.
                SceneBuildHelpers.CreateGroundBox(new Vector2(-2f, -1f), new Vector2(4f, 1f));
                SceneBuildHelpers.CreateSlope(new Vector2(2f, 0f), SlopeKind.SteepR, 4);
                SceneBuildHelpers.CreateGroundBox(new Vector2(6f, 3f), new Vector2(8f, 1f));

                var player = SceneBuildHelpers.CreatePlayer(new Vector2(0f, 0.1f));
                var binding = player.GetComponent<PlayerInputBinding>();
                var controller = player.GetComponent<PlayerController>();

                // Let the player settle on ground.
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                Assert.IsTrue(controller.IsGrounded, "Pre-condition: player should be grounded before test starts.");

                int ungroundedTicks = 0;
                int ticksOnSlope = 0;

                for (int i = 0; i < 200; i++)
                {
                    binding.DebugOverride(
                        move: new Vector2(1f, 0f),
                        jumpHeld: false, jumpPressed: false, jumpReleased: false,
                        spinJumpHeld: false, spinJumpPressed: false,
                        actionHeld: true, actionPressed: false, actionReleased: false);
                    yield return new WaitForFixedUpdate();

                    float x = player.transform.position.x;
                    if (x > 2.2f && x < 5.8f) ticksOnSlope++;
                    if (x > 2.2f && x < 5.8f && !controller.IsGrounded) ungroundedTicks++;

                    if (x > 7f) break;
                }

                Assert.That(ticksOnSlope, Is.GreaterThan(10),
                    "Player should have traversed the slope for at least several ticks.");
                Assert.That(ungroundedTicks, Is.EqualTo(0),
                    $"Player ungrounded for {ungroundedTicks} ticks while traversing a 45° slope.");
            }
            finally
            {
                SceneBuildHelpers.DestroyAllRoots();
                SceneBuildHelpers.RestorePhysicsTiming();
            }
        }
    }
}
