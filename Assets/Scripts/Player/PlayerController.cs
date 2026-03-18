namespace Player
{
    using System.Collections;
    using System.Collections.Generic;
    using Level;
    using UnityEngine;

    /// <summary>
    /// Handles the movement of the player.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        private readonly List<Platform> _oneWayPlatforms = new List<Platform>();

        [Header("Movement")]
        [SerializeField]
        private float moveSpeed = 8.25f;
        [SerializeField]
        private float smoothTime = 0.1f;
        [SerializeField]
        private float airSmoothTime = 0.175f;
        private Vector2 _input;
        private float _currentSpeed;

        [Header("Wall Movement")]
        [SerializeField]
        private float wallSlideSpeed = 5f;

        [Header("Jumping")]
        [SerializeField]
        private float jumpForce = 15f;
        [SerializeField]
        private float fallMultiplier = 2.5f;
        [SerializeField]
        private float lowJumpMultiplier = 2f;
        [SerializeField]
        private float coyoteTime = 0.15f;
        [SerializeField]
        private float jumpBufferTime = 0.15f;
        private bool _holdingJump;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;

        [Header("Enemy Bounce")]
        [SerializeField]
        private Vector2 bounceForce = new Vector2(7.5f, 10f);

        [Header("Wall Jumping")]
        [SerializeField]
        private Vector2 wallJumpForce = new Vector2(10f, 15f);
        [SerializeField]
        private float wallJumpImmovabilityTime = 0.275f;
        private bool _isWallJumping;

        private bool _isRecoiling;

        private Rigidbody2D _rb;

        private PlayerSpriteManager _spriteManager;
        private SurroundingsCheck _surroundingsCheck;
        private PlayerLives _playerLives;

        /// <summary>
        /// Set the move input.
        /// </summary>
        /// <param name="input">The move input.</param>
        public void OnMove(Vector2 input)
        {
            this._input = input;
        }

        /// <summary>
        /// Get the movement speed vector.
        /// </summary>
        /// <returns>The velocity vector.</returns>
        public Vector2 GetMovementSpeed()
        {
            return this._rb.linearVelocity;
        }

        /// <summary>
        /// Set if the jump button is currently held.
        /// </summary>
        /// <param name="holdingJump">If jump button is held.</param>
        public void SetJumpHeld(bool holdingJump)
        {
            this._holdingJump = holdingJump;
        }

        /// <summary>
        /// Reset the jump buffer counter.
        /// </summary>
        public void ResetJumpBufferCounter()
        {
            this._jumpBufferCounter = this.jumpBufferTime;
        }

        /// <summary>
        /// Multiply the velocity vector.
        /// </summary>
        /// <param name="factor">The factor to multiply with.</param>
        public void MultiplySpeed(Vector2 factor)
        {
            this._rb.linearVelocity = new Vector2(factor.x * this._rb.linearVelocityX, factor.y * this._rb.linearVelocityY);
        }

        /// <summary>
        /// Bounce the player from an enemy.
        /// </summary>
        public void EnemyBounce()
        {
            if (this._jumpBufferCounter > 0f)
            {
                this._rb.linearVelocityY = this.jumpForce;
                this._jumpBufferCounter = 0f;
            }
            else
            {
                this._rb.linearVelocity = new Vector2(this._input.x * this.bounceForce.x, this.bounceForce.y);
            }
        }

        /// <summary>
        /// Apply recoil to the player.
        /// </summary>
        /// <param name="speed">The velocity of the recoil.</param>
        /// <param name="recoilDuration">The duration of the recoil in seconds.</param>
        public void ApplyRecoil(Vector2 speed, float recoilDuration)
        {
            if (!this._isRecoiling)
            {
                this.StartCoroutine(this.RecoilRoutine(speed, recoilDuration));
            }
        }

        /// <summary>
        /// Finish the wall jump: <br />
        /// 1. Wait for <see cref="wallJumpImmovabilityTime"/>. <br />
        /// 2. Set <see cref="_isWallJumping"/> to false. <br />
        /// </summary>
        /// <returns>...</returns>
        private IEnumerator FinishWallJump()
        {
            yield return new WaitForSeconds(this.wallJumpImmovabilityTime);
            this._isWallJumping = false;
        }

        /// <summary>
        /// If the player enter the hitbox of a one way platform, save it in <see cref="_oneWayPlatforms"/>.
        /// </summary>
        /// <param name="other">The <see cref="GameObject"/> of which the player enters the <see cref="Collider2D"/>.</param>
        private void OnCollisionEnter2D(Collision2D other)
        {
            Platform platform = other.gameObject.GetComponent<Platform>();
            if (platform && platform.Type == PlatformType.OneWay)
            {
                this._oneWayPlatforms.Add(platform);
            }
        }

        /// <summary>
        /// If the player leaves the hitbox of a one way platform, remove it from <see cref="_oneWayPlatforms"/>.
        /// </summary>
        /// <param name="other">The <see cref="GameObject"/> of which the player enters the <see cref="Collider2D"/>.</param>
        private void OnCollisionExit2D(Collision2D other)
        {
            Platform platform = other.gameObject.GetComponent<Platform>();
            if (platform && platform.Type == PlatformType.OneWay)
            {
                this._oneWayPlatforms.Remove(platform);
            }
        }

        /// <summary>
        /// Update the timers.
        /// </summary>
        private void UpdateTimers()
        {
            this._jumpBufferCounter -= Time.deltaTime;

            if (this._surroundingsCheck.IsGrounded())
            {
                this._coyoteTimeCounter = this.coyoteTime;
            }
            else
            {
                this._coyoteTimeCounter -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Handle dropping through a falling platform.
        /// </summary>
        private void HandleDropThrough()
        {
            if (this._input.y < 0f && this._jumpBufferCounter > 0 && this._oneWayPlatforms.Count > 0)
            {
                this._jumpBufferCounter = 0f;

                Platform[] platforms = new Platform[this._oneWayPlatforms.Count];
                this._oneWayPlatforms.CopyTo(platforms);
                foreach (Platform platform in platforms)
                {
                    platform.DropThrough();
                }
            }
        }

        /// <summary>
        /// Update the <see cref="PlayerSpriteManager"/>.
        /// </summary>
        private void UpdateAnimator()
        {
            this._spriteManager.UpdateSpeed(this._rb.linearVelocity);

            if (this._surroundingsCheck.WallDirection() != 0 && !this._surroundingsCheck.IsGrounded())
            {
                this._spriteManager.OnWall(true, this._surroundingsCheck.WallDirection());
            }
            else
            {
                this._spriteManager.OnWall(false, 0);
                if (this._surroundingsCheck.IsWallSliding())
                {
                    this._spriteManager.Flip();
                }
            }
        }

        /// <summary>
        /// Apply movement to the player.
        /// </summary>
        private void ApplyMovement()
        {
            if (!this._isWallJumping && (this._surroundingsCheck.WallDirection() == 0 || this._surroundingsCheck.IsGrounded()))
            {
                float targetSpeed = this._input.x * this.moveSpeed;
                float currentSmoothTime = this._surroundingsCheck.IsGrounded() ? this.smoothTime : this.airSmoothTime;

                float smoothedX = Mathf.SmoothDamp(
                    this._rb.linearVelocity.x,
                    targetSpeed,
                    ref this._currentSpeed,
                    currentSmoothTime);

                this._rb.linearVelocityX = smoothedX;
            }

            // Fall faster when falling.
            if (this._rb.linearVelocityY <= 0f)
            {
                this._rb.linearVelocityY += Physics2D.gravity.y * (this.fallMultiplier - 1) * Time.fixedDeltaTime;
            }

            // If the player is moving up and isn't holding down jump. Make the "jump" shorter.
            else if (this._rb.linearVelocityY > 0 && !this._holdingJump)
            {
                this._rb.linearVelocityY += Physics2D.gravity.y * (this.lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }

            // If the player is on the wall, limit the falling speed.
            if (this._surroundingsCheck.WallDirection() != 0 && !this._surroundingsCheck.IsGrounded())
            {
                this._rb.linearVelocityY = Mathf.Max(this._rb.linearVelocityY, -this.wallSlideSpeed);
            }
        }

        /// <summary>
        /// Apply the jumps to the player.
        /// </summary>
        private void ApplyJump()
        {
            if (this._jumpBufferCounter > 0f
                && !this._surroundingsCheck.IsGrounded()
                && this._surroundingsCheck.WallDirection() != 0)
            {
                this._spriteManager.OnJump();

                this._rb.linearVelocity = new Vector2(
                    -this._surroundingsCheck.WallDirection() * this.wallJumpForce.x,
                    this.wallJumpForce.y);
                this._isWallJumping = true;
                this._jumpBufferCounter = 0f;

                this.StartCoroutine(this.FinishWallJump());
            }
            else if (this._jumpBufferCounter > 0f && this._coyoteTimeCounter > 0f)
            {
                this._rb.linearVelocityY = this.jumpForce;
                this._jumpBufferCounter = 0f;
                this._coyoteTimeCounter = 0f;

                this._spriteManager.OnJump();
            }
        }

        private void Update()
        {
            this.UpdateTimers();
            this.HandleDropThrough();
            this.UpdateAnimator();
        }

        private void FixedUpdate()
        {
            // Don't apply movement during the recoil
            if (this._isRecoiling)
            {
                if (this._surroundingsCheck.WallDirection() == 0)
                {
                    this._rb.linearVelocityY = 0f;
                    return;
                }

                // Cancel the recoil when a wall is hit.
                this._rb.linearVelocity = Vector2.zero;

                this._isRecoiling = false;
                this.StopCoroutine(nameof(this.RecoilRoutine));
            }

            this.ApplyMovement();
            this.ApplyJump();

            if (this.transform.position.y < LevelManager.Instance.GetKillPlaneY())
            {
                this._playerLives.Die();
            }
        }

        /// <summary>
        /// Handle the recoil: <br />
        /// 1. Set <see cref="_isRecoiling"/> to true and set the speed to <paramref name="speed"/>.
        /// 2. Wait for <paramref name="recoilDuration"/>.
        /// 3. Set <see cref="_isRecoiling"/> to false.
        /// </summary>
        /// <param name="speed">The velocity of the recoil.</param>
        /// <param name="recoilDuration">The duration of the recoil in seconds.</param>
        /// <returns>...</returns>
        private IEnumerator RecoilRoutine(Vector2 speed, float recoilDuration)
        {
            this._isRecoiling = true;

            this._rb.linearVelocity = speed;

            yield return new WaitForSeconds(recoilDuration);

            this._isRecoiling = false;
        }

        private void Awake()
        {
            this._rb = this.gameObject.GetComponent<Rigidbody2D>();

            this._surroundingsCheck = this.gameObject.GetComponent<SurroundingsCheck>();
            this._spriteManager = this.gameObject.GetComponent<PlayerSpriteManager>();
            this._playerLives = this.gameObject.GetComponent<PlayerLives>();
        }
    }
}
