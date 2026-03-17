namespace Player
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Handles the sprite and animator of the player.
    /// </summary>
    public class PlayerSpriteManager : MonoBehaviour
    {
        private static readonly int Respawn = Animator.StringToHash("Respawn");
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int XVelocity = Animator.StringToHash("xVelocity");
        private static readonly int YVelocity = Animator.StringToHash("yVelocity");
        private static readonly int Wall = Animator.StringToHash("OnWall");

        private Animator _animator;
        private SpriteRenderer _spriteRenderer;

        private int _movingDirection = 1;
        private float _maxFallSpeed;

        /// <summary>
        /// Gets the facing direction.
        /// </summary>
        /// <returns>-1 if the sprite is facing left or 1 if the sprite is facing right.</returns>
        public int FacingDirection()
        {
            return this._spriteRenderer.flipX ? -1 : 1;
        }

        /// <summary>
        /// Handle the moving of the character.
        /// </summary>
        /// <param name="input">The movement input.</param>
        public void OnMove(Vector2 input)
        {
            this._movingDirection = Math.Sign(input.x);
        }

        /// <summary>
        /// Handle player respawning.
        /// </summary>
        public void OnRespawn()
        {
            this._animator.SetTrigger(Respawn);
        }

        /// <summary>
        /// Handle player dying.
        /// </summary>
        public void OnDeath()
        {
            this._animator.SetTrigger(Death);
        }

        /// <summary>
        /// Handle player jumping.
        /// </summary>
        public void OnJump()
        {
            this._animator.SetTrigger(Jump);
        }

        /// <summary>
        /// Update the speed variables in the animator.
        /// </summary>
        /// <param name="movementSpeed">The player's velocity</param>
        public void UpdateSpeed(Vector2 movementSpeed)
        {
            this._animator.SetFloat(XVelocity, Mathf.Abs(movementSpeed.x));
            this._animator.SetFloat(YVelocity, movementSpeed.y);
        }

        /// <summary>
        /// Handles being on wall.
        /// </summary>
        /// <param name="onWall">If the player is on a wall.</param>
        /// <param name="wallDirection">The direction of the wall.</param>
        public void OnWall(bool onWall, int wallDirection)
        {
            this._animator.SetBool(Wall, onWall);

            switch (wallDirection)
            {
                case 1:
                    this._spriteRenderer.flipX = true;
                    this._movingDirection = -1;
                    break;
                case -1:
                    this._spriteRenderer.flipX = false;
                    this._movingDirection = 1;
                    break;
            }
        }

        /// <summary>
        /// Flip the sprite, if needed.
        /// </summary>
        public void Flip()
        {
            if (this._movingDirection == 0)
            {
                return;
            }

            this._spriteRenderer.flipX = this._movingDirection == -1;
        }

        private void Awake()
        {
            this._animator = this.GetComponent<Animator>();
            this._spriteRenderer = this.GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Always check if the sprite needs to be flipped.
        /// </summary>
        private void Update()
        {
            this.Flip();
        }
    }
}
