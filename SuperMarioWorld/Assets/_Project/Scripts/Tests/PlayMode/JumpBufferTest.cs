using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class JumpBufferTest
    {
        [UnityTest]
        public IEnumerator Jump_Buffered_Within_6_Frames_Of_Landing_Fires_On_Landing()
        {
            SceneBuildHelpers.SetupPhysicsTiming();
            try
            {
                bool firedAt6 = false;
                bool firedAt7 = false;

                var r1 = RunTrial(preLandingFrames: 6, fired => firedAt6 = fired);
                while (r1.MoveNext()) yield return r1.Current;

                SceneBuildHelpers.DestroyAllRoots();
                yield return null;

                var r2 = RunTrial(preLandingFrames: 7, fired => firedAt7 = fired);
                while (r2.MoveNext()) yield return r2.Current;

                Assert.IsTrue(firedAt6, "Jump pressed 6 frames before landing should fire on landing frame.");
                Assert.IsFalse(firedAt7, "Jump pressed 7 frames before landing should not fire.");
            }
            finally
            {
                SceneBuildHelpers.DestroyAllRoots();
                SceneBuildHelpers.RestorePhysicsTiming();
            }
        }

        // Drops the player from a known height, watches for the landing tick, presses
        // jump `preLandingFrames` ticks before landing, and reports whether the jump
        // fired on (or within one tick of) the landing frame.
        private IEnumerator RunTrial(int preLandingFrames, System.Action<bool> report)
        {
            SceneBuildHelpers.CreateGroundBox(new Vector2(-5f, -1f), new Vector2(10f, 1f));
            var player = SceneBuildHelpers.CreatePlayer(new Vector2(0f, 6f));
            var binding = player.GetComponent<PlayerInputBinding>();
            var controller = player.GetComponent<PlayerController>();

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // First pass: let the player fall freely, watch for the predicted landing.
            // We need to know which tick is "landing - preLandingFrames" to trigger the
            // press. Simpler: poll each tick — when we're close to landing (velocity.y
            // negative AND remaining distance small), start a countdown.
            int landingTick = EstimateTicksToLand(player.transform.position.y, 0f,
                Mathf.Abs(controller.Velocity.y), out _);

            int pressTick = landingTick - preLandingFrames;
            int testTick = 0;
            bool pressed = false;
            bool fired = false;

            while (testTick < landingTick + 8 && testTick < 200)
            {
                bool sendPress = testTick == pressTick && !pressed;
                if (sendPress) pressed = true;

                binding.DebugOverride(
                    move: Vector2.zero,
                    jumpHeld: false,
                    jumpPressed: sendPress,
                    jumpReleased: false,
                    spinJumpHeld: false, spinJumpPressed: false,
                    actionHeld: false, actionPressed: false, actionReleased: false);
                yield return new WaitForFixedUpdate();

                if (controller.IsGrounded && pressed)
                {
                    // Give one extra tick for the buffered jump to launch.
                    yield return new WaitForFixedUpdate();
                    fired = controller.Velocity.y > 1f;
                    break;
                }

                testTick++;
            }

            report(fired);
        }

        // Ground at top y=0 (box from y=-1 to y=0). Player collider bottom is at y=0
        // when transform.y = 0. Starting at y=y0 with v=0, time to fall distance y0
        // under gravity g: y0 = 0.5 g t^2 → t = sqrt(2 y0 / g). Ticks = t / dt.
        private static int EstimateTicksToLand(float y0, float targetY, float _, out float landTimeSeconds)
        {
            float dist = y0 - targetY;
            const float g = 60f;
            landTimeSeconds = Mathf.Sqrt(2f * dist / g);
            return Mathf.FloorToInt(landTimeSeconds / SceneBuildHelpers.FixedStep);
        }
    }
}
