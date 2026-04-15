using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class VariableJumpHeightTest
    {
        [UnityTest]
        public IEnumerator Short_Tap_Reaches_Lower_Peak_Than_Full_Hold()
        {
            SceneBuildHelpers.SetupPhysicsTiming();
            try
            {
                float tapPeak = 0f;
                var r1 = RunTrial(holdTicks: 1, peak => tapPeak = peak);
                while (r1.MoveNext()) yield return r1.Current;

                SceneBuildHelpers.DestroyAllRoots();
                yield return null;

                float holdPeak = 0f;
                var r2 = RunTrial(holdTicks: 30, peak => holdPeak = peak);
                while (r2.MoveNext()) yield return r2.Current;

                Assert.That(holdPeak, Is.GreaterThan(tapPeak + 0.3f),
                    $"Full-hold jump peak ({holdPeak}) should clearly exceed short-tap peak ({tapPeak}).");
                Assert.That(tapPeak, Is.GreaterThan(0.5f), "Short tap should still clear at least ~0.5u.");
                Assert.That(holdPeak, Is.LessThan(8f), "Full-hold jump should not exceed ~8u.");
            }
            finally
            {
                SceneBuildHelpers.DestroyAllRoots();
                SceneBuildHelpers.RestorePhysicsTiming();
            }
        }

        private IEnumerator RunTrial(int holdTicks, System.Action<float> reportPeak)
        {
            SceneBuildHelpers.CreateGroundBox(new Vector2(-5f, -1f), new Vector2(10f, 1f));
            var player = SceneBuildHelpers.CreatePlayer(new Vector2(0f, 0.1f));
            var binding = player.GetComponent<PlayerInputBinding>();
            var controller = player.GetComponent<PlayerController>();

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            float startY = player.transform.position.y;
            float peakY = startY;

            for (int i = 0; i < 80; i++)
            {
                bool pressed = i == 0;
                bool held = i < holdTicks;
                bool released = i == holdTicks;

                binding.DebugOverride(
                    move: Vector2.zero,
                    jumpHeld: held,
                    jumpPressed: pressed,
                    jumpReleased: released,
                    spinJumpHeld: false, spinJumpPressed: false,
                    actionHeld: false, actionPressed: false, actionReleased: false);
                yield return new WaitForFixedUpdate();

                peakY = Mathf.Max(peakY, player.transform.position.y);

                if (i > holdTicks + 5 && controller.IsGrounded) break;
            }

            reportPeak(peakY - startY);
        }
    }
}
