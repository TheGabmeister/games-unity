using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform target;
    [SerializeField] GridData gridData;

    [Header("Settings")]
    [SerializeField] float smoothSpeed = 10f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f; // shows ~10 tiles vertically
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

        // Clamp to map bounds so the camera never shows void
        if (gridData != null && gridData.Width > 0 && gridData.Height > 0)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float minX = halfWidth;
            float maxX = gridData.Width - halfWidth;
            float minY = halfHeight;
            float maxY = gridData.Height - halfHeight;

            // If map is smaller than camera view, center it
            if (minX > maxX)
                desired.x = gridData.Width / 2f;
            else
                desired.x = Mathf.Clamp(desired.x, minX, maxX);

            if (minY > maxY)
                desired.y = gridData.Height / 2f;
            else
                desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
