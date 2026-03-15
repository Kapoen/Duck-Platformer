using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInputController : MonoBehaviour
    {
        private PlayerController _playerController;
        private PlayerSpriteManager _spriteManager;
        private PlayerHonk _playerHonk;

        private Vector2 _lastMovementInput;
    
        public void Start()
        {
            _playerController = gameObject.GetComponent<PlayerController>();
            _spriteManager = gameObject.GetComponent<PlayerSpriteManager>();
            _playerHonk = gameObject.GetComponent<PlayerHonk>();
        }

        /// <summary>
        /// Handles the movement input.
        /// </summary>
        /// <param name="context">The movement input interaction.</param>
        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            _lastMovementInput = input;
        
            _playerController.OnMove(input);
            _spriteManager.OnMove(input);
        }
    
        /// <summary>
        /// Handles the jump input.
        /// </summary>
        /// <param name="context">The jump input interaction.</param>
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _playerController.SetJumpHeld(true);
            }
            else if (context.performed)
            {
                _playerController.ResetJumpBufferCounter();
            }
            else if (context.canceled)
            {
                _playerController.MultiplySpeed(new Vector2(1f, 0.5f));
                _playerController.SetJumpHeld(false);
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
                _playerHonk.OnHonkStart();
            }

            if (context.canceled)
            {
                _playerHonk.OnHonkCanceled();
            }
        }

        /// <summary>
        /// Constantly update the sprite manager.
        /// </summary>
        private void Update()
        {
            _spriteManager.OnMove(_lastMovementInput);
        }
    }
}
