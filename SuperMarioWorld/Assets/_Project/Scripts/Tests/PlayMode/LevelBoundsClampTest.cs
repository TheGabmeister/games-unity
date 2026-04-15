using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class LevelBoundsClampTest
    {
        [UnityTest]
        public IEnumerator Camera_Clamps_Within_Bounds_Rect_When_Target_Leaves_The_Rect()
        {
            SceneBuildHelpers.SetupPhysicsTiming();
            try
            {
                var boundsGo = new GameObject("Bounds", typeof(BoxCollider2D), typeof(LevelBounds));
                boundsGo.transform.position = new Vector3(10f, 5f, 0f);
                var boundsBox = boundsGo.GetComponent<BoxCollider2D>();
                boundsBox.isTrigger = true;
                boundsBox.size = new Vector2(40f, 14f);
                var bounds = boundsGo.GetComponent<LevelBounds>();

                var target = new GameObject("Target");
                target.transform.position = new Vector3(100f, 100f, 0f); // far outside bounds

                var camGo = new GameObject("TestCamera", typeof(Camera), typeof(LevelCamera));
                var cam = camGo.GetComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5f; // half-height 5 → camera min Y inside bounds = rect.yMin + 5
                // Force a 16:9 aspect so the clamp has deterministic halfWidth.
                cam.aspect = 16f / 9f;
                var levelCam = camGo.GetComponent<LevelCamera>();
                levelCam.SetBounds(bounds);
                levelCam.SetTarget(target.transform);

                // Drive the camera for several seconds; it should converge to the clamp.
                for (int i = 0; i < 300; i++)
                {
                    levelCam.StepOnce(SceneBuildHelpers.FixedStep);
                    yield return null;
                }

                var rect = bounds.Rect;
                float halfH = cam.orthographicSize;
                float halfW = halfH * cam.aspect;
                float maxX = rect.xMax - halfW;
                float maxY = rect.yMax - halfH;

                Assert.That(camGo.transform.position.x, Is.LessThanOrEqualTo(maxX + 0.01f),
                    $"Camera X {camGo.transform.position.x} exceeded bounds clamp maxX {maxX}.");
                Assert.That(camGo.transform.position.y, Is.LessThanOrEqualTo(maxY + 0.01f),
                    $"Camera Y {camGo.transform.position.y} exceeded bounds clamp maxY {maxY}.");
            }
            finally
            {
                SceneBuildHelpers.DestroyAllRoots();
                SceneBuildHelpers.RestorePhysicsTiming();
            }
        }
    }
}
