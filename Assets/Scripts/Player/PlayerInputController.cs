namespace Player
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Handles the player input.
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        private PlayerController _playerController;
        private PlayerSpriteManager _spriteManager;
        private PlayerHonk _playerHonk;

        private Vector2 _lastMovementInput;

        /// <summary>
        /// Handles the movement input.
        /// </summary>
        /// <param name="context">The movement input interaction.</param>
        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            this._lastMovementInput = input;

            this._playerController.OnMove(input);
            this._spriteManager.OnMove(input);
        }

        /// <summary>
        /// Handles the jump input.
        /// </summary>
        /// <param name="context">The jump input interaction.</param>
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                this._playerController.SetJumpHeld(true);
            }
            else if (context.performed)
            {
                this._playerController.ResetJumpBufferCounter();
            }
            else if (context.canceled)
            {
                this._playerController.MultiplySpeed(new Vector2(1f, 0.5f));
                this._playerController.SetJumpHeld(false);
            }
        }

        /// <summary>
        /// Handles the honk input.
        /// </summary>
        /// <param name="context">The honk input interaction.</param>
        public void OnHonk(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                this._playerHonk.OnHonkStart();
            }

            if (context.canceled)
            {
                this._playerHonk.OnHonkCanceled();
            }
        }

        private void Start()
        {
            this._playerController = this.gameObject.GetComponent<PlayerController>();
            this._spriteManager = this.gameObject.GetComponent<PlayerSpriteManager>();
            this._playerHonk = this.gameObject.GetComponent<PlayerHonk>();
        }

        /// <summary>
        /// Constantly update the sprite manager.
        /// </summary>
        private void Update()
        {
            this._spriteManager.OnMove(this._lastMovementInput);
        }
    }
}
