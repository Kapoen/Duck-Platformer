using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")] 
        [SerializeField] private float moveSpeed = 8.25f;
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private float airSmoothTime = 0.175f;
        private Vector2 _input;
        private float _currentSpeed;

        [Header("Wall Movement")] 
        [SerializeField] private float wallSlideSpeed = 5f;
    
        [Header("Jumping")] 
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.15f;
        private bool _holdingJump;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;

        [Header("Enemy Bounce")]
        [SerializeField] private Vector2 bounceForce = new Vector2(7.5f, 10f);
    
        [Header("Wall Jumping")] 
        [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 15f);
        [SerializeField] private float wallJumpImmovabilityTime = 0.275f;
        private bool _isWallJumping;

        private bool _isRecoiling;
    
        private Rigidbody2D _rb;
    
        private readonly List<Platform> _oneWayPlatforms = new List<Platform>();

        private PlayerSpriteManager _spriteManager;
        private SurroundingsCheck _surroundingsCheck;
        private PlayerLives _playerLives;

        private void Awake()
        {
            _rb = gameObject.GetComponent<Rigidbody2D>();

            _surroundingsCheck = gameObject.GetComponent<SurroundingsCheck>();
            _spriteManager = gameObject.GetComponent<PlayerSpriteManager>();
            _playerLives = gameObject.GetComponent<PlayerLives>();
        }

        /// <summary>
        /// Set the move input.
        /// </summary>
        /// <param name="input">The move input.</param>
        public void OnMove(Vector2 input)
        {
            _input = input;
        }

        /// <summary>
        /// Get the movement speed vector.
        /// </summary>
        /// <returns>The velocity vector.</returns>
        public Vector2 GetMovementSpeed()
        {
            return _rb.linearVelocity;
        }

        /// <summary>
        /// Set if the jump button is currently held.
        /// </summary>
        /// <param name="holdingJump">If jump button is held.</param>
        public void SetJumpHeld(bool holdingJump)
        {
            _holdingJump = holdingJump;
        }

        /// <summary>
        /// Reset the jump buffer counter.
        /// </summary>
        public void ResetJumpBufferCounter()
        {
            _jumpBufferCounter = jumpBufferTime;
        }

        /// <summary>
        /// Multiply the velocity vector.
        /// </summary>
        /// <param name="factor">The factor to multiply with.</param>
        public void MultiplySpeed(Vector2 factor)
        {
            _rb.linearVelocity = new Vector2(factor.x * _rb.linearVelocityX, factor.y * _rb.linearVelocityY);
        }
    
        /// <summary>
        /// Finish the wall jump: <br />
        /// 1. Wait for <see cref="wallJumpImmovabilityTime"/>. <br />
        /// 2. Set <see cref="_isWallJumping"/> to false. <br />
        /// </summary>
        /// <returns></returns>
        private IEnumerator FinishWallJump()
        {
            yield return new WaitForSeconds(wallJumpImmovabilityTime);
            _isWallJumping = false;
        }
    
        /// <summary>
        /// Bounce the player from an enemy.
        /// </summary>
        public void EnemyBounce()
        {
            if (_jumpBufferCounter > 0f)
            {
                _rb.linearVelocityY = jumpForce;
                _jumpBufferCounter = 0f;
            }
            else
            {
                _rb.linearVelocity = new Vector2(_input.x * bounceForce.x, bounceForce.y);
            }
        }

        /// <summary>
        /// If the player enter the hitbox of a one way platform, save it in <see cref="_oneWayPlatforms"/>.
        /// </summary>
        /// <param name="other">The <see cref="GameObject"/> of which the player enters the <see cref="Collider2D"/>.</param>
        private void OnCollisionEnter2D(Collision2D other)
        {
            Platform platform = other.gameObject.GetComponent<Platform>();
            if (platform && platform.type == PlatformType.OneWay)
            {
                _oneWayPlatforms.Add(platform);
            }
        }

        /// <summary>
        /// If the player leaves the hitbox of a one way platform, remove it from <see cref="_oneWayPlatforms"/>.
        /// </summary>
        /// <param name="other">The <see cref="GameObject"/> of which the player enters the <see cref="Collider2D"/>.</param>
        private void OnCollisionExit2D(Collision2D other)
        {
            Platform platform = other.gameObject.GetComponent<Platform>();
            if (platform && platform.type == PlatformType.OneWay)
            {
                _oneWayPlatforms.Remove(platform);
            }
        }

        /// <summary>
        /// Update the timers.
        /// </summary>
        private void UpdateTimers()
        {
            _jumpBufferCounter -= Time.deltaTime;
        
            if (_surroundingsCheck.IsGrounded())
            {
                _coyoteTimeCounter = coyoteTime;
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Handle dropping through a falling platform.
        /// </summary>
        private void HandleDropThrough()
        {
            if (_input.y < 0f && _jumpBufferCounter > 0 && _oneWayPlatforms.Count > 0)
            {
                _jumpBufferCounter = 0f;

                Platform[] platforms = new Platform[_oneWayPlatforms.Count];
                _oneWayPlatforms.CopyTo(platforms);
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
            _spriteManager.UpdateSpeed(_rb.linearVelocity);

            if (_surroundingsCheck.WallDirection() != 0 && !_surroundingsCheck.IsGrounded())
            {
                _spriteManager.OnWall(true, _surroundingsCheck.WallDirection());
            }
            else
            {
                _spriteManager.OnWall(false, 0);
                if (_surroundingsCheck.IsWallSliding())
                {
                    _spriteManager.Flip();
                }
            }
        }

        /// <summary>
        /// Apply movement to the player.
        /// </summary>
        private void ApplyMovement()
        {
            if (!_isWallJumping && (_surroundingsCheck.WallDirection() == 0 || _surroundingsCheck.IsGrounded()))
            {
                float targetSpeed = _input.x * moveSpeed;
                float currentSmoothTime = _surroundingsCheck.IsGrounded() ? smoothTime : airSmoothTime;
            
                float smoothedX = Mathf.SmoothDamp(
                    _rb.linearVelocity.x, 
                    targetSpeed, 
                    ref _currentSpeed, 
                    currentSmoothTime
                );

                _rb.linearVelocityX = smoothedX;
            }

            // Fall faster when falling.
            if (_rb.linearVelocityY <= 0f)
            {   
                _rb.linearVelocityY += Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            // If the player is moving up and isn't holding down jump. Make the "jump" shorter.
            else if (_rb.linearVelocityY > 0 && !_holdingJump)
            {
                _rb.linearVelocityY += Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        
            // If the player is on the wall, limit the falling speed.
            if (_surroundingsCheck.WallDirection() != 0 && !_surroundingsCheck.IsGrounded())
            {
                _rb.linearVelocityY = Mathf.Max(_rb.linearVelocityY, -wallSlideSpeed);
            }
        }
    
        /// <summary>
        /// Apply the jumps to the player.
        /// </summary>
        private void ApplyJump()
        {
            if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
            {
                _rb.linearVelocityY = jumpForce;
                _jumpBufferCounter = 0f;
                _coyoteTimeCounter = 0f;

                _spriteManager.OnJump();
            }
        
            if (_jumpBufferCounter > 0f 
                && !_surroundingsCheck.IsGrounded() 
                && _surroundingsCheck.WallDirection() != 0)
            {
                _spriteManager.OnJump();
            
                _rb.linearVelocity = new Vector2(-_surroundingsCheck.WallDirection() * wallJumpForce.x, wallJumpForce.y);
                _isWallJumping = true;
                _jumpBufferCounter = 0f;

                StartCoroutine(FinishWallJump());
            }
        }
    
        private void Update()
        {
            UpdateTimers();
            HandleDropThrough();
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            // Don't apply movement during the recoil
            if (_isRecoiling)
            {
                if (_surroundingsCheck.WallDirection() == 0)
                {
                    _rb.linearVelocityY = 0f;
                    return;
                }

                // Cancel the recoil when a wall is hit.
                _rb.linearVelocity = Vector2.zero;
                
                _isRecoiling = false;
                StopCoroutine(nameof(RecoilRoutine));
            }
        
            ApplyMovement();
            ApplyJump();

            if (transform.position.y < LevelManager.Instance.GetKillPlaneY())
            { 
                _playerLives.Die();   
            }
        }

        /// <summary>
        /// Apply recoil to the player.
        /// </summary>
        /// <param name="speed">The velocity of the recoil.</param>
        /// <param name="recoilDuration">The duration of the recoil in seconds.</param>
        public void ApplyRecoil(Vector2 speed, float recoilDuration)
        {
            if (!_isRecoiling) { 
                StartCoroutine(RecoilRoutine(speed, recoilDuration)); 
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
        /// <returns></returns>
        private IEnumerator RecoilRoutine(Vector2 speed, float recoilDuration)
        {
            _isRecoiling = true;

            _rb.linearVelocity = speed;

            yield return new WaitForSeconds(recoilDuration);

            _isRecoiling = false;
        }
    }
}
