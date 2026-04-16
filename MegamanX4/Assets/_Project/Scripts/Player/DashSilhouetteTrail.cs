using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerController))]
public class DashSilhouetteTrail : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] SpriteRenderer _sourceRenderer;

    [Header("Trail")]
    [SerializeField] Color _silhouetteColor = new(0.3f, 0.7f, 1f, 0.75f);
    [SerializeField] float _spawnInterval = 0.04f;
    [SerializeField] float _silhouetteLifetime = 0.18f;
    [SerializeField] int _sortingOrderOffset = -1;

    readonly List<Afterimage> _activeAfterimages = new();

    PlayerController _playerController;
    float _spawnTimer;
    bool _wasDashing;

    void Reset()
    {
        _playerController = GetComponent<PlayerController>();
        _sourceRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Awake()
    {
        if (!_playerController)
            _playerController = GetComponent<PlayerController>();

        if (!_sourceRenderer)
            _sourceRenderer = GetComponentInChildren<SpriteRenderer>();

        _spawnInterval = Mathf.Max(0.01f, _spawnInterval);
        _silhouetteLifetime = Mathf.Max(0.01f, _silhouetteLifetime);
    }

    void LateUpdate()
    {
        UpdateAfterimages();

        if (!_playerController || !_sourceRenderer)
            return;

        if (_playerController.IsDashing)
        {
            if (!_wasDashing)
                _spawnTimer = 0f;

            _spawnTimer -= Time.deltaTime;
            while (_spawnTimer <= 0f)
            {
                SpawnAfterimage();
                _spawnTimer += _spawnInterval;
            }
        }
        else
        {
            _spawnTimer = 0f;
        }

        _wasDashing = _playerController.IsDashing;
    }

    void OnDisable()
    {
        ClearAfterimages();
        _wasDashing = false;
        _spawnTimer = 0f;
    }

    void OnValidate()
    {
        _spawnInterval = Mathf.Max(0.01f, _spawnInterval);
        _silhouetteLifetime = Mathf.Max(0.01f, _silhouetteLifetime);
    }

    void UpdateAfterimages()
    {
        for (int i = _activeAfterimages.Count - 1; i >= 0; i--)
        {
            var afterimage = _activeAfterimages[i];
            afterimage.Age += Time.deltaTime;

            if (!afterimage.Renderer || afterimage.Age >= _silhouetteLifetime)
            {
                DestroyAfterimage(afterimage);
                _activeAfterimages.RemoveAt(i);
                continue;
            }

            float t = afterimage.Age / _silhouetteLifetime;
            var color = _silhouetteColor;
            color.a *= 1f - t;
            afterimage.Renderer.color = color;
        }
    }

    void SpawnAfterimage()
    {
        if (!_sourceRenderer.sprite)
            return;

        var afterimageObject = new GameObject("DashSilhouette");
        afterimageObject.layer = _sourceRenderer.gameObject.layer;
        afterimageObject.transform.SetPositionAndRotation(_sourceRenderer.transform.position, _sourceRenderer.transform.rotation);
        afterimageObject.transform.localScale = _sourceRenderer.transform.lossyScale;

        var renderer = afterimageObject.AddComponent<SpriteRenderer>();
        renderer.sprite = _sourceRenderer.sprite;
        renderer.color = _silhouetteColor;
        renderer.sortingLayerID = _sourceRenderer.sortingLayerID;
        renderer.sortingOrder = _sourceRenderer.sortingOrder + _sortingOrderOffset;
        renderer.maskInteraction = _sourceRenderer.maskInteraction;
        renderer.drawMode = _sourceRenderer.drawMode;
        renderer.size = _sourceRenderer.size;

        _activeAfterimages.Add(new Afterimage(afterimageObject, renderer));
    }

    void ClearAfterimages()
    {
        for (int i = 0; i < _activeAfterimages.Count; i++)
            DestroyAfterimage(_activeAfterimages[i]);

        _activeAfterimages.Clear();
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
