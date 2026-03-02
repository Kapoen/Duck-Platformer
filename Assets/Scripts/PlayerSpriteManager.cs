using System;
using UnityEngine;

public class PlayerSpriteManager : MonoBehaviour
{
    private static readonly int Respawn = Animator.StringToHash("Respawn");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Magnitude = Animator.StringToHash("Magnitude");
    private static readonly int YVelocity = Animator.StringToHash("yVelocity");
    private static readonly int Wall = Animator.StringToHash("OnWall");

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private int _movingDirection = 1;
    private float _maxFallSpeed;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public int FacingDirection()
    {
        return _spriteRenderer.flipX ? -1 : 1;
    }

    public void OnMove(Vector2 input)
    {
        _movingDirection = Math.Sign(input.x);

        Flip();
    }

    public void OnRespawn()
    {
        _animator.SetTrigger(Respawn);
    }

    public void OnDeath()
    {
        _animator.SetTrigger(Death);
    }

    public void OnJump()
    {
        _animator.SetTrigger(Jump);
    }

    public void UpdateSpeed(Vector2 movementSpeed)
    {
        _animator.SetFloat(Magnitude, movementSpeed.magnitude);
        _animator.SetFloat(YVelocity, movementSpeed.y);
    }

    public void OnWall(bool onWall, int wallDirection)
    {
        _animator.SetBool(Wall, onWall);
        
        switch (wallDirection)
        {
            case 1:
                _spriteRenderer.flipX = true;
                _movingDirection = -1;
                break;
            case -1:
                _spriteRenderer.flipX = false;
                _movingDirection = 1;
                break;
        }
    }

    public void Flip()
    {
        if (_movingDirection == 0)
        {
            return;
        }
        
        _spriteRenderer.flipX = (_movingDirection == -1);
    }
}
