using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WildDigimon : MonoBehaviour
{
    [SerializeField] private EncounterData _encounter;
    [SerializeField] private float _patrolRadius = 5f;
    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private float _chaseSpeed = 4f;
    [SerializeField] private float _detectionRange = 8f;
    [SerializeField] private float _contactRange = 1.5f;
    [SerializeField] private float _gravity = -15f;

    private CharacterController _controller;
    private Vector3 _spawnPoint;
    private Vector3 _patrolTarget;
    private Transform _player;
    private float _verticalVelocity;

    private enum State { Patrol, Chase, Defeated }
    private State _state;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        _spawnPoint = transform.position;
        _patrolTarget = PickPatrolPoint();
        _player = FindFirstObjectByType<PlayerController>()?.transform;
    }

    private void Update()
    {
        if (_state == State.Defeated) return;
        if (_player == null) return;
        if (BattleSystem.Instance != null && BattleSystem.Instance.InBattle) return;

        if (_controller.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;
        _verticalVelocity += _gravity * Time.deltaTime;

        float distToPlayer = Vector3.Distance(transform.position, _player.position);

        switch (_state)
        {
            case State.Patrol:
                UpdatePatrol(distToPlayer);
                break;
            case State.Chase:
                UpdateChase(distToPlayer);
                break;
        }
    }

    private void UpdatePatrol(float distToPlayer)
    {
        if (distToPlayer < _detectionRange)
        {
            _state = State.Chase;
            return;
        }

        MoveToward(_patrolTarget, _patrolSpeed);

        Vector3 flat = _patrolTarget - transform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude < 1f)
            _patrolTarget = PickPatrolPoint();
    }

    private void UpdateChase(float distToPlayer)
    {
        if (distToPlayer > _detectionRange * 1.5f)
        {
            _state = State.Patrol;
            _patrolTarget = PickPatrolPoint();
            return;
        }

        MoveToward(_player.position, _chaseSpeed);

        if (distToPlayer < _contactRange)
            TriggerBattle();
    }

    private void MoveToward(Vector3 target, float speed)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            direction.Normalize();
            Vector3 velocity = direction * speed;
            velocity.y = _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            Vector3 velocity = new Vector3(0f, _verticalVelocity, 0f);
            _controller.Move(velocity * Time.deltaTime);
        }
    }

    private Vector3 PickPatrolPoint()
    {
        Vector2 offset = Random.insideUnitCircle * _patrolRadius;
        return _spawnPoint + new Vector3(offset.x, 0f, offset.y);
    }

    private void TriggerBattle()
    {
        if (BattleSystem.Instance == null || BattleSystem.Instance.InBattle) return;
        if (_encounter == null || _encounter.Species == null) return;

        DigimonInstance partner = FindFirstObjectByType<DigimonInstance>();
        if (partner == null) return;

        WildDigimonInstance wildInstance = new WildDigimonInstance(_encounter.Species, _encounter.StatScale);
        BattleSystem.Instance.StartBattle(partner, wildInstance, OnBattleEnd);
    }

    private void OnBattleEnd(BattleResult result)
    {
        switch (result)
        {
            case BattleResult.Win:
                if (_encounter != null)
                    Inventory.Instance.AddBits(_encounter.BitReward);
                _state = State.Defeated;
                gameObject.SetActive(false);
                break;

            case BattleResult.Lose:
                _state = State.Patrol;
                _patrolTarget = PickPatrolPoint();
                break;

            case BattleResult.Fled:
                _state = State.Patrol;
                _patrolTarget = PickPatrolPoint();
                break;
        }
    }
}
