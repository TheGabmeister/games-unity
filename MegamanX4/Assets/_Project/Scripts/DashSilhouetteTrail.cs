using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerController))]
public class DashSilhouetteTrail : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] SpriteRenderer sourceRenderer;

    [Header("Trail")]
    [SerializeField] Color silhouetteColor = new(0.3f, 0.7f, 1f, 0.75f);
    [SerializeField] float spawnInterval = 0.04f;
    [SerializeField] float silhouetteLifetime = 0.18f;
    [SerializeField] int sortingOrderOffset = -1;

    readonly List<Afterimage> activeAfterimages = new();

    PlayerController playerController;
    float spawnTimer;
    bool wasDashing;

    void Reset()
    {
        playerController = GetComponent<PlayerController>();
        sourceRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Awake()
    {
        if (!playerController)
            playerController = GetComponent<PlayerController>();

        if (!sourceRenderer)
            sourceRenderer = GetComponentInChildren<SpriteRenderer>();

        spawnInterval = Mathf.Max(0.01f, spawnInterval);
        silhouetteLifetime = Mathf.Max(0.01f, silhouetteLifetime);
    }

    void LateUpdate()
    {
        UpdateAfterimages();

        if (!playerController || !sourceRenderer)
            return;

        if (playerController.IsDashing)
        {
            if (!wasDashing)
                spawnTimer = 0f;

            spawnTimer -= Time.deltaTime;
            while (spawnTimer <= 0f)
            {
                SpawnAfterimage();
                spawnTimer += spawnInterval;
            }
        }
        else
        {
            spawnTimer = 0f;
        }

        wasDashing = playerController.IsDashing;
    }

    void OnDisable()
    {
        ClearAfterimages();
        wasDashing = false;
        spawnTimer = 0f;
    }

    void OnValidate()
    {
        spawnInterval = Mathf.Max(0.01f, spawnInterval);
        silhouetteLifetime = Mathf.Max(0.01f, silhouetteLifetime);
    }

    void UpdateAfterimages()
    {
        for (int i = activeAfterimages.Count - 1; i >= 0; i--)
        {
            var afterimage = activeAfterimages[i];
            afterimage.Age += Time.deltaTime;

            if (!afterimage.Renderer || afterimage.Age >= silhouetteLifetime)
            {
                DestroyAfterimage(afterimage);
                activeAfterimages.RemoveAt(i);
                continue;
            }

            float t = afterimage.Age / silhouetteLifetime;
            var color = silhouetteColor;
            color.a *= 1f - t;
            afterimage.Renderer.color = color;
        }
    }

    void SpawnAfterimage()
    {
        if (!sourceRenderer.sprite)
            return;

        var afterimageObject = new GameObject("DashSilhouette");
        afterimageObject.layer = sourceRenderer.gameObject.layer;
        afterimageObject.transform.SetPositionAndRotation(sourceRenderer.transform.position, sourceRenderer.transform.rotation);
        afterimageObject.transform.localScale = sourceRenderer.transform.lossyScale;

        var renderer = afterimageObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sourceRenderer.sprite;
        renderer.color = silhouetteColor;
        renderer.sortingLayerID = sourceRenderer.sortingLayerID;
        renderer.sortingOrder = sourceRenderer.sortingOrder + sortingOrderOffset;
        renderer.maskInteraction = sourceRenderer.maskInteraction;
        renderer.drawMode = sourceRenderer.drawMode;
        renderer.size = sourceRenderer.size;

        activeAfterimages.Add(new Afterimage(afterimageObject, renderer));
    }

    void ClearAfterimages()
    {
        for (int i = 0; i < activeAfterimages.Count; i++)
            DestroyAfterimage(activeAfterimages[i]);

        activeAfterimages.Clear();
    }

    static void DestroyAfterimage(Afterimage afterimage)
    {
        if (afterimage.GameObject)
            Destroy(afterimage.GameObject);
    }

    sealed class Afterimage
    {
        public GameObject GameObject { get; }
        public SpriteRenderer Renderer { get; }
        public float Age { get; set; }

        public Afterimage(GameObject gameObject, SpriteRenderer renderer)
        {
            GameObject = gameObject;
            Renderer = renderer;
        }
    }
}
