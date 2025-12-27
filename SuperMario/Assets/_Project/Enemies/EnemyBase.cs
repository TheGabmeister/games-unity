using ScriptableObjectArchitecture;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] EnemyData _enemyData;
    int _health;
    [SerializeField] protected bool _isInvulnerable = false;
    [SerializeField] GameObject _scorePopup;
    protected GameObject _player; 

    [Header("Call these events...")]
    [SerializeField] IntGameEvent _updateScore;

    protected virtual void Start()
    {
        _health = _enemyData.health;
    }

    public void TakeDamage()
    {
        _health--;
        if(_health <= 0)
        {
            Die();
        }
    }

    protected void Die()
    {
        _updateScore.Raise(_enemyData.score);
        Destroy(gameObject);
    }

    public virtual void GetStomped()
    {
        
    }

    protected void SpawnScorePopup()
    {
        string scoreString = _scorePopup.GetComponentInChildren<TMP_Text>().text;
        var scorePopup = Instantiate(_scorePopup, transform.position, Quaternion.identity);
        scorePopup.GetComponentInChildren<TMP_Text>().text = _enemyData.score.ToString();
    }

    public void SetPlayerGameObject(GameObject player)
    {
        _player = player;
    }
}
