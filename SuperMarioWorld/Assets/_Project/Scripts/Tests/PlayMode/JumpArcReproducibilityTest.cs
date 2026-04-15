using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SMW
{
    // Canary test: physics should be deterministic at 60Hz. Running the same input
    // sequence from the same start state twice must land the player within ±0.01u
    // of itself. Regressions here flag a non-determinism bug — variable frame rate
    // creeping in, uninitialized state, or stochastic physics.
    public sealed class JumpArcReproducibilityTest
    {
        [UnityTest]
        public IEnumerator Two_Identical_Runs_Land_At_The_Same_Position()
        {
            SceneBuildHelpers.SetupPhysicsTiming();
            try
            {
                var first = yield_RunTrial();
                while (first.MoveNext()) yield return first.Current;
                Vector2 a = _endPosition;

                SceneBuildHelpers.DestroyAllRoots();
                yield return null;

                var second = yield_RunTrial();
                while (second.MoveNext()) yield return second.Current;
                Vector2 b = _endPosition;

                Assert.That(Vector2.Distance(a, b), Is.LessThan(0.01f),
                    $"Non-deterministic physics: run1 ended at {a}, run2 at {b}.");
            }
            finally
            {
                SceneBuildHelpers.DestroyAllRoots();
                SceneBuildHelpers.RestorePhysicsTiming();
            }
        }

        private Vector2 _endPosition;

        private IEnumerator yield_RunTrial()
        {
            SceneBuildHelpers.CreateGroundBox(new Vector2(-10f, -1f), new Vector2(60f, 1f));
            var player = SceneBuildHelpers.CreatePlayer(new Vector2(0f, 0.1f));
            var binding = player.GetComponent<PlayerInputBinding>();
            var controller = player.GetComponent<PlayerController>();

            // Wait a couple of ticks for ground contact + probe settling.
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Scripted sequence: run right 30 ticks, jump, hold 10 ticks, release, run 30 more.
            for (int i = 0; i < 70; i++)
            {
                bool jumpPressed = i == 30;
                bool jumpHeld = i >= 30 && i < 40;
                bool jumpReleased = i == 40;
                binding.DebugOverride(
                    move: new Vector2(1f, 0f),
                    jumpHeld: jumpHeld,
                    jumpPressed: jumpPressed,
                    jumpReleased: jumpReleased,
                    spinJumpHeld: false, spinJumpPressed: false,
                    actionHeld: true, actionPressed: false, actionReleased: false);
                yield return new WaitForFixedUpdate();
            }

            _endPosition = player.transform.position;
            _ = controller; // reference to keep the analyzer happy
        }
    }
}
