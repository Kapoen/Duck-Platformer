using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private bool smart;
    [SerializeField] private bool flying;
    [SerializeField] private bool ignoreCollision;
    [SerializeField] private float movingSpeed = 5f;
    [SerializeField] private bool startToLeft;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private LayerMask enemyCollisionLayer;
    [SerializeField] private LayerMask playerLayer;

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private SpriteRenderer _sprite;
    private Vector3 _startPosition;
    
    private int _movingDirection;
    private readonly float _turningDelay = 0.1f;
    private float _turningCounter;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _startPosition = transform.position;

        if (flying)
        {
            _rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        }

        if (ignoreCollision)
        {
            _collider.layerOverridePriority = 1;
            _collider.excludeLayers = ~playerLayer;
        }
        
        _movingDirection = startToLeft ? -1 : 1;
        _sprite.flipX = startToLeft;

        _turningCounter = _turningDelay;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (other.GetContact(0).normal.y <= -0.5f)
        {
            other.gameObject.GetComponent<PlayerController>().EnemyBounce();
            gameObject.SetActive(false);
        }
        else
        {
            other.gameObject.GetComponent<PlayerLives>().Die();
        }
    }

    private void FixedUpdate()
    {
        if (!ignoreCollision && TurnAround() && _turningCounter <= 0f)
        {
            _movingDirection *= -1;
            _sprite.flipX = !_sprite.flipX;
            _turningCounter = _turningDelay;
        }

        _turningCounter -= Time.deltaTime;
        _rb.linearVelocityX = _movingDirection * movingSpeed;
    }
    
    public void ResetEnemy()
    {
        _movingDirection = startToLeft ? -1 : 1;
        _sprite.flipX = startToLeft;
        
        transform.position = _startPosition;
        
        gameObject.SetActive(true);
    }

    private bool Grounded()
    {
        RaycastHit2D ray = Physics2D.Raycast(
            gameObject.transform.position + _movingDirection * new Vector3(_collider.bounds.extents.x + 0.1f, 0, 0),
            Vector2.down,
            _collider.bounds.extents.y + 0.1f,
            groundLayer | platformLayer | enemyCollisionLayer
        );
        
        return ray.collider;
    }

    private bool AgainstWall()
    {
        RaycastHit2D ray = Physics2D.Raycast(
            gameObject.transform.position, 
            _movingDirection * Vector2.right, 
            _collider.bounds.extents.x + 0.1f, 
            groundLayer | enemyCollisionLayer
        );

        return ray.collider;
    }

    private bool TurnAround()
    {
        return AgainstWall() || (smart && !Grounded());
    }
}
