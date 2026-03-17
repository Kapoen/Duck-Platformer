namespace Level
{
    using System.Collections;
    using Player;
    using UnityEngine;

    /// <summary>
    /// Handle the logic for an enemy
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        private const float TurningDelay = 0.1f;

        [Header("Enemy")]
        [SerializeField]
        private bool staticTillAngered;
        [SerializeField]
        private bool smart;
        [SerializeField]
        private bool flying;
        [SerializeField]
        private bool ignoreCollision;
        [SerializeField]
        private float movingSpeed = 5f;
        [SerializeField]
        private bool startToLeft;
        [SerializeField]
        private float despawnTime = 5f;

        [Header("Layers")]
        [SerializeField]
        private LayerMask groundLayer;
        [SerializeField]
        private LayerMask platformLayer;
        [SerializeField]
        private LayerMask enemyCollisionLayer;
        [SerializeField]
        private LayerMask objectLayer;
        [SerializeField]
        private LayerMask playerLayer;

        private Rigidbody2D _rb;
        private Collider2D _collider;
        private SpriteRenderer _sprite;
        private Animator _animator;
        private Vector3 _startPosition;

        private int _movingDirection;
        private float _turningCounter;

        private bool _active;
        private bool _angered;

        /// <summary>
        /// Angers the enemy, causing it to move towards the place from where the enemy is angered.
        /// </summary>
        /// <param name="origin">The place from where the enemy is angered.</param>
        public void AngerEnemy(Vector2 origin)
        {
            if (origin.x < this.transform.position.x)
            {
                this._movingDirection = -1;
                this._sprite.flipX = true;
            }
            else
            {
                this._movingDirection = 1;
                this._sprite.flipX = false;
            }

            this._animator.enabled = true;

            this._active = true;
            this._angered = true;
        }

        /// <summary>
        /// Reset the enemy to its original position and state.
        /// </summary>
        public void ResetEnemy()
        {
            this.gameObject.SetActive(false);

            this._angered = false;

            this._movingDirection = this.startToLeft ? -1 : 1;
            this._sprite.flipX = this.startToLeft;

            this.transform.position = this._startPosition;

            this._active = this._sprite.isVisible;
            this._animator.enabled = this._sprite.isVisible;

            this.StopAllCoroutines();

            this.gameObject.SetActive(true);
        }

        private void Awake()
        {
            this._rb = this.GetComponent<Rigidbody2D>();
            this._collider = this.GetComponent<Collider2D>();
            this._sprite = this.GetComponent<SpriteRenderer>();
            this._animator = this.GetComponent<Animator>();
            this._startPosition = this.transform.position;

            if (this.flying)
            {
                this._rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            }

            if (this.ignoreCollision)
            {
                this._collider.layerOverridePriority = 1;

                // Exclude everything, but the player layer.
                this._collider.excludeLayers = ~this.playerLayer;
            }

            this._movingDirection = this.startToLeft ? -1 : 1;
            this._sprite.flipX = this.startToLeft;

            this._turningCounter = TurningDelay;

            this._animator.enabled = false;
        }

        /// <summary>
        /// Activate the enemy when it is on screen.
        /// </summary>
        private void OnBecameVisible()
        {
            this._active = true;
            this._animator.enabled = !this.staticTillAngered;

            this.StopAllCoroutines();
        }

        /// <summary>
        /// When enemy is off-screen for <see cref="despawnTime"/>, reset it.
        /// </summary>
        private void OnBecameInvisible()
        {
            if (!Application.isPlaying || !this.gameObject.activeSelf)
            {
                return;
            }

            this.StartCoroutine(this.Despawn());
        }

        /// <summary>
        /// Actually despawn the enemy.
        /// </summary>
        /// <returns>...</returns>
        private IEnumerator Despawn()
        {
            yield return new WaitForSeconds(this.despawnTime);

            this.StartCoroutine(this.RespawnEnemy());
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
                this.gameObject.SetActive(false);
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
            if (!this._active || (this.staticTillAngered && !this._angered))
            {
                return;
            }

            if (this.transform.position.y < LevelManager.Instance.GetKillPlaneY())
            {
                this.gameObject.SetActive(false);
                this.StopAllCoroutines();
                return;
            }

            if (!this.ignoreCollision && this.TurnAround() && this._turningCounter <= 0f)
            {
                this._movingDirection *= -1;
                this._sprite.flipX = !this._sprite.flipX;
                this._turningCounter = TurningDelay;
            }

            this._turningCounter -= Time.deltaTime;
            this._rb.linearVelocityX = this._movingDirection * this.movingSpeed;
        }

        /// <summary>
        /// Wait until the enemy spawn is off-screen, before spawning it back.
        /// </summary>
        private IEnumerator RespawnEnemy()
        {
            while (this.IsOnScreen())
            {
                yield return new WaitForEndOfFrame();
            }

            this.ResetEnemy();
        }

        /// <summary>
        /// Checks if the enemy is on-screen.
        /// </summary>
        /// <returns>If enemy is on-screen.</returns>
        private bool IsOnScreen()
        {
            if (Camera.main)
            {
                Vector3 viewportPoint = Camera.main.WorldToViewportPoint(this._startPosition);
                return viewportPoint.x is >= 0 and <= 1 && viewportPoint.y is >= 0 and <= 1;
            }

            return false;
        }

        /// <summary>
        /// Checks if there is ground in front of the enemy.
        /// </summary>
        /// <returns>A bool depending on if there is ground.</returns>
        private bool IsGround()
        {
            RaycastHit2D ray = Physics2D.Raycast(
                this.gameObject.transform.position + (this._movingDirection * new Vector3(this._collider.bounds.extents.x + 0.1f, 0, 0)),
                Vector2.down,
                this._collider.bounds.extents.y + 0.1f,
                this.groundLayer | this.platformLayer | this.enemyCollisionLayer);

            return ray.collider;
        }

        /// <summary>
        /// Checks if the enemy is against a wall.
        /// </summary>
        /// <returns>A bool depending on if the enemy is against a wall.</returns>
        private bool IsAgainstWall()
        {
            RaycastHit2D ray = Physics2D.Raycast(
                this.gameObject.transform.position - new Vector3(0, this._collider.bounds.extents.y - 0.1f, 0),
                this._movingDirection * Vector2.right,
                this._collider.bounds.extents.x + 0.1f,
                this.groundLayer | this.enemyCollisionLayer | this.objectLayer);

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
            bool turnOnGround = !this.flying && this.smart && !this._angered;
            return (this.IsGround() && this.IsAgainstWall()) || (turnOnGround && !this.IsGround());
        }
    }
}
