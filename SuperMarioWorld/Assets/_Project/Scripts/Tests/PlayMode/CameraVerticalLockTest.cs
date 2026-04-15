using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class CameraVerticalLockTest
    {
        [UnityTest]
        public IEnumerator Isolated_Jump_Does_Not_Move_Camera_Y_But_Grounded_Climb_Does()
        {
            SceneBuildHelpers.SetupPhysicsTiming();
            try
            {
                // Ground at y=0..1, stair top at y=3..4 accessed by a step at y=2.
                SceneBuildHelpers.CreateGroundBox(new Vector2(-5f, -1f), new Vector2(20f, 1f));
                SceneBuildHelpers.CreateGroundBox(new Vector2(4f, 1f), new Vector2(3f, 1f), "Step1");
                SceneBuildHelpers.CreateGroundBox(new Vector2(7f, 3f), new Vector2(6f, 1f), "Step2");

                var player = SceneBuildHelpers.CreatePlayer(new Vector2(0f, 0.1f));
                var binding = player.GetComponent<PlayerInputBinding>();

                var camGo = new GameObject("TestCamera", typeof(Camera), typeof(LevelCamera));
                var cam = camGo.GetComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 7f;
                var levelCam = camGo.GetComponent<LevelCamera>();
                levelCam.SetTarget(player.transform);
                camGo.transform.position = new Vector3(0f, 0.1f, -10f);

                // Part 1: jump straight up. Camera Y should be essentially unchanged.
                float initialCamY = camGo.transform.position.y;
                for (int i = 0; i < 30; i++)
                {
                    bool pressed = i == 0;
                    bool held = i < 15;
                    binding.DebugOverride(
                        move: Vector2.zero,
                        jumpHeld: held, jumpPressed: pressed, jumpReleased: !held && i == 15,
                        spinJumpHeld: false, spinJumpPressed: false,
                        actionHeld: false, actionPressed: false, actionReleased: false);
                    yield return new WaitForFixedUpdate();
                    levelCam.StepOnce(SceneBuildHelpers.FixedStep);
                }
                float afterJumpCamY = camGo.transform.position.y;
                Assert.That(Mathf.Abs(afterJumpCamY - initialCamY), Is.LessThan(0.3f),
                    $"Camera Y drifted during an isolated jump: {initialCamY} → {afterJumpCamY}.");

                // Let the player settle, then climb the stairs (right + jump cycle).
                for (int i = 0; i < 30; i++)
                {
                    binding.DebugOverride(
                        move: Vector2.zero,
                        jumpHeld: false, jumpPressed: false, jumpReleased: false,
                        spinJumpHeld: false, spinJumpPressed: false,
                        actionHeld: false, actionPressed: false, actionReleased: false);
                    yield return new WaitForFixedUpdate();
                    levelCam.StepOnce(SceneBuildHelpers.FixedStep);
                }

                float baselineCamY = camGo.transform.position.y;

                // Walk right + jump up two steps. Each up-step uses a timed jump.
                for (int i = 0; i < 240; i++)
                {
                    bool pressed = (i == 30 || i == 120);
                    bool held = (i >= 30 && i < 45) || (i >= 120 && i < 135);
                    binding.DebugOverride(
                        move: new Vector2(1f, 0f),
                        jumpHeld: held, jumpPressed: pressed, jumpReleased: false,
                        spinJumpHeld: false, spinJumpPressed: false,
                        actionHeld: false, actionPressed: false, actionReleased: false);
                    yield return new WaitForFixedUpdate();
                    levelCam.StepOnce(SceneBuildHelpers.FixedStep);
                }

                float finalCamY = camGo.transform.position.y;
                Assert.That(finalCamY - baselineCamY, Is.GreaterThan(0.5f),
                    $"Camera Y should climb when the player stair-steps upward. baseline={baselineCamY}, final={finalCamY}.");
            }
            finally
            {
                SceneBuildHelpers.DestroyAllRoots();
                SceneBuildHelpers.RestorePhysicsTiming();
            }
        }
    }
}
