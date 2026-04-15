using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class CameraForwardBiasTest
    {
        [UnityTest]
        public IEnumerator Sprinting_Right_Leads_Player_By_Configured_Bias()
        {
            SceneBuildHelpers.SetupPhysicsTiming();
            try
            {
                SceneBuildHelpers.CreateGroundBox(new Vector2(-5f, -1f), new Vector2(100f, 1f));
                var player = SceneBuildHelpers.CreatePlayer(new Vector2(0f, 0.1f));
                var binding = player.GetComponent<PlayerInputBinding>();

                var camGo = new GameObject("TestCamera", typeof(Camera), typeof(LevelCamera));
                var cam = camGo.GetComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 7f;
                var levelCam = camGo.GetComponent<LevelCamera>();
                levelCam.SetTarget(player.transform);
                camGo.transform.position = new Vector3(0f, 0.1f, -10f);

                // Sprint right for 2 seconds. Each fixed tick, drive binding + step camera.
                for (int i = 0; i < 120; i++)
                {
                    binding.DebugOverride(
                        move: new Vector2(1f, 0f),
                        jumpHeld: false, jumpPressed: false, jumpReleased: false,
                        spinJumpHeld: false, spinJumpPressed: false,
                        actionHeld: true, actionPressed: false, actionReleased: false);
                    yield return new WaitForFixedUpdate();
                    levelCam.StepOnce(SceneBuildHelpers.FixedStep);
                }

                float playerX = player.transform.position.x;
                float camX = camGo.transform.position.x;
                Assert.That(camX - playerX, Is.GreaterThan(1.0f),
                    $"Camera should lead a sprinting-right player. playerX={playerX}, camX={camX}.");
            }
            finally
            {
                SceneBuildHelpers.DestroyAllRoots();
                SceneBuildHelpers.RestorePhysicsTiming();
            }
        }
    }
}
