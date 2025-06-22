using UnityEngine;
using UnityEditor;

public class Utils
{
    public static Vector3 GetPlayerStart()
    {
        var playerStart = GameObject.FindGameObjectWithTag("PlayerStart");
        if (!playerStart)
        {
            playerStart = GameObject.Find("PlayerStart");
            if (!playerStart)
            {
                // Use editor camera view instead
                Vector3 cameraPos = SceneView.lastActiveSceneView.camera.transform.position;
                return new Vector3(cameraPos.x, cameraPos.y, 0);
            }
        }
        return playerStart.transform.position;
    }
}
