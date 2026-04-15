using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class CeilingCancelTest
    {
        [UnityTest]
        public IEnumerator Vertical_Velocity_Zeros_On_Ceiling_Contact_Without_Rebound()
        {
            SceneBuildHelpers.SetupPhysicsTiming();
            try
            {
                SceneBuildHelpers.CreateGroundBox(new Vector2(-5f, -1f), new Vector2(10f, 1f));
                // Ceiling at y=4, extends up to y=5. Player (2u tall) spawn at y=0;
                // a full-hold jump peaks well above 4 so we'll contact the ceiling.
                SceneBuildHelpers.CreateCeilingBox(new Vector2(-5f, 5f), new Vector2(10f, 1f), "Ceiling");

                var player = SceneBuildHelpers.CreatePlayer(new Vector2(0f, 0.1f));
                var binding = player.GetComponent<PlayerInputBinding>();
                var controller = player.GetComponent<PlayerController>();

                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                // Full-hold jump.
                for (int i = 0; i < 80; i++)
                {
                    bool pressed = i == 0;
                    binding.DebugOverride(
                        move: Vector2.zero,
                        jumpHeld: true,
                        jumpPressed: pressed,
                        jumpReleased: false,
                        spinJumpHeld: false, spinJumpPressed: false,
                        actionHeld: false, actionPressed: false, actionReleased: false);
                    yield return new WaitForFixedUpdate();

                    var probe = player.GetComponent<GroundProbe>();
                    if (probe.CeilingContact)
                    {
                        Assert.That(controller.Velocity.y, Is.LessThanOrEqualTo(0.01f),
                            $"Ceiling contact should zero (or invert) upward velocity. Got {controller.Velocity.y}.");
                        yield break;
                    }
                }

                Assert.Fail("Player never reached the ceiling in 80 ticks.");
            }
            finally
            {
                SceneBuildHelpers.DestroyAllRoots();
                SceneBuildHelpers.RestorePhysicsTiming();
            }
        }
    }
}
