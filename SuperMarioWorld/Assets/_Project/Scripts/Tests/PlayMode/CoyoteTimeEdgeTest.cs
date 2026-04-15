using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class CoyoteTimeEdgeTest
    {
        [UnityTest]
        public IEnumerator Jump_At_6_Frames_Past_Ledge_Succeeds_And_7_Fails()
        {
            SceneBuildHelpers.SetupPhysicsTiming();
            try
            {
                bool succeededAt6 = false;
                bool succeededAt7 = false;

                var r1 = RunTrial(delayTicks: 6, fired =>  succeededAt6 = fired);
                while (r1.MoveNext()) yield return r1.Current;

                SceneBuildHelpers.DestroyAllRoots();
                yield return null;

                var r2 = RunTrial(delayTicks: 7, fired => succeededAt7 = fired);
                while (r2.MoveNext()) yield return r2.Current;

                Assert.IsTrue(succeededAt6, "Jump at 6 frames post-ledge should succeed.");
                Assert.IsFalse(succeededAt7, "Jump at 7 frames post-ledge should fail.");
            }
            finally
            {
                SceneBuildHelpers.DestroyAllRoots();
                SceneBuildHelpers.RestorePhysicsTiming();
            }
        }

        // Walks off a short platform. After IsGrounded goes false for the first time,
        // waits `delayTicks` fixed updates and attempts a jump. Reports whether
        // velocity.y became positive (jump fired).
        private IEnumerator RunTrial(int delayTicks, System.Action<bool> report)
        {
            SceneBuildHelpers.CreateGroundBox(new Vector2(-5f, -1f), new Vector2(10f, 1f));
            var player = SceneBuildHelpers.CreatePlayer(new Vector2(3f, 0.1f));
            var binding = player.GetComponent<PlayerInputBinding>();
            var controller = player.GetComponent<PlayerController>();

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Walk right until we fall off.
            bool fallStarted = false;
            int fallTick = -1;
            bool jumpPulseFired = false;
            int testTick = 0;
            const int timeout = 120;
            bool fired = false;

            while (testTick < timeout)
            {
                bool sentJump = false;
                if (fallStarted && testTick - fallTick == delayTicks && !jumpPulseFired)
                {
                    sentJump = true;
                    jumpPulseFired = true;
                }

                binding.DebugOverride(
                    move: new Vector2(1f, 0f),
                    jumpHeld: false,
                    jumpPressed: sentJump,
                    jumpReleased: false,
                    spinJumpHeld: false, spinJumpPressed: false,
                    actionHeld: false, actionPressed: false, actionReleased: false);

                yield return new WaitForFixedUpdate();

                if (!fallStarted && !controller.IsGrounded)
                {
                    fallStarted = true;
                    fallTick = testTick;
                }

                // One tick after we attempted the jump, check if velocity went up.
                if (jumpPulseFired && testTick == fallTick + delayTicks + 1)
                {
                    fired = controller.Velocity.y > 1f;
                    break;
                }

                testTick++;
            }

            report(fired);
        }
    }
}
