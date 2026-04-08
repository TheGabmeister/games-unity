using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GridData gridData;

    [Header("Movement")]
    [SerializeField] float moveTime = 0.15f;
    [SerializeField] float runMoveTime = 0.075f;

    public Vector2Int GridPosition { get; private set; }
    public int FacingDirection { get; private set; } = 2; // 0=up, 1=right, 2=down, 3=left

    bool isMoving;
    SpriteRenderer spriteRenderer;

    // Direction vectors: up, right, down, left
    static readonly Vector2Int[] DirectionVectors =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 10;
        UpdateSprite();
    }

    void OnEnable()
    {
        var pm = GameManager.Instance?.PartyManager;
        if (pm != null) pm.OnPartyChanged += UpdateSprite;
    }

    void OnDestroy()
    {
        var pm = GameManager.Instance?.PartyManager;
        if (pm != null) pm.OnPartyChanged -= UpdateSprite;
    }

    /// Rebuild the sprite from the party leader's class color. Call after party changes.
    public void UpdateSprite()
    {
        Color32 color = new Color32(50, 100, 220, 255); // default blue
        var leader = GameManager.Instance?.PartyManager?.Leader;
        if (leader?.ClassDef != null)
        {
            var c = leader.ClassDef.ClassColor;
            color = new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), 255);
        }

        int size = 16;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color32[size * size];
        var center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 1;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                pixels[y * size + x] = dist <= radius ? color : new Color32(0, 0, 0, 0);
            }

        tex.SetPixels32(pixels);
        tex.Apply();

        spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
    }

    public void SetPosition(Vector2Int pos)
    {
        GridPosition = pos;
        transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.StateManager.CurrentState != GameState.Exploration) return;
        if (isMoving) return;

        var input = GameManager.Instance.InputManager;
        if (input == null) return;

        // Check confirm for interaction
        if (input.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
        {
            Interact();
            return;
        }

        // Check menu
        if (input.MenuAction != null && input.MenuAction.WasPressedThisFrame())
        {
            Debug.Log("[Player] Menu requested");
            return;
        }

        // Movement
        if (input.MoveAction == null) return;
        Vector2 moveInput = input.MoveAction.ReadValue<Vector2>();

        int direction = -1;
        if (moveInput.y > 0.5f) direction = 0;       // up
        else if (moveInput.x > 0.5f) direction = 1;   // right
        else if (moveInput.y < -0.5f) direction = 2;   // down
        else if (moveInput.x < -0.5f) direction = 3;   // left

        if (direction >= 0)
        {
            FacingDirection = direction;
            TryMove(direction);
        }
    }

    async void TryMove(int direction)
    {
        Vector2Int target = GridPosition + DirectionVectors[direction];

        if (gridData == null || !gridData.IsPassable(target))
        {
            return;
        }

        isMoving = true;
        GridPosition = target;

        bool running = GameManager.Instance.InputManager.RunAction?.IsPressed() ?? false;
        float duration = running ? runMoveTime : moveTime;

        Vector3 targetWorld = new Vector3(target.x + 0.5f, target.y + 0.5f, 0f);
        await Tween.Position(transform, targetWorld, duration);

        isMoving = false;

        // Notify encounter system of completed step
        var encounter = FindAnyObjectByType<EncounterSystem>();
        encounter?.OnPlayerStep();
    }

    void Interact()
    {
        Vector2Int facingPos = GridPosition + DirectionVectors[FacingDirection];

        // Phase 1: no interactables exist yet.
        // Future phases will check for NPCs, chests, doors, etc. here.
        // Per spec: facing an empty tile or wall does nothing.
    }
}
