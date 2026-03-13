using Player;
using System.Collections;
using UnityEngine;

namespace Level
{
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy")]
        [SerializeField] private bool smart;
        [SerializeField] private bool flying;
        [SerializeField] private bool ignoreCollision;
        [SerializeField] private float movingSpeed = 5f;
        [SerializeField] private bool startToLeft;
        [SerializeField] private float despawnTime = 5f;

        [Header("Layers")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask platformLayer;
        [SerializeField] private LayerMask enemyCollisionLayer;
        [SerializeField] private LayerMask objectLayer;
        [SerializeField] private LayerMask playerLayer;

        private Rigidbody2D _rb;
        private Collider2D _collider;
        private SpriteRenderer _sprite;
        private Animator _animator;
        private Vector3 _startPosition;
    
        private int _movingDirection;
        private readonly float _turningDelay = 0.1f;
        private float _turningCounter;

        private bool _active;
        private bool _angered;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _startPosition = transform.position;

            if (flying)
            {
                _rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            }

            if (ignoreCollision)
            {
                _collider.layerOverridePriority = 1;
                // Exclude everything, but the player layer.
                _collider.excludeLayers = ~playerLayer;
            }
        
            _movingDirection = startToLeft ? -1 : 1;
            _sprite.flipX = startToLeft;

            _turningCounter = _turningDelay;

            _animator.enabled = false;
        }

        /// <summary>
        /// Activate the enemy when it is on screen.
        /// </summary>
        private void OnBecameVisible()
        {
            _active = true;
            _animator.enabled = false;
        
            StopAllCoroutines();
        }

        /// <summary>
        /// When enemy is off-screen for <see cref="despawnTime"/>, reset it.
        /// </summary>
        private void OnBecameInvisible()
        {
            if (!Application.isPlaying || !gameObject.activeSelf)
            {
                return;
            }

            StartCoroutine(Despawn());
        }

        /// <summary>
        /// Actually despawn the enemy.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Despawn()
        {
            yield return new WaitForSeconds(despawnTime);
        
            ResetEnemy();
        }

        /// <summary>
        /// If the player enters the hitbox of the enemy, let the player bounce if the player hits the enemy from above, else the player dies.
        /// </summary>
        /// <param name="other">The <see cref="GameObject"/> that enters the hitbox.</param>
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

        /// <summary>
        /// Turn the enemy around if needed and move it.
        /// </summary>
        private void FixedUpdate()
        {
            if (!_active)
            {
                return;
            }

            if (transform.position.y < LevelManager.Instance.GetKillPlaneY())
            {
                gameObject.SetActive(false);
                StopAllCoroutines();
                return;
            }
        
            if (!ignoreCollision && TurnAround() && _turningCounter <= 0f)
            {
                _movingDirection *= -1;
                _sprite.flipX = !_sprite.flipX;
                _turningCounter = _turningDelay;
            }

            _turningCounter -= Time.deltaTime;
            _rb.linearVelocityX = _movingDirection * movingSpeed;
        }
    
        /// <summary>
        /// Reset the enemy to its original position and state.
        /// </summary>
        public void ResetEnemy()
        {
            _movingDirection = startToLeft ? -1 : 1;
            _sprite.flipX = startToLeft;
        
            transform.position = _startPosition;

            _active = _sprite.isVisible;
            _animator.enabled = _sprite.isVisible;

            _angered = false;
        
            StopAllCoroutines();
        
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Checks if there is ground in front of the enemy.
        /// </summary>
        /// <returns>A bool depending on if there is ground.</returns>
        private bool IsGround()
        {
            RaycastHit2D ray = Physics2D.Raycast(
                gameObject.transform.position + _movingDirection * new Vector3(_collider.bounds.extents.x + 0.1f, 0, 0),
                Vector2.down,
                _collider.bounds.extents.y + 0.1f,
                groundLayer | platformLayer | enemyCollisionLayer
            );
        
            return ray.collider;
        }

        /// <summary>
        /// Checks if the enemy is against a wall.
        /// </summary>
        /// <returns>A bool depending on if the enemy is against a wall.</returns>
        private bool IsAgainstWall()
        {
            RaycastHit2D ray = Physics2D.Raycast(
                gameObject.transform.position, 
                _movingDirection * Vector2.right, 
                _collider.bounds.extents.x + 0.1f, 
                groundLayer | enemyCollisionLayer | objectLayer
            );

            return ray.collider;
        }

        /// <summary>
        /// Checks if the enemy should turn around, so if it's against a wall
        /// or if it's smart and there is no ground in front of it. <br />
        /// Angered enemies, will always walk off edges.
        /// </summary>
        /// <returns>A bool depending on if the enemy should turn around.</returns>
        private bool TurnAround()
        {
            bool turnOnGround = !flying && smart && !_angered;
            return IsAgainstWall() || (turnOnGround && !IsGround());
        }

        /// <summary>
        /// Angers the enemy, causing it to move towards the place from where the enemy is angered.
        /// </summary>
        /// <param name="origin">The place from where the enemy is angered.</param>
        public void AngerEnemy(Vector2 origin)
        {
            if (origin.x < transform.position.x)
            {
                _movingDirection = -1;
                _sprite.flipX = true;
            }
            else
            {
                _movingDirection = 1;
                _sprite.flipX = false;
            }

            _active = true;
            _angered = true;
        }
    }
}
