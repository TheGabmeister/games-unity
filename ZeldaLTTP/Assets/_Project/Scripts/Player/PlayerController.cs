using UnityEngine;
using EventBus;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{

    PlayerInputHandler _input;
    [SerializeField] float _interactDistance = 1f;
    [SerializeField] Weapon _weapon;

    private void OnEnable()
    {
        _input.MenuAction += ToggleMenu;
        _input.InteractAction += Interact;
        _input.InventoryAction += ToggleInventory;
        _input.AttackAction += Attack;
    }

    private void OnDisable()
    {
        _input.MenuAction -= ToggleMenu;
        _input.InteractAction -= Interact;
        _input.InventoryAction -= ToggleInventory;
        _input.AttackAction -= Attack;
    }

    void Awake()
    {
        _input = GetComponent<PlayerInputHandler>();
    }

    private void Start()
    {
       
    }

    void ToggleMenu()
    {
        Bus<E_GameUI_ToggleMenu>.Raise(new E_GameUI_ToggleMenu { });
    }

    void Interact()
    {
        Vector2 forward = transform.TransformDirection(Vector2.right) * _interactDistance;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, forward);
        Debug.DrawRay(transform.position, forward, Color.green);

        if (hit)
        {
            var interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                Debug.Log(hit.collider.gameObject);
                interactable.Interact();
            }
        }
    }

    void ToggleInventory()
    {
        Bus<E_GameUI_ToggleInventory>.Raise(new E_GameUI_ToggleInventory { });
    }

    void Attack()
    {
        _weapon?.Attack();
    }
}
