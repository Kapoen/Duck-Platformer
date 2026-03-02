using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")] 
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float airAcceleration = 15f;
    private Vector2 _input;

    [Header("Wall Movement")] 
    [SerializeField] private float wallSlideSpeed = 5f;
    private bool _wallSliding;
    
    [Header("Jumping")] 
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    private bool _jumpHeld;
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

    private void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();

        _surroundingsCheck = gameObject.GetComponent<SurroundingsCheck>();
        _spriteManager = gameObject.GetComponent<PlayerSpriteManager>();
    }

    public void OnMove(Vector2 input)
    {
        _input = input;
    }

    public Vector2 GetMovementSpeed()
    {
        return _rb.linearVelocity;
    }

    public void SetJumpHeld(bool jumpHeld)
    {
        _jumpHeld = jumpHeld;
    }

    public void ResetJumpBufferCounter()
    {
        _jumpBufferCounter = jumpBufferTime;
    }

    public void MultiplySpeed(Vector2 factor)
    {
        _rb.linearVelocity = new Vector2(factor.x * _rb.linearVelocityX, factor.y * _rb.linearVelocityY);
    }
    
    private IEnumerator FinishWallJump()
    {
        yield return new WaitForSeconds(wallJumpImmovabilityTime);
        _isWallJumping = false;
    }
    
    public void EnemyBounce()
    {
        if (_jumpBufferCounter > 0f)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, jumpForce);
            _jumpBufferCounter = 0f;
        }
        else
        {
            _rb.linearVelocity = new Vector2(_input.x * bounceForce.x, bounceForce.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Platform platform = other.gameObject.GetComponent<Platform>();
        if (platform && platform.type == PlatformType.OneWay)
        {
            _oneWayPlatforms.Add(platform);
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        Platform platform = other.gameObject.GetComponent<Platform>();
        if (platform && platform.type == PlatformType.OneWay)
        {
            _oneWayPlatforms.Remove(platform);
        }
    }

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
            if (_wallSliding)
            {
                _spriteManager.Flip();
            }
        }
    }

    private void ApplyMovement()
    {
        if (_isRecoiling)
        {
            return;
        }
        
        float targetSpeed = _input.x * moveSpeed;
        float currentAccel = _surroundingsCheck.IsGrounded() ? acceleration : airAcceleration;
        
        float speedDiff = targetSpeed - _rb.linearVelocityX;
        float movement = speedDiff * currentAccel;
        if (!_isWallJumping && (_surroundingsCheck.WallDirection() == 0 || _surroundingsCheck.IsGrounded()))
        {
            _rb.AddForceX(movement);
        }

        if (_rb.linearVelocityY <= 0f)
        {   
            _rb.linearVelocityY += Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (_rb.linearVelocityY > 0 && !_jumpHeld)
        {
            _rb.linearVelocityY += Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
        
        if (_surroundingsCheck.WallDirection() != 0 && !_surroundingsCheck.IsGrounded())
        {
            _rb.linearVelocityY = Mathf.Max(_rb.linearVelocityY, -wallSlideSpeed);
        }
    }
    
    private void ApplyJump()
    {
        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, jumpForce);
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;

            _spriteManager.OnJump();
        }
        
        if (_jumpBufferCounter > 0f && !_surroundingsCheck.IsGrounded() && _surroundingsCheck.WallDirection() != 0 && _input.x * _surroundingsCheck.WallDirection() <= 0f)
        {
            _spriteManager.OnJump();
            
            _rb.linearVelocity = new Vector2(-_surroundingsCheck.WallDirection() * wallJumpForce.x, wallJumpForce.y);
            _isWallJumping = true;
            _jumpBufferCounter = 0f;

            StartCoroutine(FinishWallJump());
        }
    }

    private void CheckWallSliding()
    {
        _wallSliding = (_surroundingsCheck.WallDirection() != 0 && !_surroundingsCheck.IsGrounded());
    }
    
    private void Update()
    {
        CheckWallSliding();
        UpdateTimers();
        HandleDropThrough();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyJump();
    }

    public void ApplyRecoil(Vector2 speed, float recoilDuration)
    {
        if (!_isRecoiling)
            StartCoroutine(RecoilRoutine(speed, recoilDuration));
    }

    private IEnumerator RecoilRoutine(Vector2 speed, float recoilDuration)
    {
        _isRecoiling = true;

        _rb.linearVelocity = speed;

        yield return new WaitForSeconds(recoilDuration);

        _isRecoiling = false;
    }
}
